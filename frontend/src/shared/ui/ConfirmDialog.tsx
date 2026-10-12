interface Props {
    isOpen: boolean
    title: string
    message?: string
    confirmLabel?: string
    cancelLabel?: string
    onConfirm: () => void
    onCancel: () => void
    isLoading?: boolean
}

export default function ConfirmDialog({
    isOpen,
    title,
    message,
    confirmLabel = 'Yes',
    cancelLabel = 'No',
    onConfirm,
    onCancel,
    isLoading = false,
}: Props) {
    if (!isOpen) return null

    return (
        <div
            className="fixed inset-0 z-[200] flex items-center justify-center bg-black/50 backdrop-blur-sm"
            onClick={onCancel}
        >
            <div
                className="bg-white rounded-lg shadow-2xl p-6 max-w-md w-full mx-4"
                onClick={(e) => e.stopPropagation()}
            >
                <h3 className="text-xl font-bold text-gray-900 mb-2">
                    {title}
                </h3>

                {message && (
                    <p className="text-gray-600 mb-6">{message}</p>
                )}

                <div className="flex gap-3 mt-6">
                    <button
                        onClick={onConfirm}
                        disabled={isLoading}
                        className="flex-1 px-4 py-2.5 bg-green-500 text-white rounded-lg hover:bg-green-600 disabled:opacity-50 disabled:cursor-not-allowed font-medium transition"
                    >
                        {isLoading ? '...' : confirmLabel}
                    </button>
                    <button
                        onClick={onCancel}
                        disabled={isLoading}
                        className="flex-1 px-4 py-2.5 bg-red-500 text-white rounded-lg hover:bg-red-600 disabled:opacity-50 disabled:cursor-not-allowed font-medium transition"
                    >
                        {cancelLabel}
                    </button>
                </div>
            </div>
        </div>
    )
}