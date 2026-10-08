import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { sendMessage, getConversations, getConversation } from '../api/agentApi'
import type { ChatRequest } from '../api/agentApi'  // ✅

export function useChat() {
    const queryClient = useQueryClient()

    const conversationsQuery = useQuery({
        queryKey: ['agent', 'conversations'],
        queryFn: getConversations,
    })

    const sendMessageMutation = useMutation({
        mutationFn: (request: ChatRequest) => sendMessage(request),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['agent', 'conversations'] })
        },
    })

    return {
        conversations: conversationsQuery.data ?? [],
        isLoading: conversationsQuery.isLoading,
        sendMessage: sendMessageMutation.mutate,
        isSending: sendMessageMutation.isPending,
        error: sendMessageMutation.error,
    }
}

export function useConversation(sessionId: string) {
    return useQuery({
        queryKey: ['agent', 'conversation', sessionId],
        queryFn: () => getConversation(sessionId),
        enabled: !!sessionId,
    })
}