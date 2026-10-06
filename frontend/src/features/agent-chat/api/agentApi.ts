import { apiClient } from '@/shared/api/client'

export interface ChatRequest {
    message: string
    conversationId?: string
}

export interface ChatResponse {
    reply: string
    conversationId: string
    messageCount: number
    isGuest: boolean
    expiresAt?: string
}

export interface ConversationDto {
    sessionId: string
    title?: string
    messageCount: number
    createdAt: string
    updatedAt: string
    lastMessageAt?: string
}

export interface ConversationDetailDto extends ConversationDto {
    messages: ChatMessageDto[]
}

export interface ChatMessageDto {
    role: string
    text: string
    createdAt: string
}

export async function sendMessage(request: ChatRequest): Promise<ChatResponse> {
    const { data } = await apiClient.post<ChatResponse>('/agent/chat', request)
    return data
}

export async function getConversations(): Promise<ConversationDto[]> {
    const { data } = await apiClient.get<ConversationDto[]>('/agent/conversations')
    return data
}

export async function getConversation(sessionId: string): Promise<ConversationDetailDto> {
    const { data } = await apiClient.get<ConversationDetailDto>(`/agent/conversations/${sessionId}`)
    return data
}

export async function deleteConversation(sessionId: string): Promise<void> {
    await apiClient.delete(`/agent/conversations/${sessionId}`)
}