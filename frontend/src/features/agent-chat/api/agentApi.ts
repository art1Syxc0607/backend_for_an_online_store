import { apiClient } from '@/shared/api/client'

export interface ChatRequest {
    message: string
    sessionId?: string
}

export interface ChatResponse {
    reply: string
    sessionId: string
    messageCount: number
    isGuest: boolean
    expiresAt?: string
    daysUntilExpiry?: number
}

export interface ConversationDto {
    sessionId: string
    title?: string
    messageCount: number
    createdAt: string
    updatedAt: string
    lastMessageAt?: string
}

export interface ChatMessageDto {
    role: string
    text: string
    createdAt: string
}

export interface ConversationDetailDto extends ConversationDto {
    messages: ChatMessageDto[]
}

// ✅ Отправить сообщение
export async function sendMessage(
    message: string,
    sessionId?: string
): Promise<ChatResponse> {
    const { data } = await apiClient.post<ChatResponse>('/agent/chat', {
        message,
        sessionId,
    })
    return data
}

// ✅ Список диалогов
export async function getConversations(): Promise<ConversationDto[]> {
    const { data } = await apiClient.get<ConversationDto[]>('/agent/conversations')
    return data
}

// ✅ Детали диалога
export async function getConversation(
    sessionId: string
): Promise<ConversationDetailDto> {
    const { data } = await apiClient.get<ConversationDetailDto>(
        `/agent/conversations/${sessionId}`
    )
    return data
}

// ✅ Переименовать
export async function changeTitle(
    sessionId: string,
    newTitle: string
): Promise<void> {
    await apiClient.put(`/agent/conversations/${sessionId}/title`, { newTitle })
}

// ✅ Удалить
export async function deleteConversation(sessionId: string): Promise<void> {
    await apiClient.delete(`/agent/conversations/${sessionId}`)
}