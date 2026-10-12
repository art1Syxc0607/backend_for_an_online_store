import { useState } from 'react'
import type { GetProductReviewsParams } from '../api/reviewApi'

interface Props {
    filters: GetProductReviewsParams
    onApply: (filters: GetProductReviewsParams) => void
    totalCount: number
}

export default function ReviewFilters({ filters, onApply, totalCount }: Props) {
    const [localFilters, setLocalFilters] = useState<GetProductReviewsParams>(filters)

    const handleChange = <K extends keyof GetProductReviewsParams>(
        key: K,
        value: GetProductReviewsParams[K]
    ) => {
        setLocalFilters((prev) => ({ ...prev, [key]: value }))
    }

    const handleApply = () => {
        onApply(localFilters)
    }

    const handleReset = () => {
        const reset: GetProductReviewsParams = {
            sortBy: 'DateOfCreation',
            descending: true,
        }
        setLocalFilters(reset)
        onApply(reset)
    }

    return (
        <div className="bg-white rounded-lg shadow p-4 mb-4">
            <div className="flex items-center justify-between mb-3">
                <h3 className="font-bold">Фильтры отзывов</h3>
                <span className="text-sm text-gray-500">Всего: {totalCount}</span>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-3">
                {/* Минимальный рейтинг */}
                <div>
                    <label className="block text-xs font-medium mb-1 text-gray-600">
                        Мин. рейтинг
                    </label>
                    <select
                        value={localFilters.minRating ?? ''}
                        onChange={(e) =>
                            handleChange('minRating', e.target.value ? Number(e.target.value) : undefined)
                        }
                        className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        <option value="">Все</option>
                        <option value="5">5 ★</option>
                        <option value="4">4 ★ и выше</option>
                        <option value="3">3 ★ и выше</option>
                        <option value="2">2 ★ и выше</option>
                        <option value="1">1 ★ и выше</option>
                    </select>
                </div>

                {/* Сортировка */}
                <div>
                    <label className="block text-xs font-medium mb-1 text-gray-600">
                        Сортировка
                    </label>
                    <select
                        value={localFilters.sortBy ?? 'DateOfCreation'}
                        onChange={(e) => handleChange('sortBy', e.target.value)}
                        className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        <option value="DateOfCreation">По дате</option>
                        <option value="Rating">По рейтингу</option>
                    </select>
                </div>

                {/* Направление */}
                <div>
                    <label className="block text-xs font-medium mb-1 text-gray-600">
                        Направление
                    </label>
                    <select
                        value={localFilters.descending ? 'desc' : 'asc'}
                        onChange={(e) => handleChange('descending', e.target.value === 'desc')}
                        className="w-full px-3 py-2 border rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        <option value="desc">Сначала новые</option>
                        <option value="asc">Сначала старые</option>
                    </select>
                </div>

                {/* Чекбоксы */}
                <div className="space-y-2">
                    <label className="flex items-center gap-2 cursor-pointer text-sm">
                        <input
                            type="checkbox"
                            checked={localFilters.hasMedia ?? false}
                            onChange={(e) => handleChange('hasMedia', e.target.checked || undefined)}
                            className="w-4 h-4"
                        />
                        <span>С медиа</span>
                    </label>

                    <label className="flex items-center gap-2 cursor-pointer text-sm">
                        <input
                            type="checkbox"
                            checked={localFilters.isVerifiedPurchase ?? false}
                            onChange={(e) =>
                                handleChange('isVerifiedPurchase', e.target.checked || undefined)
                            }
                            className="w-4 h-4"
                        />
                        <span>Только покупки</span>
                    </label>

                    <label className="flex items-center gap-2 cursor-pointer text-sm">
                        <input
                            type="checkbox"
                            checked={localFilters.hasAdminResponse ?? false}
                            onChange={(e) =>
                                handleChange('hasAdminResponse', e.target.checked || undefined)
                            }
                            className="w-4 h-4"
                        />
                        <span>С ответом</span>
                    </label>
                </div>
            </div>

            {/* Кнопки */}
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