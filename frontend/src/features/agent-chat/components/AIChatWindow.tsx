import { useEffect, useRef, useState } from 'react'
import { useConversations, useConversation, useChatActions } from '../hooks/useChat'
import ConversationSidebar from './ConversationSidebar'
import ChatMessageBubble from './ChatMessageBubble'
import type { ChatMessageDto } from '../api/agentApi'

interface Props {
    onClose: () => void
    onDragStart: (e: React.MouseEvent) => void
}

export default function AIChatWindow({ onClose, onDragStart }: Props) {
    const [sidebarOpen, setSidebarOpen] = useState(false)
    const [activeSessionId, setActiveSessionId] = useState<string>()
    const [input, setInput] = useState('')

    // ✅ Локальные сообщения (для optimistic UI)
    const [localMessages, setLocalMessages] = useState<ChatMessageDto[]>([])

    // ✅ Флаг: ждём ответ агента
    const [isWaitingForAgent, setIsWaitingForAgent] = useState(false)

    const { data: conversations = [] } = useConversations()
    const { data: currentConversation, isLoading: loadingConversation } =
        useConversation(activeSessionId)
    const { sendMessage, isSending } = useChatActions()

    const messagesEndRef = useRef<HTMLDivElement>(null)

    // ✅ Все сообщения: из БД + локальные (optimistic)
    const serverMessages = currentConversation?.messages ?? []
    const allMessages = [...serverMessages, ...localMessages]

    // ✅ Скролл вниз
    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
    }, [allMessages.length, isWaitingForAgent])

    // ✅ Когда ответ пришёл — очищаем локальные
    useEffect(() => {
        if (currentConversation && localMessages.length > 0) {
            // ✅ Проверяем, что все локальные сообщения уже в БД
            const lastLocalText = localMessages[localMessages.length - 1]?.text
            const found = serverMessages.some(
                (m) => m.text === lastLocalText && m.role?.toLowerCase() === 'user'
            )
            if (found) {
                setLocalMessages([])
            }
        }
    }, [currentConversation, serverMessages, localMessages])

    const handleSend = async () => {
        if (!input.trim() || isSending) return

        const messageText = input.trim()
        setInput('')

        // ✅ 1. Сразу добавляем сообщение пользователя (optimistic)
        const optimisticMessage: ChatMessageDto = {
            role: 'User',
            text: messageText,
            createdAt: new Date().toISOString(),
        }
        setLocalMessages((prev) => [...prev, optimisticMessage])

        // ✅ 2. Показываем анимацию ожидания
        setIsWaitingForAgent(true)

        try {
            const response = await sendMessage({
                message: messageText,
                sessionId: activeSessionId,
            })

            // ✅ 3. Если новый диалог — запоминаем sessionId
            if (!activeSessionId) {
                setActiveSessionId(response.sessionId)
            }
            // ✅ 4. После ответа — данные придут из useConversation
        } catch (error) {
            console.error('Failed to send:', error)
            // ✅ Возвращаем текст в input + убираем optimistic
            setInput(messageText)
            setLocalMessages((prev) => prev.slice(0, -1))
        } finally {
            setIsWaitingForAgent(false)
        }
    }

    const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault()
            handleSend()
        }
    }

    const handleSelectConversation = (sessionId: string) => {
        setActiveSessionId(sessionId)
        setLocalMessages([])  // ✅ очищаем локальные
        setSidebarOpen(false)
    }

    const handleNewChat = () => {
        setActiveSessionId(undefined)
        setLocalMessages([])  // ✅ очищаем локальные
        setSidebarOpen(false)
    }

    return (
        <div className="flex h-full w-full">
            <ConversationSidebar
                isOpen={sidebarOpen}
                conversations={conversations}
                activeSessionId={activeSessionId}
                onSelect={handleSelectConversation}
                onNewChat={handleNewChat}
                onClose={() => setSidebarOpen(false)}
            />

            <div className="flex-1 flex flex-col min-w-0">
                {/* Header */}
                <div
                    onMouseDown={onDragStart}
                    className="flex items-center justify-between px-3 py-2 border-b bg-gradient-to-r from-blue-500 to-purple-600 text-white cursor-move select-none"
                >
                    <div className="flex items-center gap-2">
                        <button
                            onClick={() => setSidebarOpen((v) => !v)}
                            className="p-1.5 hover:bg-white/20 rounded transition cursor-pointer"
                        >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                            </svg>
                        </button>

                        <span className="text-xl">🤖</span>
                        <div className="min-w-0">
                            <div className="font-bold text-sm truncate">
                                {currentConversation?.title || 'AI Ассистент'}
                            </div>
                            <div className="text-xs opacity-80">
                                {allMessages.length > 0
                                    ? `${allMessages.length} сообщений`
                                    : 'Онлайн'}
                            </div>
                        </div>
                    </div>

                    <button
                        onClick={onClose}
                        className="p-1.5 hover:bg-white/20 rounded transition cursor-pointer"
                    >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                {/* ✅ Messages */}
                <div className="flex-1 overflow-y-auto p-3 space-y-3 bg-gray-50">
                    {/* ✅ Заставка — ТОЛЬКО если НЕТ ни серверных, ни локальных */}
                    {allMessages.length === 0 && !loadingConversation ? (
                        <div className="text-center py-8">
                            <div className="text-5xl mb-3">👋</div>
                            <h3 className="font-bold mb-1">Привет! Я AI Ассистент</h3>
                            <p className="text-sm text-gray-500 max-w-xs mx-auto">
                                Могу помочь с выбором товаров, ответить на вопросы о заказах
                            </p>
                            <div className="mt-4 space-y-2 max-w-xs mx-auto">
                                {[
                                    'Какие есть смартфоны?',
                                    'Покажи ноутбуки до 100000 ₽',
                                    'Что купить в подарок?',
                                ].map((hint) => (
                                    <button
                                        key={hint}
                                        onClick={() => setInput(hint)}
                                        className="w-full text-left text-sm px-3 py-2 bg-white border rounded-lg hover:border-blue-500 hover:bg-blue-50 transition"
                                    >
                                        💡 {hint}
                                    </button>
                                ))}
                            </div>
                        </div>
                    ) : (
                        <>
                            {allMessages.map((msg, idx) => (
                                <ChatMessageBubble key={idx} message={msg} />
                            ))}

                            {/* ✅ Анимация ожидания ответа */}
                            {isWaitingForAgent && (
                                <div className="flex justify-start">
                                    <div className="bg-white border rounded-2xl rounded-bl-sm px-4 py-3 shadow-sm">
                                        <div className="flex gap-1 items-center">
                                            <span
                                                className="w-2 h-2 bg-gray-400 rounded-full animate-bounce"
                                                style={{ animationDelay: '0ms' }}
                                            />
                                            <span
                                                className="w-2 h-2 bg-gray-400 rounded-full animate-bounce"
                                                style={{ animationDelay: '150ms' }}
                                            />
                                            <span
                                                className="w-2 h-2 bg-gray-400 rounded-full animate-bounce"
                                                style={{ animationDelay: '300ms' }}
                                            />
                                        </div>
                                    </div>
                                </div>
                            )}
                        </>
                    )}
                    <div ref={messagesEndRef} />
                </div>

                {/* Input */}
                <div className="border-t p-2 bg-white">
                    <div className="flex items-end gap-2">
                        <textarea
                            value={input}
                            onChange={(e) => setInput(e.target.value)}
                            onKeyDown={handleKeyDown}
                            placeholder="Напишите сообщение..."
                            rows={1}
                            disabled={isSending || isWaitingForAgent}
                            className="flex-1 px-3 py-2 border rounded-lg resize-none focus:outline-none focus:ring-2 focus:ring-blue-500 text-sm max-h-24"
                            style={{ minHeight: '40px' }}
                        />
                        <button
                            onClick={handleSend}
                            disabled={!input.trim() || isSending || isWaitingForAgent}
                            className="p-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed transition"
                        >
                            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 19l9 2-9-18-9 18 9-2zm0 0v-8" />
                            </svg>
                        </button>
                    </div>
                </div>
            </div>
        </div>
    )
}