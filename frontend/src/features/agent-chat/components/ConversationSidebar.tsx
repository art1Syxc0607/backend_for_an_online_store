import { useState } from 'react'
import { useChatActions } from '../hooks/useChat'
import ConversationItemMenu from './ConversationItemMenu'
import type { ConversationDto } from '../api/agentApi'   // ✅ import type

interface Props {
    isOpen: boolean
    conversations: ConversationDto[]
    activeSessionId?: string
    onSelect: (sessionId: string) => void
    onNewChat: () => void
    onClose: () => void
}

export default function ConversationSidebar({
    isOpen,
    conversations,
    activeSessionId,
    onSelect,
    onNewChat,
    onClose,
}: Props) {
    const [menuSessionId, setMenuSessionId] = useState<string>()

    return (
        <>
            {/* Overlay */}
            {isOpen && (
                <div
                    className="absolute inset-0 bg-black/30 z-10"
                    onClick={onClose}
                />
            )}

            {/* Sidebar */}
            <div
                className={`
                    absolute top-0 left-0 h-full w-64 bg-white z-20
                    transform transition-transform duration-300
                    flex flex-col border-r
                    ${isOpen ? 'translate-x-0' : '-translate-x-full'}
                `}
            >
                {/* Header */}
                <div className="p-3 border-b bg-gray-50">
                    <div className="flex items-center justify-between mb-2">
                        <h3 className="font-bold text-sm">Диалоги</h3>
                        <button
                            onClick={onClose}
                            className="p-1 hover:bg-gray-200 rounded"
                        >
                            <svg
                                className="w-4 h-4"
                                fill="none"
                                stroke="currentColor"
                                viewBox="0 0 24 24"
                            >
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth={2}
                                    d="M6 18L18 6M6 6l12 12"
                                />
                            </svg>
                        </button>
                    </div>

                    {/* New chat */}
                    <button
                        onClick={onNewChat}
                        className="w-full px-3 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 text-sm font-medium transition"
                    >
                        + Новый чат
                    </button>
                </div>

                {/* List */}
                <div className="flex-1 overflow-y-auto">
                    {conversations.length === 0 ? (
                        <div className="text-center py-8 text-gray-400 text-sm">
                            Нет диалогов
                        </div>
                    ) : (
                        conversations.map((conv) => (
                            <div
                                key={conv.sessionId}
                                className={`relative group border-b ${activeSessionId === conv.sessionId
                                        ? 'bg-blue-50'
                                        : 'hover:bg-gray-50'
                                    }`}
                            >
                                <button
                                    onClick={() => onSelect(conv.sessionId)}
                                    className="w-full text-left px-3 py-2 pr-8"
                                >
                                    <div className="font-medium text-sm truncate">
                                        {conv.title || 'Новый диалог'}
                                    </div>
                                    <div className="text-xs text-gray-500 mt-0.5">
                                        {conv.messageCount} сообщений
                                    </div>
                                    {conv.lastMessageAt && (
                                        <div className="text-xs text-gray-400 mt-0.5">
                                            {new Date(conv.lastMessageAt).toLocaleDateString(
                                                'ru-RU',
                                                {
                                                    day: 'numeric',
                                                    month: 'short',
                                                    hour: '2-digit',
                                                    minute: '2-digit',
                                                }
                                            )}
                                        </div>
                                    )}
                                </button>

                                {/* ✅ Кнопка "..." */}
                                <button
                                    onClick={(e) => {
                                        e.stopPropagation()
                                        setMenuSessionId(
                                            menuSessionId === conv.sessionId
                                                ? undefined
                                                : conv.sessionId
                                        )
                                    }}
                                    className="absolute right-1 top-2 p-1 opacity-0 group-hover:opacity-100 hover:bg-gray-200 rounded transition"
                                    title="Действия"
                                >
                                    <svg
                                        className="w-4 h-4"
                                        fill="currentColor"
                                        viewBox="0 0 24 24"
                                    >
                                        <circle cx="12" cy="5" r="2" />
                                        <circle cx="12" cy="12" r="2" />
                                        <circle cx="12" cy="19" r="2" />
                                    </svg>
                                </button>

                                {/* ✅ Меню */}
                                {menuSessionId === conv.sessionId && (
                                    <ConversationItemMenu
                                        conversation={conv}
                                        onClose={() => setMenuSessionId(undefined)}
                                        onDeleted={() => {
                                            setMenuSessionId(undefined)
                                            if (activeSessionId === conv.sessionId) {
                                                onNewChat()
                                            }
                                        }}
                                    />
                                )}
                            </div>
                        ))
                    )}
                </div>
            </div>
        </>
    )
}