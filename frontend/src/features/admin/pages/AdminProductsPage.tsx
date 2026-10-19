import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
    addProduct, updateProduct, deleteProduct,
    getAdminProducts, uploadProductFiles, deleteProductFiles,
} from '../api/adminProductsApi'
import type {
    AdminProductFilterRequest,
    AdminProductDto,
} from '../api/adminProductsApi'
import ProductForm from '../components/ProductForm'
import AdminTable from '../components/AdminTable'
import type { Column } from '../components/AdminTable'
import AdminModal from '../components/AdminModal'
import StarRating from '@/shared/ui/StarRating'

const SORT_OPTIONS = [
    { value: 'Name', label: 'По названию' },
    { value: 'Price', label: 'По цене' },
    { value: 'AvailableQuantity', label: 'По наличию' },
    { value: 'Rating', label: 'По рейтингу' },
    { value: 'ReviewAmount', label: 'По отзывам' },
    { value: 'PaymentAmount', label: 'По продажам' },
]

export default function AdminProductsPage() {
    const queryClient = useQueryClient()

    const [editingProduct, setEditingProduct] = useState<AdminProductDto | null>(null)
    const [isCreating, setIsCreating] = useState(false)
    const [deletingId, setDeletingId] = useState<number | null>(null)

    const [filters, setFilters] = useState<AdminProductFilterRequest>({
        pageNumber: 1,
        pageSize: 20,
        sortBy: 'Name',
        sortDesc: true,
    })

    // ✅ Любое изменение фильтра сбрасывает на 1-ю страницу
    const updateFilter = (patch: Partial<AdminProductFilterRequest>) =>
        setFilters((f) => ({ ...f, ...patch, pageNumber: 1 }))

    // ✅ Переход на конкретную страницу
    const gotoPage = (pageNumber: number) =>
        setFilters((f) => ({ ...f, pageNumber }))

    const productsQuery = useQuery({
        queryKey: ['admin', 'products', filters],
        queryFn: () => getAdminProducts(filters),
        placeholderData: (prev) => prev,
    })

    const currentPage = filters.pageNumber ?? 1
    const totalPages = productsQuery.data?.totalPages ?? 0

    const deleteMutation = useMutation({
        mutationFn: deleteProduct,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin', 'products'] })
            setDeletingId(null)
        },
    })

    const columns: Column<AdminProductDto>[] = [
        {
            key: 'id',
            title: 'ID',
            width: '70px',
            render: (p) => <span className="text-gray-500">#{p.id}</span>,
        },
        {
            key: 'image',
            title: 'Фото',
            width: '70px',
            render: (p) => (
                <div className="w-12 h-12 bg-gray-200 rounded overflow-hidden">
                    {p.imageUrls?.[0] ? (
                        <img src={p.imageUrls[0]} alt={p.name} className="w-full h-full object-cover" />
                    ) : (
                        <div className="w-full h-full flex items-center justify-center text-gray-400 text-xs">📷</div>
                    )}
                </div>
            ),
        },
        {
            key: 'name',
            title: 'Название',
            render: (p) => (
                <div>
                    <div className="font-medium">{p.name}</div>
                    <div className="text-xs text-gray-500 line-clamp-1">{p.description || '—'}</div>
                </div>
            ),
        },
        {
            key: 'reviews',
            title: 'Отзывы / Рейтинг',
            align: 'center',
            render: (p) => (
                <div className="flex flex-col items-center gap-0.5">
                    <StarRating rating={p.averageRating} size="sm" />
                    <span className="text-xs text-gray-500">{p.countOfReviews} отз.</span>
                </div>
            ),
        },
        {
            key: 'category',
            title: 'Категория',
            render: (p) => <span className="text-sm">{p.categoryName || '—'}</span>,
        },
        {
            key: 'price',
            title: 'Цена',
            align: 'right',
            render: (p) => (
                <span className="font-medium">{p.price.toLocaleString('ru-RU')} ₽</span>
            ),
        },
        {
            key: 'purchasePrice',
            title: 'Закупка',
            align: 'right',
            render: (p) => (
                <span className="text-sm text-gray-600">{p.purchasePrice.toLocaleString('ru-RU')} ₽</span>
            ),
        },
        {
            key: 'stock',
            title: 'Склад',
            align: 'center',
            render: (p) => <span className="text-sm">{p.stockQuantity}</span>,
        },
        {
            key: 'reserved',
            title: 'Резерв',
            align: 'center',
            render: (p) => (
                <span className={`text-sm ${p.reservedQuantity > 0 ? 'text-yellow-600' : 'text-gray-400'}`}>
                    {p.reservedQuantity}
                </span>
            ),
        },
        {
            key: 'available',
            title: 'Доступно',
            align: 'center',
            render: (p) => (
                <span className={`text-sm font-medium ${
                    p.availableQuantity > 10 ? 'text-green-600' :
                    p.availableQuantity > 0 ? 'text-yellow-600' : 'text-red-600'
                }`}>
                    {p.availableQuantity}
                </span>
            ),
        },
        {
            key: 'actions',
            title: 'Действия',
            align: 'right',
            render: (p) => (
                <div className="flex gap-2 justify-end">
                    <button
                        onClick={(e) => { e.stopPropagation(); setEditingProduct(p) }}
                        className="px-3 py-1 text-xs bg-blue-500 text-white rounded hover:bg-blue-600"
                    >
                        ✏️
                    </button>
                    <button
                        onClick={(e) => { e.stopPropagation(); setDeletingId(p.id) }}
                        className="px-3 py-1 text-xs bg-red-500 text-white rounded hover:bg-red-600"
                    >
                        🗑
                    </button>
                </div>
            ),
        },
    ]

    return (
        <div>
            <div className="flex items-center justify-between mb-6">
                <h1 className="text-3xl font-bold">📦 Товары</h1>
                <button
                    onClick={() => setIsCreating(true)}
                    className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                >
                    + Добавить товар
                </button>
            </div>

            {/* Фильтры */}
            <div className="bg-white rounded-lg shadow p-4 mb-6">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Поиск</label>
                        <input
                            type="text"
                            placeholder="Название товара..."
                            value={filters.searchText ?? ''}
                            onChange={(e) => updateFilter({ searchText: e.target.value || undefined })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        />
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">ID категории</label>
                        <input
                            type="number"
                            value={filters.categoryId ?? ''}
                            onChange={(e) => updateFilter({ categoryId: e.target.value ? Number(e.target.value) : undefined })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        />
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Цена от</label>
                        <input
                            type="number"
                            step="0.01"
                            value={filters.priceLimitMin ?? ''}
                            onChange={(e) => updateFilter({ priceLimitMin: e.target.value ? Number(e.target.value) : undefined })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        />
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Цена до</label>
                        <input
                            type="number"
                            step="0.01"
                            value={filters.priceLimitMax ?? ''}
                            onChange={(e) => updateFilter({ priceLimitMax: e.target.value ? Number(e.target.value) : undefined })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        />
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Сортировка</label>
                        <select
                            value={filters.sortBy}
                            onChange={(e) => updateFilter({ sortBy: e.target.value })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        >
                            {SORT_OPTIONS.map((o) => (
                                <option key={o.value} value={o.value}>{o.label}</option>
                            ))}
                        </select>
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Направление</label>
                        <select
                            value={filters.sortDesc ? 'desc' : 'asc'}
                            onChange={(e) => updateFilter({ sortDesc: e.target.value === 'desc' })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        >
                            <option value="asc">По возрастанию ↑</option>
                            <option value="desc">По убыванию ↓</option>
                        </select>
                    </div>
                    <div className="flex items-end gap-2">
                        <label className="flex items-center gap-2 px-3 py-2 border rounded-lg text-sm cursor-pointer">
                            <input
                                type="checkbox"
                                checked={filters.onlyAvailable ?? false}
                                onChange={(e) => updateFilter({ onlyAvailable: e.target.checked })}
                            />
                            Только в наличии
                        </label>
                        <button
                            onClick={() => setFilters({ pageNumber: 1, pageSize: 20, sortBy: 'Name', sortDesc: true })}
                            className="px-3 py-2 bg-gray-200 rounded-lg hover:bg-gray-300 text-sm"
                        >
                            Сбросить
                        </button>
                    </div>
                </div>
            </div>

            <AdminTable
                columns={columns}
                data={productsQuery.data?.items ?? []}
                isLoading={productsQuery.isLoading}
                keyExtractor={(p) => p.id}
                emptyMessage="Товаров нет"
                onRowClick={(p) => window.open(`/admin/products/${p.id}`, '_blank', 'noopener,noreferrer')}
            />

            {/* Пагинация */}
            {productsQuery.data && (
                <div className="flex justify-center items-center gap-2 mt-6 flex-wrap">
                    <button
                        onClick={() => gotoPage(1)}
                        disabled={currentPage <= 1}
                        className="px-3 py-2 bg-gray-200 rounded disabled:opacity-50"
                        title="Первая страница"
                    >
                        «
                    </button>
                    <button
                        onClick={() => gotoPage(currentPage - 1)}
                        disabled={currentPage <= 1}
                        className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
                    >
                        ←
                    </button>
                    <span className="text-sm">
                        Страница <strong>{currentPage}</strong> из <strong>{totalPages || 1}</strong>
                    </span>
                    <button
                        onClick={() => gotoPage(currentPage + 1)}
                        disabled={!productsQuery.data.hasNextPage}
                        className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
                    >
                        →
                    </button>
                    <button
                        onClick={() => gotoPage(totalPages)}
                        disabled={currentPage >= totalPages}
                        className="px-3 py-2 bg-gray-200 rounded disabled:opacity-50"
                        title="Последняя страница"
                    >
                        »
                    </button>
                    <span className="text-xs text-gray-500 w-full text-center">
                        Всего товаров: {productsQuery.data.totalCount}
                    </span>
                </div>
            )}

            {/* Модалка создания/редактирования */}
            <AdminModal
                isOpen={isCreating || !!editingProduct}
                title={isCreating ? 'Добавить товар' : 'Редактировать товар'}
                onClose={() => { setIsCreating(false); setEditingProduct(null) }}
                maxWidth="lg"
            >
                <ProductForm
                    product={editingProduct}
                    onSubmit={async (data, media) => {
                        if (editingProduct) {
                            await updateProduct({ id: editingProduct.id, ...data })
                            if (media.removedUrls.length)
                                await deleteProductFiles(editingProduct.id, media.removedUrls)
                            if (media.newFiles.length)
                                await uploadProductFiles(editingProduct.id, media.newFiles)
                        } else {
                            await addProduct({
                                name: data.name,
                                description: data.description ?? '',
                                price: data.price,
                                purchasePrice: data.purchasePrice,
                                stockQuantity: data.stockQuantity,
                                categoryId: data.categoryId,
                                files: media.newFiles,
                            })
                        }
                        queryClient.invalidateQueries({ queryKey: ['admin', 'products'] })
                        queryClient.invalidateQueries({ queryKey: ['admin', 'product'] })
                        setIsCreating(false)
                        setEditingProduct(null)
                    }}
                    onCancel={() => { setIsCreating(false); setEditingProduct(null) }}
                />
            </AdminModal>

            {/* Подтверждение удаления */}
            <AdminModal
                isOpen={!!deletingId}
                title="Удалить товар?"
                onClose={() => setDeletingId(null)}
                maxWidth="sm"
                footer={
                    <div className="flex gap-2">
                        <button
                            onClick={() => deletingId && deleteMutation.mutate(deletingId)}
                            disabled={deleteMutation.isPending}
                            className="flex-1 px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 disabled:opacity-50"
                        >
                            {deleteMutation.isPending ? '...' : 'Да, удалить'}
                        </button>
                        <button
                            onClick={() => setDeletingId(null)}
                            className="flex-1 px-4 py-2 bg-gray-200 rounded-lg hover:bg-gray-300"
                        >
                            Отмена
                        </button>
                    </div>
                }
            >
                <p>Вы уверены, что хотите удалить этот товар? Действие нельзя отменить.</p>
            </AdminModal>
        </div>
    )
}

