import { useState } from 'react'
import { SortOrderBy, getOrderStatusLabel } from '../api/orderApi'
import type { OrderHistoryParams, OrderStatus } from '../api/orderApi'

// ✅ Массив статусов (вместо Object.values)
const ORDER_STATUSES: OrderStatus[] = [
    'Pending',
    'Paid',
    'Shipped',
    'Delivered',
    'Received',
    'Cancelled',
]

interface Props {
    filters: OrderHistoryParams
    onApply: (filters: OrderHistoryParams) => void
    totalCount: number
}

export default function OrderHistoryFilters({ filters, onApply, totalCount }: Props) {
    const [localFilters, setLocalFilters] = useState<OrderHistoryParams>(filters)

    const handleStatusChange = (value: string) => {
        const status = value === '' ? undefined : (value as OrderStatus)
        setLocalFilters((prev) => ({ ...prev, orderStatus: status }))
    }

    const handleDateChange = (
        key: 'firstDayOfThePeriod' | 'lastDayOfThePeriod',
        value: string
    ) => {
        setLocalFilters((prev) => ({
            ...prev,
            [key]: value ? new Date(value).toISOString() : undefined,
        }))
    }

    const handleApply = () => {
        onApply({ ...localFilters, pageNumber: 1 })
    }

    const handleReset = () => {
        const reset: OrderHistoryParams = {
            orderBy: SortOrderBy.DateOfCreation,
            sortDesc: true,
            pageNumber: 1,
            pageSize: 10,
        }
        setLocalFilters(reset)
        onApply(reset)
    }

    const firstDateValue = localFilters.firstDayOfThePeriod
        ? localFilters.firstDayOfThePeriod.split('T')[0]
        : ''
    const lastDateValue = localFilters.lastDayOfThePeriod
        ? localFilters.lastDayOfThePeriod.split('T')[0]
        : ''

    return (
        <div className="bg-white rounded-lg shadow p-4 mb-4">
            <div className="flex items-center justify-between mb-3">
                <h3 className="font-bold">Фильтры</h3>
                <span className="text-sm text-gray-500">Найдено: {totalCount}</span>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3">
                {/* Статус */}
                <div>
                    <label className="block text-xs font-medium mb-1 text-gray-600">
                        Статус
                    </label>
                    <select
                        value={localFilters.orderStatus ?? ''}
                        onChange={(e) => handleStatusChange(e.target.value)}
                        className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        <option value="">Все статусы</option>
                        {ORDER_STATUSES.map((status) => (
                            <option key={status} value={status}>
                                {getOrderStatusLabel(status).label}
                            </option>
                        ))}
                    </select>
                </div>

                {/* От */}
                <div>
                    <label className="block text-xs font-medium mb-1 text-gray-600">
                        От
                    </label>
                    <input
                        type="date"
                        value={firstDateValue}
                        onChange={(e) => handleDateChange('firstDayOfThePeriod', e.target.value)}
                        className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                </div>

                {/* До */}
                <div>
                    <label className="block text-xs font-medium mb-1 text-gray-600">
                        До
                    </label>
                    <input
                        type="date"
                        value={lastDateValue}
                        onChange={(e) => handleDateChange('lastDayOfThePeriod', e.target.value)}
                        className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                </div>

                {/* Сортировка */}
                <div>
                    <label className="block text-xs font-medium mb-1 text-gray-600">
                        Сортировка
                    </label>
                    <select
                        value={localFilters.orderBy ?? SortOrderBy.DateOfCreation}
                        onChange={(e) =>
                            setLocalFilters((prev) => ({
                                ...prev,
                                orderBy: Number(e.target.value) as SortOrderBy,
                            }))
                        }
                        className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        <option value={SortOrderBy.DateOfCreation}>По дате</option>
                        <option value={SortOrderBy.Price}>По сумме</option>
                        <option value={SortOrderBy.Status}>По статусу</option>
                    </select>
                </div>

                {/* Направление */}
                <div>
                    <label className="block text-xs font-medium mb-1 text-gray-600">
                        Направление
                    </label>
                    <select
                        value={localFilters.sortDesc === false ? 'asc' : 'desc'}
                        onChange={(e) =>
                            setLocalFilters((prev) => ({
                                ...prev,
                                sortDesc: e.target.value === 'desc',
                            }))
                        }
                        className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        <option value="desc">По убыванию</option>
                        <option value="asc">По возрастанию</option>
                    </select>
                </div>

                {/* Размер страницы */}
                <div>
                    <label className="block text-xs font-medium mb-1 text-gray-600">
                        На странице
                    </label>
                    <select
                        value={localFilters.pageSize ?? 10}
                        onChange={(e) =>
                            setLocalFilters((prev) => ({
                                ...prev,
                                pageSize: Number(e.target.value),
                            }))
                        }
                        className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        <option value={10}>10</option>
                        <option value={20}>20</option>
                        <option value={50}>50</option>
                    </select>
                </div>
            </div>

            <div className="flex gap-2 mt-4">
                <button
                    onClick={handleApply}
                    className="px-4 py-2 bg-blue-500 text-white rounded-lg text-sm hover:bg-blue-600"
                >
                    Применить
                </button>
                <button
                    onClick={handleReset}
                    className="px-4 py-2 bg-gray-200 rounded-lg text-sm hover:bg-gray-300"
                >
                    Сбросить
                </button>
            </div>
        </div>
    )
}