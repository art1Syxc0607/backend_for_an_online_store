import { ReactNode, useEffect } from 'react'

interface Props {
    isOpen: boolean
    title: string
    onClose: () => void
    children: ReactNode
    footer?: ReactNode
    maxWidth?: 'sm' | 'md' | 'lg' | 'xl'
}

export default function AdminModal({
    isOpen,
    title,
    onClose,
    children,
    footer,
    maxWidth = 'md',
}: Props) {
    // ✅ Escape закрывает
    useEffect(() => {
        const handleKey = (e: KeyboardEvent) => {
            if (e.key === 'Escape' && isOpen) onClose()
        }
        document.addEventListener('keydown', handleKey)
        return () => document.removeEventListener('keydown', handleKey)
    }, [isOpen, onClose])

    if (!isOpen) return null

    const widths = {
        sm: 'max-w-md',
        md: 'max-w-2xl',
        lg: 'max-w-4xl',
        xl: 'max-w-6xl',
    }

    return (
        <div
            className="fixed inset-0 z-[200] flex items-center justify-center bg-black/50 backdrop-blur-sm p-4"
            onClick={onClose}
        >
            <div
                className={`bg-white rounded-lg shadow-2xl w-full ${widths[maxWidth]} max-h-[90vh] flex flex-col`}
                onClick={(e) => e.stopPropagation()}
            >
                {/* Header */}
                <div className="flex items-center justify-between p-4 border-b">
                    <h2 className="text-xl font-bold">{title}</h2>
                    <button
                        onClick={onClose}
                        className="p-1.5 hover:bg-gray-100 rounded text-gray-500 hover:text-gray-700 transition"
                    >
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                </div>

                {/* Body */}
                <div className="flex-1 overflow-y-auto p-6">
                    {children}
                </div>

                {/* Footer */}
                {footer && (
                    <div className="border-t p-4 bg-gray-50">
                        {footer}
                    </div>
                )}
            </div>
        </div>
    )
}