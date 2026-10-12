import { useState, useRef, useEffect } from 'react'
import { useChatActions } from '../hooks/useChat'
import type { ConversationDto } from '../api/agentApi'   // ✅ import type

interface Props {
    conversation: ConversationDto
    onClose: () => void
    onDeleted: () => void
}

export default function ConversationItemMenu({
    conversation,
    onClose,
    onDeleted,
}: Props) {
    const { renameConversation, deleteConversation, isRenaming, isDeleting } =
        useChatActions()
    const [mode, setMode] = useState<'menu' | 'rename' | 'delete'>('menu')
    const [newTitle, setNewTitle] = useState(conversation.title ?? '')
    const menuRef = useRef<HTMLDivElement>(null)

    // ✅ Закрытие при клике вне
    useEffect(() => {
        const handleClick = (e: MouseEvent) => {
            if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
                onClose()
            }
        }
        document.addEventListener('mousedown', handleClick)
        return () => document.removeEventListener('mousedown', handleClick)
    }, [onClose])

    const handleRename = async () => {
        if (!newTitle.trim() || newTitle === conversation.title) {
            onClose()
            return
        }
        try {
            await renameConversation({
                sessionId: conversation.sessionId,
                title: newTitle.trim(),
            })
            onClose()
        } catch (error) {
            console.error('Failed to rename:', error)
        }
    }

    const handleDelete = async () => {
        try {
            await deleteConversation(conversation.sessionId)
            onDeleted()
        } catch (error) {
            console.error('Failed to delete:', error)
        }
    }

    // ═══════════════════════════════════════════
    // Меню
    // ═══════════════════════════════════════════
    if (mode === 'menu') {
        return (
            <div
                ref={menuRef}
                className="absolute right-1 top-10 z-30 bg-white border rounded-lg shadow-lg py-1 min-w-[150px]"
            >
                <button
                    onClick={(e) => {
                        e.stopPropagation()
                        setMode('rename')
                    }}
                    className="w-full text-left px-3 py-2 text-sm hover:bg-gray-100 flex items-center gap-2"
                >
                    ✏️ Переименовать
                </button>
                <button
                    onClick={(e) => {
                        e.stopPropagation()
                        setMode('delete')
                    }}
                    className="w-full text-left px-3 py-2 text-sm hover:bg-red-50 text-red-600 flex items-center gap-2"
                >
                    🗑 Удалить
                </button>
            </div>
        )
    }

    // ═══════════════════════════════════════════
    // Переименование
    // ═══════════════════════════════════════════
    if (mode === 'rename') {
        return (
            <div
                ref={menuRef}
                className="absolute right-1 top-10 z-30 bg-white border rounded-lg shadow-lg p-2 min-w-[200px]"
            >
                <input
                    type="text"
                    value={newTitle}
                    onChange={(e) => setNewTitle(e.target.value)}
                    onKeyDown={(e) => {
                        if (e.key === 'Enter') handleRename()
                        if (e.key === 'Escape') onClose()
                    }}
                    maxLength={200}
                    autoFocus
                    placeholder="Новое название"
                    className="w-full px-2 py-1.5 border rounded text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
                <div className="flex gap-1 mt-2">
                    <button
                        onClick={handleRename}
                        disabled={isRenaming}
                        className="flex-1 px-2 py-1 bg-blue-500 text-white rounded text-xs hover:bg-blue-600 disabled:opacity-50"
                    >
                        {isRenaming ? '...' : 'OK'}
                    </button>
                    <button
                        onClick={onClose}
                        className="flex-1 px-2 py-1 bg-gray-200 rounded text-xs hover:bg-gray-300"
                    >
                        Отмена
                    </button>
                </div>
            </div>
        )
    }

    // ═══════════════════════════════════════════
    // Удаление
    // ═══════════════════════════════════════════
    return (
        <div
            ref={menuRef}
            className="absolute right-1 top-10 z-30 bg-white border rounded-lg shadow-lg p-3 min-w-[220px]"
        >
            <p className="text-sm mb-3">Удалить этот диалог?</p>
            <div className="flex gap-2">
                <button
                    onClick={handleDelete}
                    disabled={isDeleting}
                    className="flex-1 px-3 py-1.5 bg-red-500 text-white rounded text-sm hover:bg-red-600 disabled:opacity-50"
                >
                    {isDeleting ? '...' : 'Удалить'}
                </button>
                <button
                    onClick={onClose}
                    className="flex-1 px-3 py-1.5 bg-gray-200 rounded text-sm hover:bg-gray-300"
                >
                    Отмена
                </button>
            </div>
        </div>
    )
}