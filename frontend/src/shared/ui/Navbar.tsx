import { Link } from 'react-router'
import { useAuth } from '@/features/auth/hooks/useAuth'
import { useCart } from '@/features/cart/hooks/useCart'
import { useCartDrawer } from '@/features/cart/context/CartContext'
import { useCurrentOrder } from '@/features/current-order/context/CurrentOrderContext'

export default function Navbar() {
    const { isAuthenticated, user } = useAuth()
    const { cart } = useCart()
    const { toggleCart } = useCartDrawer()
    const { totalItems: currentOrderItems, toggleDrawer: toggleCurrentOrder } = useCurrentOrder()

    const itemCount = cart?.totalItems ?? 0

    return (
        <nav className="bg-white shadow sticky top-0 z-50">
            <div className="container mx-auto px-4 py-3 flex items-center justify-between">
                <Link to="/" className="text-xl font-bold text-blue-600">
                    ArtWebStore
                </Link>
                <div className="flex gap-4 items-center">
                    <Link to="/" className="text-gray-600 hover:text-gray-900">
                        Товары
                    </Link>

                    {user?.role === 'Admin' && (
                        <Link
                            to="/admin"
                            className="px-3 py-1.5 bg-purple-500 text-white rounded-lg hover:bg-purple-600 text-sm font-medium"
                        >
                            🎛️ Админка
                        </Link>
                    )}

                    {/* ✅ Кнопка корзины */}
                    {isAuthenticated && (
                        <button
                            onClick={toggleCart}
                            className="relative text-gray-600 hover:text-gray-900 text-2xl"
                            title="Корзина"
                        >
                            🛒
                            {itemCount > 0 && (
                                <span className="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center font-bold">
                                    {itemCount > 99 ? '99+' : itemCount}
                                </span>
                            )}
                        </button>
                    )}

                    {/* ✅ Кнопка текущего заказа */}
                    {isAuthenticated && (
                        <button
                            onClick={toggleCurrentOrder}
                            className="relative text-gray-600 hover:text-gray-900 text-2xl"
                            title="Текущий заказ"
                        >
                            📦
                            {currentOrderItems > 0 && (
                                <span className="absolute -top-1 -right-1 bg-green-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center font-bold">
                                    {currentOrderItems > 99 ? '99+' : currentOrderItems}
                                </span>
                            )}
                        </button>
                    )}

                    {isAuthenticated && (
                        <Link
                            to="/orders"
                            className="text-gray-600 hover:text-gray-900"
                            title="История заказов"
                        >
                            📋 Заказы
                        </Link>
                    )}

                    {isAuthenticated ? (
                        <Link
                            to="/profile"
                            className="flex items-center gap-2 text-gray-600 hover:text-gray-900"
                        >
                            <span>👤</span>
                            <span className="hidden sm:inline">{user?.userName}</span>
                        </Link>
                    ) : (
                        <Link to="/login" className="text-gray-600 hover:text-gray-900">
                            Войти
                        </Link>
                    )}
                </div>
            </div>
        </nav>
    )
}