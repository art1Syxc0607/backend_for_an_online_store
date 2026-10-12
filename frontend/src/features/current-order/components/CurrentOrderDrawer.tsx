import { useState } from 'react'
import { Link, useNavigate } from 'react-router'
import { useCurrentOrder } from '../context/CurrentOrderContext'
import { createOrder } from '@/features/orders/api/orderApi'

export default function CurrentOrderDrawer() {
    const {
        items,
        isOpen,
        closeDrawer,
        updateQuantity,
        removeItem,
        clearOrder,
        totalItems,
        totalPrice,
    } = useCurrentOrder()

    const navigate = useNavigate()
    const [showCheckout, setShowCheckout] = useState(false)
    const [address, setAddress] = useState('')
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [error, setError] = useState<string | null>(null)

    const handleCheckout = async (e: React.FormEvent) => {
        e.preventDefault()
        setError(null)
        setIsSubmitting(true)

        try {
            const orderId = await createOrder({
                items: items.map((i) => ({
                    productId: i.productId,
                    quantity: i.quantity,
                })),
                shippingAddress: address,
            })

            clearOrder()
            closeDrawer()
            setShowCheckout(false)
            setAddress('')

            // ✅ Переход на страницу заказа
            navigate(`/orders/${orderId}`)
        } catch (err: any) {
            setError(
                err?.response?.data?.error ??
                err?.response?.data?.message ??
                'Ошибка при оформлении заказа'
            )
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <>
            {/* Backdrop */}
            {isOpen && (
                <div
                    className="fixed inset-0 bg-black/50 z-[90]"
                    onClick={closeDrawer}
                />
            )}

            {/* Drawer */}
            <div
                className={`fixed top-0 right-0 h-full w-full max-w-md bg-white shadow-xl z-[95] transform transition-transform duration-300 ${isOpen ? 'translate-x-0' : 'translate-x-full'
                    }`}
            >
                <div className="flex flex-col h-full">
                    {/* Header */}
                    <div className="flex items-center justify-between p-4 border-b">
                        <h2 className="text-xl font-bold">
                            📦 Текущий заказ {totalItems > 0 ? `(${totalItems})` : ''}
                        </h2>
                        <button
                            onClick={closeDrawer}
                            className="text-gray-500 hover:text-gray-700 text-2xl w-8 h-8 flex items-center justify-center"
                        >
                            ×
                        </button>
                    </div>

                    {/* Body */}
                    <div className="flex-1 overflow-y-auto p-4">
                        {items.length === 0 ? (
                            <div className="text-center py-12">
                                <div className="text-6xl mb-4">📦</div>
                                <p className="text-gray-500 mb-4">Заказ пуст</p>
                                <p className="text-xs text-gray-400 mb-4">
                                    Добавьте товары, чтобы оформить заказ
                                </p>
                                <button
                                    onClick={closeDrawer}
                                    className="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                                >
                                    Перейти к покупкам
                                </button>
                            </div>
                        ) : (
                            <div className="space-y-3">
                                {items.map((item) => (
                                    <div
                                        key={item.productId}
                                        className="flex gap-3 p-3 bg-gray-50 rounded-lg"
                                    >
                                        {/* Фото */}
                                        <div className="w-16 h-16 bg-gray-200 rounded overflow-hidden flex-shrink-0">
                                            {item.productImage ? (
                                                <img
                                                    src={item.productImage}
                                                    alt={item.productName}
                                                    className="w-full h-full object-cover"
                                                />
                                            ) : (
                                                <div className="w-full h-full flex items-center justify-center text-gray-400 text-xs">
                                                    Нет фото
                                                </div>
                                            )}
                                        </div>

                                        {/* Инфо */}
                                        <div className="flex-1 min-w-0">
                                            <Link
                                                to={`/products/${item.productId}`}
                                                onClick={closeDrawer}
                                                className="font-medium text-gray-900 hover:text-blue-600 line-clamp-2 text-sm"
                                            >
                                                {item.productName}
                                            </Link>

                                            <div className="text-sm text-blue-600 font-bold mt-1">
                                                {item.price.toLocaleString('ru-RU')} ₽
                                            </div>

                                            {/* Количество */}
                                            <div className="flex items-center gap-2 mt-2">
                                                <button
                                                    onClick={() =>
                                                        updateQuantity(
                                                            item.productId,
                                                            item.quantity - 1
                                                        )
                                                    }
                                                    className="w-7 h-7 rounded bg-gray-200 hover:bg-gray-300 text-sm font-bold"
                                                >
                                                    −
                                                </button>
                                                <span className="text-sm w-8 text-center font-medium">
                                                    {item.quantity}
                                                </span>
                                                <button
                                                    onClick={() =>
                                                        updateQuantity(
                                                            item.productId,
                                                            item.quantity + 1
                                                        )
                                                    }
                                                    className="w-7 h-7 rounded bg-gray-200 hover:bg-gray-300 text-sm font-bold"
                                                >
                                                    +
                                                </button>

                                                <button
                                                    onClick={() => removeItem(item.productId)}
                                                    className="ml-auto text-red-500 hover:text-red-700 text-sm"
                                                >
                                                    🗑
                                                </button>
                                            </div>
                                        </div>

                                        {/* Итого */}
                                        <div className="text-right text-sm font-medium whitespace-nowrap">
                                            {(item.price * item.quantity).toLocaleString('ru-RU')} ₽
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                    {/* Footer */}
                    {items.length > 0 && (
                        <div className="border-t p-4 space-y-3">
                            {/* Итого */}
                            <div className="flex justify-between items-center text-lg">
                                <span className="font-medium">Итого:</span>
                                <span className="font-bold text-blue-600">
                                    {totalPrice.toLocaleString('ru-RU')} ₽
                                </span>
                            </div>

                            {/* Ошибка */}
                            {error && (
                                <div className="p-3 bg-red-100 border border-red-400 text-red-700 rounded text-sm">
                                    {error}
                                </div>
                            )}

                            {/* Форма / кнопка */}
                            {!showCheckout ? (
                                <>
                                    <button
                                        onClick={() => setShowCheckout(true)}
                                        className="w-full px-6 py-3 bg-blue-500 text-white rounded-lg hover:bg-blue-600 font-medium"
                                    >
                                        Оформить заказ
                                    </button>
                                    <button
                                        onClick={clearOrder}
                                        className="w-full px-6 py-2 text-red-500 hover:text-red-700 text-sm"
                                    >
                                        Очистить заказ
                                    </button>
                                </>
                            ) : (
                                <form onSubmit={handleCheckout} className="space-y-3">
                                    <div>
                                        <label className="block text-sm font-medium mb-1">
                                            Адрес доставки
                                        </label>
                                        <textarea
                                            value={address}
                                            onChange={(e) => setAddress(e.target.value)}
                                            placeholder="Введите полный адрес"
                                            rows={3}
                                            required
                                            minLength={5}
                                            maxLength={500}
                                            className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none text-sm"
                                        />
                                    </div>

                                    <button
                                        type="submit"
                                        disabled={isSubmitting || address.trim().length < 5}
                                        className="w-full px-6 py-3 bg-green-500 text-white rounded-lg hover:bg-green-600 disabled:opacity-50 font-medium"
                                    >
                                        {isSubmitting ? 'Оформление...' : '✅ Подтвердить заказ'}
                                    </button>

                                    <button
                                        type="button"
                                        onClick={() => setShowCheckout(false)}
                                        className="w-full px-6 py-2 text-gray-600 hover:text-gray-900 text-sm"
                                    >
                                        ← Назад
                                    </button>
                                </form>
                            )}
                        </div>
                    )}
                </div>
            </div>
        </>
    )
}