import { Link, Outlet, useLocation, useNavigate } from 'react-router'
import { useAuth } from '@/features/auth/hooks/useAuth'

const NAV_ITEMS = [
    { path: '/admin', label: '📊 Дашборд', exact: true },
    { path: '/admin/products', label: '📦 Товары' },
    { path: '/admin/categories', label: '📁 Категории' },
    { path: '/admin/orders', label: '🛒 Заказы' },
    { path: '/admin/reviews', label: '⭐ Отзывы' },
    { path: '/admin/users', label: '👥 Пользователи' },
]

export default function AdminLayout() {
    const location = useLocation()
    const navigate = useNavigate()
    const { user, logout, isAuthenticated } = useAuth()

    // ✅ Проверка прав
    if (!isAuthenticated || user?.role !== 'Admin') {
        return (
            <div className="text-center py-20">
                <h1 className="text-3xl font-bold mb-4">⛔ Доступ запрещён</h1>
                <p className="text-gray-600 mb-6">
                    Эта страница доступна только администраторам
                </p>
                <button
                    onClick={() => navigate('/')}
                    className="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                >
                    На главную
                </button>
            </div>
        )
    }

    const isActive = (item: typeof NAV_ITEMS[0]) => {
        if (item.exact) return location.pathname === item.path
        return location.pathname.startsWith(item.path)
    }

    return (
        <div className="min-h-screen bg-gray-100">
            <div className="flex">
                {/* ✅ Sidebar */}
                <aside className="w-64 bg-white shadow-lg min-h-screen fixed left-0 top-0 bottom-0 overflow-y-auto">
                    <div className="p-4 border-b">
                        <Link to="/admin" className="text-xl font-bold text-blue-600">
                            🎛️ Админ-панель
                        </Link>
                    </div>

                    <nav className="p-4 space-y-1">
                        {NAV_ITEMS.map((item) => (
                            <Link
                                key={item.path}
                                to={item.path}
                                className={`block px-4 py-2 rounded-lg transition ${isActive(item)
                                        ? 'bg-blue-500 text-white font-medium'
                                        : 'text-gray-700 hover:bg-gray-100'
                                    }`}
                            >
                                {item.label}
                            </Link>
                        ))}
                    </nav>

                    <div className="p-4 border-t mt-auto">
                        <div className="text-sm text-gray-500 mb-2">
                            {user?.userName}
                        </div>
                        <button
                            onClick={() => navigate('/')}
                            className="block w-full text-left px-4 py-2 text-sm text-gray-700 hover:bg-gray-100 rounded-lg"
                        >
                            ← К магазину
                        </button>
                        <button
                            onClick={logout}
                            className="block w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50 rounded-lg"
                        >
                            🚪 Выйти
                        </button>
                    </div>
                </aside>

                {/* ✅ Content */}
                <main className="flex-1 ml-64 p-8">
                    <Outlet />
                </main>
            </div>
        </div>
    )
}