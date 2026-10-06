import { useState } from 'react'
import { useChat, useConversation } from '../hooks/useChat'

export default function ChatPage() {
    const [message, setMessage] = useState('')
    const [activeSessionId, setActiveSessionId] = useState<string>()
    const { conversations, sendMessage, isSending } = useChat()
    const { data: conversationDetail } = useConversation(activeSessionId ?? '')

    const handleSend = () => {
        if (!message.trim()) return
        sendMessage(
            { message, conversationId: activeSessionId },
            {
                onSuccess: (data) => {
                    setActiveSessionId(data.conversationId)
                    setMessage('')
                },
            }
        )
    }

    const messages = conversationDetail?.messages ?? []

    return (
        <div className="flex h-[calc(100vh-200px)] gap-4">
            {/* Sidebar: ������ �������� */}
            <aside className="w-64 bg-white rounded-lg shadow p-4 overflow-y-auto">
                <h2 className="font-bold mb-4">�������</h2>
                <button
                    onClick={() => setActiveSessionId(undefined)}
                    className="w-full mb-4 px-3 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                >
                    + ����� ���
                </button>
                <ul className="space-y-2">
                    {conversations.map((conv) => (
                        <li key={conv.sessionId}>
                            <button
                                onClick={() => setActiveSessionId(conv.sessionId)}
                                className={`w-full text-left px-3 py-2 rounded ${activeSessionId === conv.sessionId ? 'bg-blue-100' : 'hover:bg-gray-100'
                                    }`}
                            >
                                <div className="font-medium truncate">{conv.title ?? '����� ������'}</div>
                                <div className="text-xs text-gray-500">{conv.messageCount} ���������</div>
                            </button>
                        </li>
                    ))}
                </ul>
            </aside>

            {/* Chat area */}
            <div className="flex-1 flex flex-col bg-white rounded-lg shadow">
                <div className="flex-1 overflow-y-auto p-4 space-y-4">
                    {messages.map((msg, idx) => (
                        <div
                            key={idx}
                            className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}
                        >
                            <div
                                className={`max-w-[70%] px-4 py-2 rounded-lg ${msg.role === 'user'
                                        ? 'bg-blue-500 text-white'
                                        : 'bg-gray-100 text-gray-900'
                                    }`}
                            >
                                {msg.text}
                            </div>
                        </div>
                    ))}
                </div>

                <div className="border-t p-4 flex gap-2">
                    <input
                        type="text"
                        value={message}
                        onChange={(e) => setMessage(e.target.value)}
                        onKeyDown={(e) => e.key === 'Enter' && handleSend()}
                        placeholder="������� ���������..."
                        className="flex-1 px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                        disabled={isSending}
                    />
                    <button
                        onClick={handleSend}
                        disabled={isSending || !message.trim()}
                        className="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50"
                    >
                        {isSending ? '...' : '���������'}
                    </button>
                </div>
            </div>
        </div>
    )
}