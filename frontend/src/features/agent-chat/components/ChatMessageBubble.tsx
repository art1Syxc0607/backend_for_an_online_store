import type { ChatMessageDto } from '../api/agentApi'

interface Props {
    message: ChatMessageDto
}

export default function ChatMessageBubble({ message }: Props) {
    // ✅ Нормализация: приводим к нижнему регистру
    const role = (message.role ?? '').toString().toLowerCase()
    const isUser = role === 'user'

    return (
        <div className={`flex ${isUser ? 'justify-end' : 'justify-start'}`}>
            <div
                className={`
                    max-w-[85%] px-3 py-2 rounded-2xl text-sm
                    ${isUser
                        ? 'bg-sky-100 text-black rounded-br-sm'
                        : 'bg-white text-black border rounded-bl-sm shadow-sm'
                    }
                `}
            >
                <div className="whitespace-pre-wrap break-words">
                    {message.text}
                </div>
                <div
                    className={`text-xs mt-1 ${isUser ? 'text-gray-500' : 'text-gray-400'
                        }`}
                >
                    {new Date(message.createdAt).toLocaleTimeString('ru-RU', {
                        hour: '2-digit',
                        minute: '2-digit',
                    })}
                </div>
            </div>
        </div>
    )
}