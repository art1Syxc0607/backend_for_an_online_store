import { useState } from 'react'
import { Link, useNavigate } from 'react-router'
import { useOrderHistory } from '../hooks/useOrderHistory'
import OrderHistoryFilters from '../components/OrderHistoryFilters'
import OrderItemRow from '../components/OrderItemRow'
import OrderActions from '../components/OrderActions'
import { SortOrderBy, getOrderStatusLabel } from '../api/orderApi'
import type { OrderHistoryParams } from '../api/orderApi'

export default function OrdersHistoryPage() {
    const navigate = useNavigate()

    const [filters, setFilters] = useState<OrderHistoryParams>({
        orderBy: SortOrderBy.DateOfCreation,
        sortDesc: true,
        pageNumber: 1,
        pageSize: 10,
    })

    const { data, isLoading, error } = useOrderHistory(filters)

    const handleApplyFilters = (newFilters: OrderHistoryParams) => {
        setFilters(newFilters)
    }

    const handlePageChange = (newPage: number) => {
        setFilters((prev) => ({ ...prev, pageNumber: newPage }))
        window.scrollTo({ top: 0, behavior: 'smooth' })
    }

    if (isLoading) {
        return <div className="text-center py-20">Загрузка истории заказов...</div>
    }

    if (error) {
        return (
            <div className="text-center py-20 text-red-500">
                Ошибка загрузки. Попробуйте позже.
            </div>
        )
    }

    const orders = data?.items ?? []
    const totalCount = data?.totalCount ?? 0

    return (
        <div className="max-w-5xl mx-auto">
            {/* ═══════════════════════════════════════════ */}
            {/* Header */}
            {/* ═══════════════════════════════════════════ */}
            <div className="flex items-center justify-between mb-6">
                <h1 className="text-3xl font-bold">История заказов</h1>
                <Link to="/" className="text-blue-600 hover:underline text-sm">
                    ← К покупкам
                </Link>
            </div>

            {/* ═══════════════════════════════════════════ */}
            {/* ✅ ФИЛЬТРЫ */}
            {/* ═══════════════════════════════════════════ */}
            <OrderHistoryFilters
                filters={filters}
                onApply={handleApplyFilters}
                totalCount={totalCount}
            />

            {/* ═══════════════════════════════════════════ */}
            {/* Пустое состояние / Список */}
            {/* ═══════════════════════════════════════════ */}
            {orders.length === 0 ? (
                <div className="text-center py-20 bg-white rounded-lg shadow">
                    <div className="text-6xl mb-4">📦</div>
                    <h2 className="text-2xl font-bold mb-2">Заказов не найдено</h2>
                    <Link
                        to="/"
                        className="inline-block px-6 py-2 mt-4 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                    >
                        Перейти к покупкам
                    </Link>
                </div>
            ) : (
                <>
                    {/* ✅ Список заказов */}
                    <div className="space-y-4">
                        {orders.map((order) => {
                            const status = getOrderStatusLabel(order.status)

                            return (
                                <div
                                    key={order.orderId}
                                    onClick={() => navigate(`/orders/${order.orderId}`)}
                                    className="block bg-white rounded-lg shadow hover:shadow-lg transition-shadow p-6 cursor-pointer"
                                >
                                    {/* Header */}
                                    <div className="flex items-center justify-between mb-4">
                                        <div>
                                            <h3 className="text-lg font-bold">
                                                Заказ #{order.orderId}
                                            </h3>
                                            <p className="text-sm text-gray-500">
                                                {new Date(order.createdAt).toLocaleDateString('ru-RU', {
                                                    day: 'numeric',
                                                    month: 'long',
                                                    year: 'numeric',
                                                    hour: '2-digit',
                                                    minute: '2-digit',
                                                })}
                                            </p>
                                        </div>
                                        <span
                                            className={`px-3 py-1 rounded-full text-sm font-medium whitespace-nowrap ${status.color}`}
                                        >
                                            {status.label}
                                        </span>
                                    </div>

                                    {/* Товары (превью) */}
                                    {order.items && order.items.length > 0 && (
                                        <div className="space-y-2 mb-4">
                                            {order.items.slice(0, 3).map((item) => (
                                                <OrderItemRow
                                                    key={item.id}
                                                    item={item}
                                                    size="sm"
                                                />
                                            ))}
                                            {order.items.length > 3 && (
                                                <div className="text-sm text-gray-500 pl-2">
                                                    + ещё {order.items.length - 3} товар(ов)
                                                </div>
                                            )}
                                        </div>
                                    )}

                                    {/* Footer */}
                                    <div className="flex items-center justify-between pt-3 border-t">
                                        <div className="text-sm text-gray-600">
                                            📍 {order.shippingAddress.length > 40
                                                ? order.shippingAddress.slice(0, 40) + '...'
                                                : order.shippingAddress}
                                        </div>
                                        <div className="text-lg font-bold text-blue-600 whitespace-nowrap ml-4">
                                            {order.totalAmount.toLocaleString('ru-RU')} ₽
                                        </div>
                                    </div>

                                    {/* ✅ Действия — ВНУТРИ map, order доступен */}
                                    <div
                                        className="mt-4 pt-4 border-t"
                                        onClick={(e) => e.stopPropagation()}
                                    >
                                        <OrderActions
                                            orderId={order.orderId}
                                            status={order.status}
                                            size="sm"
                                        />
                                    </div>
                                </div>
                            )
                        })}
                    </div>

                    {/* ✅ Пагинация */}
                    {data && data.totalPages > 1 && (
                        <div className="flex justify-center items-center gap-2 mt-8">
                            <button
                                onClick={() => handlePageChange(data.pageNumber - 1)}
                                disabled={!data.hasPreviousPage}
                                className="px-4 py-2 bg-gray-200 rounded-lg disabled:opacity-50 hover:bg-gray-300 disabled:cursor-not-allowed"
                            >
                                ← Назад
                            </button>

                            <div className="flex items-center gap-1">
                                {Array.from(
                                    { length: Math.min(5, data.totalPages) },
                                    (_, i) => {
                                        const start = Math.max(
                                            1,
                                            Math.min(
                                                data.pageNumber - 2,
                                                data.totalPages - 4
                                            )
                                        )
                                        return start + i
                                    }
                                )
                                    .filter((p) => p >= 1 && p <= data.totalPages)
                                    .map((pageNum) => (
                                        <button
                                            key={pageNum}
                                            onClick={() => handlePageChange(pageNum)}
                                            className={`w-10 h-10 rounded-lg ${pageNum === data.pageNumber
                                                    ? 'bg-blue-500 text-white font-bold'
                                                    : 'hover:bg-gray-200'
                                                }`}
                                        >
                                            {pageNum}
                                        </button>
                                    ))}
                            </div>

                            <button
                                onClick={() => handlePageChange(data.pageNumber + 1)}
                                disabled={!data.hasNextPage}
                                className="px-4 py-2 bg-gray-200 rounded-lg disabled:opacity-50 hover:bg-gray-300 disabled:cursor-not-allowed"
                            >
                                Вперёд →
                            </button>
                        </div>
                    )}
                </>
            )}
        </div>
    )
}