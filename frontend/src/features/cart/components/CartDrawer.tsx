import { Link } from 'react-router'
import { useCart } from '../hooks/useCart'
import { useCartDrawer } from '../context/CartContext'

export default function CartDrawer() {
    const { isOpen, closeCart } = useCartDrawer()
    const { cart, isLoading, updateQuantity, removeFromCart, isUpdating } = useCart()

    return (
        <>
            {/* Backdrop */}
            {isOpen && (
                <div
                    className="fixed inset-0 bg-black/50 z-[90] transition-opacity"
                    onClick={closeCart}
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
                            🛒 Корзина {cart?.totalItems ? `(${cart.totalItems})` : ''}
                        </h2>
                        <button
                            onClick={closeCart}
                            className="text-gray-500 hover:text-gray-700 text-2xl w-8 h-8 flex items-center justify-center"
                        >
                            ×
                        </button>
                    </div>

                    {/* Body */}
                    <div className="flex-1 overflow-y-auto p-4">
                        {isLoading ? (
                            <div className="text-center py-8 text-gray-500">Загрузка...</div>
                        ) : !cart || !cart.items || cart.items.length === 0 ? (
                            <div className="text-center py-12">
                                <div className="text-6xl mb-4">🛒</div>
                                <p className="text-gray-500 mb-4">Корзина пуста</p>
                                <button
                                    onClick={closeCart}
                                    className="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                                >
                                    Перейти к покупкам
                                </button>
                            </div>
                        ) : (
                            <div className="space-y-3">
                                {cart.items.map((item) => (
                                    <div
                                        key={item.id}
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
                                                onClick={closeCart}
                                                className="font-medium text-gray-900 hover:text-blue-600 line-clamp-2 text-sm"
                                            >
                                                {item.productName}
                                            </Link>

                                            <div className="text-sm text-blue-600 font-bold mt-1">
                                                {item.productPrice?.toLocaleString('ru-RU') ?? 0} ₽
                                            </div>

                                            {/* Количество */}
                                            <div className="flex items-center gap-2 mt-2">
                                                <button
                                                    onClick={() =>
                                                        updateQuantity(
                                                            item.productId,  // ← productId!
                                                            Math.max(1, item.quantity - 1)
                                                        )
                                                    }
                                                    disabled={item.quantity <= 1 || isUpdating}
                                                    className="w-7 h-7 rounded bg-gray-200 hover:bg-gray-300 disabled:opacity-50 text-sm font-bold"
                                                >
                                                    −
                                                </button>
                                                <span className="text-sm w-8 text-center font-medium">
                                                    {item.quantity}
                                                </span>
                                                <button
                                                    onClick={() =>
                                                        updateQuantity(
                                                            item.productId,  // ← productId!
                                                            item.quantity + 1
                                                        )
                                                    }
                                                    disabled={isUpdating}
                                                    className="w-7 h-7 rounded bg-gray-200 hover:bg-gray-300 disabled:opacity-50 text-sm font-bold"
                                                >
                                                    +
                                                </button>

                                                <button
                                                    onClick={() => removeFromCart(item.productId)}  // ← productId!
                                                    disabled={isUpdating}
                                                    className="ml-auto text-red-500 hover:text-red-700 text-sm"
                                                    title="Удалить"
                                                >
                                                    🗑
                                                </button>
                                            </div>
                                        </div>

                                        {/* ✅ Итого — вычисляем на фронте */}
                                        <div className="text-right text-sm font-medium whitespace-nowrap">
                                            {((item.productPrice ?? 0) * item.quantity).toLocaleString('ru-RU')} ₽
                                        </div>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                    {/* Footer */}
                    {cart && cart.items && cart.items.length > 0 && (
                        <div className="border-t p-4 space-y-3">
                            <div className="flex justify-between items-center text-lg">
                                <span className="font-medium">Итого:</span>
                                {/* ✅ Используем backend-поле */}
                                <span className="font-bold text-blue-600">
                                    {cart.totalPrice?.toLocaleString('ru-RU') ?? 0} ₽
                                </span>
                            </div>

                            <Link
                                to="/checkout"
                                onClick={closeCart}
                                className="block w-full px-6 py-3 bg-blue-500 text-white text-center rounded-lg hover:bg-blue-600 font-medium"
                            >
                                Оформить заказ
                            </Link>

                            <button
                                onClick={closeCart}
                                className="block w-full px-6 py-2 text-gray-600 hover:text-gray-900 text-sm"
                            >
                                Продолжить покупки
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </>
    )
}