import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useParams, Link } from 'react-router'
import {
    getAdminOrderById,
    shipOrder,
    deliverOrder,
} from '../api/adminOrdersApi'
import { getOrderStatusLabel } from '@/features/orders/api/orderApi'

function formatDateTime(value?: string | null) {
    if (!value) return '—'
    return new Date(value).toLocaleString('ru-RU', {
        day: 'numeric', month: 'long', year: 'numeric',
        hour: '2-digit', minute: '2-digit',
    })
}

export default function AdminOrderDetailPage() {
    const { id } = useParams<{ id: string }>()
    const orderId = Number(id)
    const queryClient = useQueryClient()

    const { data: order, isLoading, error } = useQuery({
        queryKey: ['admin', 'order', orderId],
        queryFn: () => getAdminOrderById(orderId),
        enabled: !!orderId,
    })

    const shipMutation = useMutation({
        mutationFn: shipOrder,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin', 'order', orderId] })
            queryClient.invalidateQueries({ queryKey: ['admin', 'orders'] })
        },
    })

    const deliverMutation = useMutation({
        mutationFn: deliverOrder,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin', 'order', orderId] })
            queryClient.invalidateQueries({ queryKey: ['admin', 'orders'] })
        },
    })

    if (isLoading) {
        return <div className="text-center py-20 text-gray-500">Загрузка...</div>
    }

    if (error || !order) {
        return (
            <div className="text-center py-20">
                <p className="text-red-500 mb-4">Заказ не найден</p>
                <Link
                    to="/admin/orders"
                    className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                >
                    ← К списку заказов
                </Link>
            </div>
        )
    }

    const status = getOrderStatusLabel(order.status)
    const items = order.items ?? []

    const purchaseTotal = items.reduce(
        (sum, i) => sum + (i.purchasePriceAtPurchase ?? 0) * i.quantity,
        0
    )
    const margin = order.totalAmount - purchaseTotal

    return (
        <div>
            <div className="mb-6">
                <Link to="/admin/orders" className="text-blue-500 hover:underline text-sm">
                    ← К списку заказов
                </Link>
                <div className="flex items-center justify-between mt-2">
                    <h1 className="text-3xl font-bold">Заказ #{order.orderId}</h1>
                    <span className={`px-4 py-2 rounded-full font-medium ${status.color}`}>
                        {status.label}
                    </span>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Основная информация */}
                <div className="lg:col-span-2 space-y-4">
                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Информация о заказе</h3>
                        <dl className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                            <div>
                                <dt className="text-gray-500">ID заказа</dt>
                                <dd className="font-medium">#{order.orderId}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">User ID</dt>
                                <dd className="font-medium">{order.userId}</dd>
                            </div>
                            <div className="md:col-span-2">
                                <dt className="text-gray-500">Адрес доставки</dt>
                                <dd className="font-medium">{order.shippingAddress || '—'}</dd>
                            </div>
                        </dl>
                    </div>

                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Товары в заказе ({items.length})</h3>
                        <div className="overflow-x-auto">
                            <table className="w-full text-sm">
                                <thead>
                                    <tr className="text-gray-500 border-b text-left">
                                        <th className="py-2 pr-3">Товар</th>
                                        <th className="py-2 px-3 text-right">Цена</th>
                                        <th className="py-2 px-3 text-right">Закупка</th>
                                        <th className="py-2 px-3 text-center">Кол-во</th>
                                        <th className="py-2 pl-3 text-right">Сумма</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {items.map((item) => {
                                        const imageSrc = item.productImage || item.productImages?.[0]
                                        const total = item.totalPrice ?? (item.priceAtPurchase ?? 0) * item.quantity
                                        return (
                                            <tr key={item.id} className="border-b last:border-0">
                                                <td className="py-3 pr-3">
                                                    <div className="flex items-center gap-3">
                                                        <div className="w-12 h-12 bg-gray-200 rounded overflow-hidden flex-shrink-0">
                                                            {imageSrc ? (
                                                                <img
                                                                    src={imageSrc}
                                                                    alt={item.productNameAtPurchase}
                                                                    className="w-full h-full object-cover"
                                                                />
                                                            ) : (
                                                                <div className="w-full h-full flex items-center justify-center text-gray-400 text-xs">📷</div>
                                                            )}
                                                        </div>
                                                        <div className="min-w-0">
                                                            <div className="font-medium line-clamp-1">
                                                                {item.productNameAtPurchase || 'Товар'}
                                                            </div>
                                                            <div className="text-xs text-gray-400">
                                                                ID: {item.productId}
                                                                {item.productSku ? ` · Арт: ${item.productSku}` : ''}
                                                                {item.categoryName ? ` · ${item.categoryName}` : ''}
                                                            </div>
                                                            <a
                                                                href={`/admin/products/${item.productId}`}
                                                                target="_blank"
                                                                rel="noopener noreferrer"
                                                                className="text-xs text-blue-500 hover:underline"
                                                            >
                                                                Открыть товар в админке ↗
                                                            </a>
                                                        </div>
                                                    </div>
                                                </td>
                                                <td className="py-3 px-3 text-right whitespace-nowrap">
                                                    {(item.priceAtPurchase ?? 0).toLocaleString('ru-RU')} ₽
                                                </td>
                                                <td className="py-3 px-3 text-right whitespace-nowrap text-gray-600">
                                                    {(item.purchasePriceAtPurchase ?? 0).toLocaleString('ru-RU')} ₽
                                                </td>
                                                <td className="py-3 px-3 text-center">{item.quantity}</td>
                                                <td className="py-3 pl-3 text-right font-medium whitespace-nowrap">
                                                    {total.toLocaleString('ru-RU')} ₽
                                                </td>
                                            </tr>
                                        )
                                    })}
                                    {items.length === 0 && (
                                        <tr>
                                            <td colSpan={5} className="py-6 text-center text-gray-400">
                                                Товаров нет
                                            </td>
                                        </tr>
                                    )}
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>

                {/* Правая колонка */}
                <div className="lg:col-span-1 space-y-4">
                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Действия</h3>
                        <div className="flex flex-col gap-2">
                            {order.status === 'Paid' && (
                                <button
                                    onClick={() => shipMutation.mutate(order.orderId)}
                                    disabled={shipMutation.isPending}
                                    className="px-4 py-2 bg-indigo-500 text-white rounded-lg hover:bg-indigo-600 disabled:opacity-50"
                                >
                                    {shipMutation.isPending ? '...' : '📦 Отправить'}
                                </button>
                            )}
                            {order.status === 'Shipped' && (
                                <button
                                    onClick={() => deliverMutation.mutate(order.orderId)}
                                    disabled={deliverMutation.isPending}
                                    className="px-4 py-2 bg-purple-500 text-white rounded-lg hover:bg-purple-600 disabled:opacity-50"
                                >
                                    {deliverMutation.isPending ? '...' : '🚚 Доставить'}
                                </button>
                            )}
                            {order.status !== 'Paid' && order.status !== 'Shipped' && (
                                <p className="text-sm text-gray-500">
                                    Нет доступных действий для текущего статуса.
                                </p>
                            )}
                        </div>
                    </div>

                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Финансы</h3>
                        <dl className="space-y-3 text-sm">
                            <div className="flex justify-between">
                                <dt className="text-gray-500">Итого к оплате</dt>
                                <dd className="font-bold text-green-600">
                                    {order.totalAmount.toLocaleString('ru-RU')} ₽
                                </dd>
                            </div>
                            <div className="flex justify-between">
                                <dt className="text-gray-500">Закупочная стоимость</dt>
                                <dd className="font-medium text-gray-700">
                                    {purchaseTotal.toLocaleString('ru-RU')} ₽
                                </dd>
                            </div>
                            <div className="flex justify-between border-t pt-3">
                                <dt className="text-gray-500">Маржа</dt>
                                <dd className={`font-bold ${margin >= 0 ? 'text-green-600' : 'text-red-600'}`}>
                                    {margin.toLocaleString('ru-RU')} ₽
                                </dd>
                            </div>
                        </dl>
                    </div>

                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Хронология</h3>
                        <dl className="space-y-3 text-sm">
                            <div>
                                <dt className="text-gray-500">Создан</dt>
                                <dd className="font-medium">{formatDateTime(order.createdAt)}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Оплачен</dt>
                                <dd className="font-medium">{formatDateTime(order.paidAt)}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Отправлен</dt>
                                <dd className="font-medium">{formatDateTime(order.shippedAt)}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Доставлен</dt>
                                <dd className="font-medium">{formatDateTime(order.deliveredAt)}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Получен</dt>
                                <dd className="font-medium">{formatDateTime(order.receivedAt)}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Отменён</dt>
                                <dd className="font-medium">{formatDateTime(order.cancelledAt)}</dd>
                            </div>
                        </dl>
                    </div>
                </div>
            </div>
        </div>
    )
}
