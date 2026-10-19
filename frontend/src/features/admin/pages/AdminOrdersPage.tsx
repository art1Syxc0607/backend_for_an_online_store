import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getAllOrders, shipOrder, deliverOrder } from '../api/adminOrdersApi'
import { getOrderStatusLabel, SortOrderBy } from '@/features/orders/api/orderApi'
import type { OrderStatus } from '@/features/orders/api/orderApi'
import AdminTable from '../components/AdminTable'                  // ✅ значение
import type { Column } from '../components/AdminTable'             // ✅ тип
import type { OrderResponseDto } from '@/features/orders/api/orderApi'

const PAGE_SIZE = 10

const STATUS_OPTIONS: { value: OrderStatus; label: string }[] = [
    { value: 'Pending', label: '⏳ Ожидает' },
    { value: 'Paid', label: '💳 Оплачен' },
    { value: 'Shipped', label: '📦 Отправлен' },
    { value: 'Delivered', label: '🚚 Доставлен' },
    { value: 'Received', label: '✅ Получен' },
    { value: 'Cancelled', label: '❌ Отменён' },
]

interface OrderFilters {
    orderStatus?: OrderStatus
    userId?: number
    firstDayOfThePeriod?: string
    lastDayOfThePeriod?: string
    orderBy: SortOrderBy
    sortDesc: boolean
}

export default function AdminOrdersPage() {
    const queryClient = useQueryClient()

    const [page, setPage] = useState(1)
    const [filters, setFilters] = useState<OrderFilters>({
        orderBy: SortOrderBy.DateOfCreation,
        sortDesc: true,
    })

    // ✅ Любое изменение фильтра сбрасывает на 1-ю страницу
    const updateFilter = (patch: Partial<OrderFilters>) => {
        setFilters((f) => ({ ...f, ...patch }))
        setPage(1)
    }

    const ordersQuery = useQuery({
        queryKey: ['admin', 'orders', filters],
        queryFn: () => getAllOrders({
            orderStatus: filters.orderStatus,
            userId: filters.userId,
            firstDayOfThePeriod: filters.firstDayOfThePeriod,
            lastDayOfThePeriod: filters.lastDayOfThePeriod,
            orderBy: filters.orderBy,
            sortDesc: filters.sortDesc,
        }),
        placeholderData: (prev) => prev,
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
                            onClick={(e) => { e.stopPropagation(); shipMutation.mutate(o.orderId) }}
                            disabled={shipMutation.isPending}
                            className="px-3 py-1 text-xs bg-indigo-500 text-white rounded hover:bg-indigo-600 disabled:opacity-50"
                        >
                            📦 Отправить
                        </button>
                    )}
                    {o.status === 'Shipped' && (
                        <button
                            onClick={(e) => { e.stopPropagation(); deliverMutation.mutate(o.orderId) }}
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

    // ✅ Клиентская пагинация (эндпоинт отдаёт полный отфильтрованный список)
    const allOrders = ordersQuery.data ?? []
    const totalCount = allOrders.length
    const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE))
    const currentPage = Math.min(page, totalPages)
    const pagedOrders = allOrders.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE)

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
                            onChange={(e) => updateFilter({ orderStatus: (e.target.value || undefined) as OrderStatus | undefined })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        >
                            <option value="">Все</option>
                            {STATUS_OPTIONS.map((s) => (
                                <option key={s.value} value={s.value}>{s.label}</option>
                            ))}
                        </select>
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">User ID</label>
                        <input
                            type="number"
                            value={filters.userId ?? ''}
                            onChange={(e) => updateFilter({ userId: e.target.value ? Number(e.target.value) : undefined })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        />
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Сортировка</label>
                        <select
                            value={filters.orderBy}
                            onChange={(e) => updateFilter({ orderBy: Number(e.target.value) })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        >
                            <option value={SortOrderBy.DateOfCreation}>По дате</option>
                            <option value={SortOrderBy.Price}>По сумме</option>
                            <option value={SortOrderBy.Status}>По статусу</option>
                            <option value={SortOrderBy.UserId}>По пользователю</option>
                        </select>
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Направление</label>
                        <select
                            value={filters.sortDesc ? 'desc' : 'asc'}
                            onChange={(e) => updateFilter({ sortDesc: e.target.value === 'desc' })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        >
                            <option value="desc">По убыванию ↓</option>
                            <option value="asc">По возрастанию ↑</option>
                        </select>
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Период с</label>
                        <input
                            type="date"
                            value={filters.firstDayOfThePeriod ?? ''}
                            onChange={(e) => updateFilter({ firstDayOfThePeriod: e.target.value || undefined })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        />
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Период по</label>
                        <input
                            type="date"
                            value={filters.lastDayOfThePeriod ?? ''}
                            onChange={(e) => updateFilter({ lastDayOfThePeriod: e.target.value || undefined })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        />
                    </div>
                    <div className="flex items-end">
                        <button
                            onClick={() => {
                                setFilters({ orderBy: SortOrderBy.DateOfCreation, sortDesc: true })
                                setPage(1)
                            }}
                            className="px-4 py-2 bg-gray-200 rounded-lg hover:bg-gray-300 text-sm"
                        >
                            Сбросить
                        </button>
                    </div>
                </div>
                <p className="text-xs text-gray-400 mt-2">
                    Фильтр по периоду применяется, только если указаны обе даты.
                </p>
            </div>

            <AdminTable
                columns={columns}
                data={pagedOrders}
                isLoading={ordersQuery.isLoading}
                keyExtractor={(o) => o.orderId}
                emptyMessage="Заказов нет"
                onRowClick={(o) => window.open(`/admin/orders/${o.orderId}`, '_blank', 'noopener,noreferrer')}
            />

            {/* Пагинация */}
            {ordersQuery.data && (
                <div className="flex justify-center items-center gap-2 mt-6 flex-wrap">
                    <button
                        onClick={() => setPage(1)}
                        disabled={currentPage <= 1}
                        className="px-3 py-2 bg-gray-200 rounded disabled:opacity-50"
                        title="Первая страница"
                    >
                        «
                    </button>
                    <button
                        onClick={() => setPage(currentPage - 1)}
                        disabled={currentPage <= 1}
                        className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
                    >
                        ←
                    </button>
                    <span className="text-sm">
                        Страница <strong>{currentPage}</strong> из <strong>{totalPages}</strong>
                    </span>
                    <button
                        onClick={() => setPage(currentPage + 1)}
                        disabled={currentPage >= totalPages}
                        className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
                    >
                        →
                    </button>
                    <button
                        onClick={() => setPage(totalPages)}
                        disabled={currentPage >= totalPages}
                        className="px-3 py-2 bg-gray-200 rounded disabled:opacity-50"
                        title="Последняя страница"
                    >
                        »
                    </button>
                    <span className="text-xs text-gray-500 w-full text-center">
                        Всего заказов: {totalCount}
                    </span>
                </div>
            )}
        </div>
    )
}
