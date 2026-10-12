import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getAllOrders, shipOrder, deliverOrder } from '../api/adminOrdersApi'
import { getOrderStatusLabel, SortOrderBy } from '@/features/orders/api/orderApi'
import AdminTable from '../components/AdminTable'                  // ✅ значение
import type { Column } from '../components/AdminTable'             // ✅ тип
import type { OrderResponseDto } from '@/features/orders/api/orderApi'

export default function AdminOrdersPage() {
    const queryClient = useQueryClient()
    const [filters, setFilters] = useState({
        orderStatus: undefined as string | undefined,
        userId: undefined as number | undefined,
        orderBy: SortOrderBy.DateOfCreation,
        sortDesc: true,
    })

    const ordersQuery = useQuery({
        queryKey: ['admin', 'orders', filters],
        queryFn: () => getAllOrders({
            orderStatus: filters.orderStatus,
            userId: filters.userId,
            orderBy: filters.orderBy,
            sortDesc: filters.sortDesc,
        }),
    })

    const shipMutation = useMutation({
        mutationFn: shipOrder,
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin', 'orders'] }),
    })

    const deliverMutation = useMutation({
        mutationFn: deliverOrder,
        onSuccess: () => queryClient.invalidateQueries({ queryKey: ['admin', 'orders'] }),
    })

    const columns: Column<OrderResponseDto>[] = [
        {
            key: 'id',
            title: 'Заказ',
            render: (o) => (
                <div>
                    <div className="font-medium">#{o.orderId}</div>
                    <div className="text-xs text-gray-500">
                        {new Date(o.createdAt).toLocaleDateString('ru-RU')}
                    </div>
                </div>
            ),
        },
        {
            key: 'userId',
            title: 'User ID',
            align: 'center',
            render: (o) => <span className="text-sm">{o.userId}</span>,
        },
        {
            key: 'total',
            title: 'Сумма',
            align: 'right',
            render: (o) => (
                <span className="font-medium">{o.totalAmount.toLocaleString('ru-RU')} ₽</span>
            ),
        },
        {
            key: 'status',
            title: 'Статус',
            render: (o) => {
                const status = getOrderStatusLabel(o.status)
                return (
                    <span className={`px-3 py-1 rounded-full text-xs font-medium ${status.color}`}>
                        {status.label}
                    </span>
                )
            },
        },
        {
            key: 'actions',
            title: 'Действия',
            align: 'right',
            render: (o) => (
                <div className="flex gap-2 justify-end">
                    {o.status === 'Paid' && (
                        <button
                            onClick={() => shipMutation.mutate(o.orderId)}
                            disabled={shipMutation.isPending}
                            className="px-3 py-1 text-xs bg-indigo-500 text-white rounded hover:bg-indigo-600 disabled:opacity-50"
                        >
                            📦 Отправить
                        </button>
                    )}
                    {o.status === 'Shipped' && (
                        <button
                            onClick={() => deliverMutation.mutate(o.orderId)}
                            disabled={deliverMutation.isPending}
                            className="px-3 py-1 text-xs bg-purple-500 text-white rounded hover:bg-purple-600 disabled:opacity-50"
                        >
                            🚚 Доставить
                        </button>
                    )}
                </div>
            ),
        },
    ]

    return (
        <div>
            <h1 className="text-3xl font-bold mb-6">🛒 Заказы</h1>

            {/* Фильтры */}
            <div className="bg-white rounded-lg shadow p-4 mb-6">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Статус</label>
                        <select
                            value={filters.orderStatus ?? ''}
                            onChange={(e) => setFilters({ ...filters, orderStatus: e.target.value || undefined })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        >
                            <option value="">Все</option>
                            <option value="Pending">⏳ Ожидает</option>
                            <option value="Paid">💳 Оплачен</option>
                            <option value="Shipped">📦 Отправлен</option>
                            <option value="Delivered">🚚 Доставлен</option>
                            <option value="Received">✅ Получен</option>
                            <option value="Cancelled">❌ Отменён</option>
                        </select>
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">User ID</label>
                        <input
                            type="number"
                            value={filters.userId ?? ''}
                            onChange={(e) => setFilters({ ...filters, userId: e.target.value ? Number(e.target.value) : undefined })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        />
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Сортировка</label>
                        <select
                            value={filters.orderBy}
                            onChange={(e) => setFilters({ ...filters, orderBy: Number(e.target.value) })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        >
                            <option value={SortOrderBy.DateOfCreation}>По дате</option>
                            <option value={SortOrderBy.Price}>По сумме</option>
                            <option value={SortOrderBy.Status}>По статусу</option>
                        </select>
                    </div>
                </div>
            </div>

            <AdminTable
                columns={columns}
                data={ordersQuery.data ?? []}
                isLoading={ordersQuery.isLoading}
                keyExtractor={(o) => o.orderId}
                emptyMessage="Заказов нет"
            />
        </div>
    )
}