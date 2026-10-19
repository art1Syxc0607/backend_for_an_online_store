import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import {
    getNumberOfNewOrders,
    getRevenue,
    getPopularProducts,
} from '../api/adminDashboardApi'

export default function AdminDashboardPage() {
    // ✅ Период по умолчанию — 30 дней
    const [firstDay, setFirstDay] = useState(
        new Date(Date.now() - 30 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]
    )
    const [lastDay, setLastDay] = useState(
        new Date().toISOString().split('T')[0]
    )
    const [popularPage, setPopularPage] = useState(1)

    const ordersQuery = useQuery({
        queryKey: ['admin', 'dashboard', 'orders', firstDay, lastDay],
        queryFn: () => getNumberOfNewOrders(firstDay, lastDay),
    })

    const revenueQuery = useQuery({
        queryKey: ['admin', 'dashboard', 'revenue', firstDay, lastDay],
        queryFn: () => getRevenue(firstDay, lastDay),
    })

    const popularQuery = useQuery({
        queryKey: ['admin', 'dashboard', 'popular', firstDay, lastDay, popularPage],
        queryFn: () => getPopularProducts(firstDay, lastDay, popularPage, 10),
        placeholderData: (prev) => prev,
    })

    const orders = ordersQuery.data
    const revenue = revenueQuery.data

    return (
        <div>
            <div className="flex items-center justify-between mb-6">
                <h1 className="text-3xl font-bold">📊 Дашборд</h1>

                {/* Период */}
                <div className="flex items-center gap-2">
                    <input
                        type="date"
                        value={firstDay}
                        onChange={(e) => { setFirstDay(e.target.value); setPopularPage(1) }}
                        className="px-3 py-2 border rounded-lg text-sm"
                    />
                    <span className="text-gray-500">—</span>
                    <input
                        type="date"
                        value={lastDay}
                        onChange={(e) => { setLastDay(e.target.value); setPopularPage(1) }}
                        className="px-3 py-2 border rounded-lg text-sm"
                    />
                </div>
            </div>

            {/* ✅ Статистика */}
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8">
                <StatCard
                    title="💰 Выручка"
                    value={revenue ? `${revenue.revenue.toLocaleString('ru-RU')} ₽` : '...'}
                    color="bg-green-50 border-green-200"
                />
                <StatCard
                    title="💵 Прибыль"
                    value={revenue ? `${revenue.income.toLocaleString('ru-RU')} ₽` : '...'}
                    color="bg-blue-50 border-blue-200"
                />
                <StatCard
                    title="📦 Всего заказов"
                    value={orders ? orders.totalOrders.toString() : '...'}
                    color="bg-purple-50 border-purple-200"
                />
                <StatCard
                    title="⏳ Ожидают оплаты"
                    value={orders ? orders.numberOfPending.toString() : '...'}
                    color="bg-yellow-50 border-yellow-200"
                />
            </div>

            {/* ✅ Заказы по статусам */}
            {orders && (
                <div className="bg-white rounded-lg shadow p-6 mb-8">
                    <h2 className="font-bold text-lg mb-4">Заказы по статусам</h2>
                    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3">
                        <StatusBox label="⏳ Ожидают" value={orders.numberOfPending} color="bg-yellow-100 text-yellow-700" />
                        <StatusBox label="💳 Оплачено" value={orders.numberOfPaid} color="bg-blue-100 text-blue-700" />
                        <StatusBox label="📦 Отправлено" value={orders.numberOfShipped} color="bg-indigo-100 text-indigo-700" />
                        <StatusBox label="🚚 Доставлено" value={orders.numberOfDelivered} color="bg-purple-100 text-purple-700" />
                        <StatusBox label="✅ Получено" value={orders.numberOfReceived} color="bg-green-100 text-green-700" />
                        <StatusBox label="❌ Отменено" value={orders.numberOfCancelled} color="bg-red-100 text-red-700" />
                    </div>
                </div>
            )}

            {/* ✅ Топ-10 товаров */}
            <div className="bg-white rounded-lg shadow p-6">
                <h2 className="font-bold text-lg mb-4">🏆 Топ-10 товаров за период</h2>

                {popularQuery.isLoading ? (
                    <div className="text-center py-4 text-gray-500">Загрузка...</div>
                ) : (
                    <>
                        <div className="space-y-2">
                            {popularQuery.data?.items.map((product, idx) => (
                                <div
                                    key={product.productId}
                                    onClick={() => window.open(`/admin/products/${product.productId}`, '_blank', 'noopener,noreferrer')}
                                    className="flex items-center gap-4 p-3 bg-gray-50 rounded-lg hover:bg-gray-100 cursor-pointer"
                                >
                                    <div className="text-2xl font-bold text-gray-400 w-8">
                                        #{(popularPage - 1) * 10 + idx + 1}
                                    </div>
                                    <div className="w-12 h-12 bg-gray-200 rounded overflow-hidden flex-shrink-0">
                                        {product.imageUrls?.[0] ? (
                                            <img
                                                src={product.imageUrls[0]}
                                                alt={product.name}
                                                className="w-full h-full object-cover"
                                            />
                                        ) : (
                                            <div className="w-full h-full flex items-center justify-center text-gray-400 text-xs">
                                                📷
                                            </div>
                                        )}
                                    </div>
                                    <div className="flex-1 min-w-0">
                                        <div className="font-medium truncate">{product.name}</div>
                                        <div className="text-sm text-gray-500">
                                            Продано: {product.totalPurchases} · Заказов: {product.presenceInOrders}
                                        </div>
                                    </div>
                                    <div className="text-right">
                                        <div className="font-bold text-blue-600">
                                            {product.price.toLocaleString('ru-RU')} ₽
                                        </div>
                                        <div className="text-xs text-gray-500">
                                            ⭐ {product.averageRating.toFixed(1)} ({product.reviewCount})
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>

                        {/* ✅ Пагинация */}
                        {popularQuery.data && popularQuery.data.totalPages > 1 && (
                            <div className="flex justify-center items-center gap-2 mt-6">
                                <button
                                    onClick={() => setPopularPage(1)}
                                    disabled={popularPage <= 1}
                                    className="px-3 py-2 bg-gray-200 rounded disabled:opacity-50"
                                    title="Первая страница"
                                >
                                    «
                                </button>
                                <button
                                    onClick={() => setPopularPage((p) => Math.max(1, p - 1))}
                                    disabled={popularPage <= 1}
                                    className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
                                >
                                    ←
                                </button>
                                <span className="text-sm">
                                    Страница <strong>{popularQuery.data.pageNumber}</strong> из{' '}
                                    <strong>{popularQuery.data.totalPages}</strong>
                                </span>
                                <button
                                    onClick={() => setPopularPage((p) => p + 1)}
                                    disabled={!popularQuery.data.hasNextPage}
                                    className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
                                >
                                    →
                                </button>
                                <button
                                    onClick={() => setPopularPage(popularQuery.data.totalPages)}
                                    disabled={popularPage >= popularQuery.data.totalPages}
                                    className="px-3 py-2 bg-gray-200 rounded disabled:opacity-50"
                                    title="Последняя страница"
                                >
                                    »
                                </button>
                            </div>
                        )}
                    </>
                )}
            </div>
        </div>
    )
}

function StatCard({ title, value, color }: { title: string; value: string; color: string }) {
    return (
        <div className={`${color} border rounded-lg p-4`}>
            <div className="text-sm text-gray-600 mb-1">{title}</div>
            <div className="text-2xl font-bold">{value}</div>
        </div>
    )
}

function StatusBox({ label, value, color }: { label: string; value: number; color: string }) {
    return (
        <div className={`${color} rounded-lg p-3 text-center`}>
            <div className="text-xs opacity-80 mb-1">{label}</div>
            <div className="text-2xl font-bold">{value}</div>
        </div>
    )
}