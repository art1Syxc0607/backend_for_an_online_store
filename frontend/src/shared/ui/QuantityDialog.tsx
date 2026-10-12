import { useState, useEffect } from 'react'

interface Props {
    isOpen: boolean
    productName: string
    maxQuantity: number
    onConfirm: (quantity: number) => void
    onCancel: () => void
}

export default function QuantityDialog({
    isOpen,
    productName,
    maxQuantity,
    onConfirm,
    onCancel,
}: Props) {
    const [quantity, setQuantity] = useState(1)

    useEffect(() => {
        if (isOpen) setQuantity(1)
    }, [isOpen])

    if (!isOpen) return null

    const handleConfirm = () => {
        if (quantity >= 1 && quantity <= maxQuantity) {
            onConfirm(quantity)
        }
    }

    return (
        <div
            className="fixed inset-0 bg-black/50 flex items-center justify-center z-[100]"
            onClick={onCancel}
        >
            <div
                className="bg-white rounded-lg p-6 max-w-md w-full mx-4 shadow-xl"
                onClick={(e) => e.stopPropagation()}
            >
                <h3 className="text-xl font-bold mb-4">Добавить в корзину</h3>

                <p className="text-gray-700 mb-4">
                    <strong>{productName}</strong>
                </p>

                {/* Количество */}
                <div className="mb-4">
                    <label className="block text-sm font-medium mb-2">
                        Количество
                    </label>
                    <div className="flex items-center gap-2">
                        <button
                            onClick={() => setQuantity((q) => Math.max(1, q - 1))}
                            disabled={quantity <= 1}
                            className="w-10 h-10 rounded-lg bg-gray-200 hover:bg-gray-300 disabled:opacity-50 text-xl font-bold"
                        >
                            −
                        </button>
                        <input
                            type="number"
                            value={quantity}
                            onChange={(e) => {
                                const val = Number(e.target.value)
                                if (val >= 1 && val <= maxQuantity) setQuantity(val)
                            }}
                            min={1}
                            max={maxQuantity}
                            className="flex-1 px-4 py-2 border rounded-lg text-center focus:outline-none focus:ring-2 focus:ring-blue-500"
                        />
                        <button
                            onClick={() => setQuantity((q) => Math.min(maxQuantity, q + 1))}
                            disabled={quantity >= maxQuantity}
                            className="w-10 h-10 rounded-lg bg-gray-200 hover:bg-gray-300 disabled:opacity-50 text-xl font-bold"
                        >
                            +
                        </button>
                    </div>
                    <p className="text-xs text-gray-500 mt-1">
                        Максимум: {maxQuantity} шт.
                    </p>
                </div>

                {/* Кнопки */}
                <div className="flex gap-2">
                    <button
                        onClick={onCancel}
                        className="flex-1 px-4 py-2 bg-gray-200 rounded-lg hover:bg-gray-300"
                    >
                        Отмена
                    </button>
                    <button
                        onClick={handleConfirm}
                        className="flex-1 px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                    >
                        Добавить
                    </button>
                </div>
            </div>
        </div>
    )
}