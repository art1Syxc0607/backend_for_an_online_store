import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/shared/api/client'
import type { ProductFilterRequest } from '../api/productApi'

interface CategoryDto {
    id: number
    name: string
}

interface Props {
    initialFilters: ProductFilterRequest
    onApply: (filters: ProductFilterRequest) => void
    onCancel: () => void
}

export default function ProductFilters({ initialFilters, onApply, onCancel }: Props) {
    const [searchText, setSearchText] = useState(initialFilters.searchText ?? '')
    const [categoryId, setCategoryId] = useState<number | undefined>(initialFilters.categoryId)
    const [priceLimitMin, setPriceLimitMin] = useState<number | undefined>(initialFilters.priceLimitMin)
    const [priceLimitMax, setPriceLimitMax] = useState<number | undefined>(initialFilters.priceLimitMax)
    const [onlyAvailable, setOnlyAvailable] = useState(initialFilters.onlyAvailable ?? false)
    const [sortBy, setSortBy] = useState(initialFilters.sortBy ?? 'Name')
    const [sortDesc, setSortDesc] = useState(initialFilters.sortDesc ?? true)

    // ✅ Загружаем категории
    const { data: categories } = useQuery({
        queryKey: ['categories'],
        queryFn: async () => {
            const { data } = await apiClient.get<CategoryDto[]>('/categories')
            return data
        },
    })

    const handleApply = () => {
        onApply({
            searchText: searchText.trim() || undefined,
            categoryId,
            priceLimitMin,
            priceLimitMax,
            onlyAvailable: onlyAvailable || undefined,
            sortBy,
            sortDesc,
            pageNumber: 1,
            pageSize: 20,
        })
    }

    const handleReset = () => {
        setSearchText('')
        setCategoryId(undefined)
        setPriceLimitMin(undefined)
        setPriceLimitMax(undefined)
        setOnlyAvailable(false)
        setSortBy('Name')
        setSortDesc(true)
    }

    return (
        <div className="bg-white rounded-lg shadow p-6 mb-6">
            <h2 className="text-xl font-bold mb-4">🔍 Фильтры</h2>

            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {/* Поиск */}
                <div className="lg:col-span-3">
                    <label className="block text-sm font-medium mb-1">Поиск</label>
                    <input
                        type="text"
                        value={searchText}
                        onChange={(e) => setSearchText(e.target.value)}
                        placeholder="Название товара..."
                        className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                </div>

                {/* Категория */}
                <div>
                    <label className="block text-sm font-medium mb-1">Категория</label>
                    <select
                        value={categoryId ?? ''}
                        onChange={(e) => setCategoryId(e.target.value ? Number(e.target.value) : undefined)}
                        className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        <option value="">Все категории</option>
                        {categories?.map((cat) => (
                            <option key={cat.id} value={cat.id}>
                                {cat.name}
                            </option>
                        ))}
                    </select>
                </div>

                {/* Цена от */}
                <div>
                    <label className="block text-sm font-medium mb-1">Цена от</label>
                    <input
                        type="number"
                        value={priceLimitMin ?? ''}
                        onChange={(e) => setPriceLimitMin(e.target.value ? Number(e.target.value) : undefined)}
                        placeholder="0"
                        className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                </div>

                {/* Цена до */}
                <div>
                    <label className="block text-sm font-medium mb-1">Цена до</label>
                    <input
                        type="number"
                        value={priceLimitMax ?? ''}
                        onChange={(e) => setPriceLimitMax(e.target.value ? Number(e.target.value) : undefined)}
                        placeholder="100000"
                        className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />
                </div>

                {/* Сортировка */}
                <div>
                    <label className="block text-sm font-medium mb-1">Сортировка</label>
                    <select
                        value={sortBy}
                        onChange={(e) => setSortBy(e.target.value)}
                        className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        <option value="Name">По названию</option>
                        <option value="Price">По цене</option>
                        <option value="Rating">По рейтингу</option>
                        <option value="AvailableQuantity">По наличию</option>
                    </select>
                </div>

                {/* Направление */}
                <div>
                    <label className="block text-sm font-medium mb-1">Направление</label>
                    <select
                        value={sortDesc ? 'desc' : 'asc'}
                        onChange={(e) => setSortDesc(e.target.value === 'desc')}
                        className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        <option value="desc">По убыванию</option>
                        <option value="asc">По возрастанию</option>
                    </select>
                </div>

                {/* Только в наличии */}
                <div className="flex items-end">
                    <label className="flex items-center gap-2 cursor-pointer">
                        <input
                            type="checkbox"
                            checked={onlyAvailable}
                            onChange={(e) => setOnlyAvailable(e.target.checked)}
                            className="w-5 h-5"
                        />
                        <span className="text-sm font-medium">Только в наличии</span>
                    </label>
                </div>
            </div>

            {/* Кнопки */}
            <div className="flex gap-2 mt-6">
                <button
                    onClick={handleApply}
                    className="px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                >
                    Применить
                </button>
                <button
                    onClick={handleReset}
                    className="px-6 py-2 bg-gray-200 rounded-lg hover:bg-gray-300"
                >
                    Сбросить
                </button>
                <button
                    onClick={onCancel}
                    className="px-6 py-2 bg-gray-100 rounded-lg hover:bg-gray-200"
                >
                    Отмена
                </button>
            </div>
        </div>
    )
}