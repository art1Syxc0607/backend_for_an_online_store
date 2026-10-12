import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
    sendMessage,
    getConversations,
    getConversation,
    changeTitle,
    deleteConversation,
} from '../api/agentApi'
import type {
    ChatMessageDto,
    ConversationDetailDto,
} from '../api/agentApi'

const CONVERSATIONS_KEY = ['agent', 'conversations']

export function useConversations() {
    return useQuery({
        queryKey: CONVERSATIONS_KEY,
        queryFn: getConversations,
    })
}

export function useConversation(sessionId?: string) {
    return useQuery({
        queryKey: ['agent', 'conversation', sessionId],
        queryFn: () => getConversation(sessionId!),
        enabled: !!sessionId,
    })
}

export function useChatActions() {
    const queryClient = useQueryClient()

    // ✅ Отправка сообщения с optimistic updates
    const sendMutation = useMutation({
        mutationFn: ({ message, sessionId }: { message: string; sessionId?: string }) =>
            sendMessage(message, sessionId),

        // ✅ ДО запроса — добавляем сообщение пользователя + placeholder
        onMutate: async ({ message, sessionId }) => {
            // Сохраняем предыдущее состояние
            const previousConversation = queryClient.getQueryData<ConversationDetailDto>(
                ['agent', 'conversation', sessionId]
            )

            if (sessionId) {
                // ✅ Optimistic: сообщение пользователя
                const userMessage: ChatMessageDto = {
                    role: 'user',
                    text: message,
                    createdAt: new Date().toISOString(),
                }

                queryClient.setQueryData<ConversationDetailDto>(
                    ['agent', 'conversation', sessionId],
                    (old) => {
                        if (!old) return old
                        return {
                            ...old,
                            messages: [...old.messages, userMessage],
                        }
                    }
                )
            }

            return { previousConversation, sessionId }
        },

        // ✅ После успеха — обновляем данные с сервера
        onSuccess: (data, variables, context) => {
            // ✅ Если новый диалог — инвалидируем список
            queryClient.invalidateQueries({ queryKey: CONVERSATIONS_KEY })

            // ✅ Загружаем актуальные сообщения
            if (data.sessionId) {
                queryClient.invalidateQueries({
                    queryKey: ['agent', 'conversation', data.sessionId],
                })
            }
        },

        // ✅ При ошибке — возвращаем предыдущее состояние
        onError: (error, variables, context) => {
            if (context?.previousConversation) {
                queryClient.setQueryData(
                    ['agent', 'conversation', context.sessionId],
                    context.previousConversation
                )
            }
        },
    })

    const renameMutation = useMutation({
        mutationFn: ({ sessionId, title }: { sessionId: string; title: string }) =>
            changeTitle(sessionId, title),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: CONVERSATIONS_KEY })
        },
    })

    const deleteMutation = useMutation({
        mutationFn: (sessionId: string) => deleteConversation(sessionId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: CONVERSATIONS_KEY })
        },
    })

    return {
        sendMessage: sendMutation.mutateAsync,
        isSending: sendMutation.isPending,

        renameConversation: renameMutation.mutateAsync,
        isRenaming: renameMutation.isPending,

        deleteConversation: deleteMutation.mutateAsync,
        isDeleting: deleteMutation.isPending,
    }
}