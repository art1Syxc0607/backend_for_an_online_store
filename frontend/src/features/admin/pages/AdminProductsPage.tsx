import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { getFilteredProducts } from '@/features/products/api/productApi'
import {
    addProduct, updateProduct, deleteProduct,
    uploadProductFiles, deleteProductFiles,
    getLowStockProducts,
} from '../api/adminProductsApi'
import type {
    AddProductRequest,
    UpdateProductRequest,
} from '../api/adminProductsApi'
import AdminTable from '../components/AdminTable'                // ✅ значение
import type { Column } from '../components/AdminTable'           // ✅ тип
import AdminModal from '../components/AdminModal'
import type { ProductDto } from '@/shared/api/types'

export default function AdminProductsPage() {
    const queryClient = useQueryClient()
    const [page, setPage] = useState(1)
    const [editingProduct, setEditingProduct] = useState<ProductDto | null>(null)
    const [isCreating, setIsCreating] = useState(false)
    const [deletingId, setDeletingId] = useState<number | null>(null)
    const [error, setError] = useState<string | null>(null)

    const productsQuery = useQuery({
        queryKey: ['admin', 'products', page],
        queryFn: () => getFilteredProducts({
            pageNumber: page,
            pageSize: 20,
            sortBy: 'Name',
            sortDesc: false,
        }),
    })

    // ✅ Удаление
    const deleteMutation = useMutation({
        mutationFn: deleteProduct,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin', 'products'] })
            setDeletingId(null)
        },
    })

    const columns: Column<ProductDto>[] = [
        {
            key: 'id',
            title: 'ID',
            width: '80px',
            render: (p) => <span className="text-gray-500">#{p.id}</span>,
        },
        {
            key: 'image',
            title: 'Фото',
            width: '80px',
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
                    <div className="text-xs text-gray-500">{p.sku || '—'}</div>
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
            key: 'stock',
            title: 'На складе',
            align: 'center',
            render: (p) => (
                <span className={`text-sm font-medium ${p.availableQuantity > 10 ? 'text-green-600' :
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
                        onClick={() => setEditingProduct(p)}
                        className="px-3 py-1 text-xs bg-blue-500 text-white rounded hover:bg-blue-600"
                    >
                        ✏️
                    </button>
                    <button
                        onClick={() => setDeletingId(p.id)}
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

            <AdminTable
                columns={columns}
                data={productsQuery.data?.items ?? []}
                isLoading={productsQuery.isLoading}
                keyExtractor={(p) => p.id}
                emptyMessage="Товаров нет"
            />

            {/* Пагинация */}
            {productsQuery.data && productsQuery.data.totalPages > 1 && (
                <div className="flex justify-center items-center gap-2 mt-6">
                    <button
                        onClick={() => setPage((p) => Math.max(1, p - 1))}
                        disabled={!productsQuery.data.hasPreviousPage}
                        className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
                    >
                        ←
                    </button>
                    <span className="text-sm">
                        {productsQuery.data.pageNumber} / {productsQuery.data.totalPages}
                    </span>
                    <button
                        onClick={() => setPage((p) => p + 1)}
                        disabled={!productsQuery.data.hasNextPage}
                        className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
                    >
                        →
                    </button>
                </div>
            )}

            {/* ✅ Модалка создания/редактирования */}
            <AdminModal
                isOpen={isCreating || !!editingProduct}
                title={isCreating ? 'Добавить товар' : 'Редактировать товар'}
                onClose={() => { setIsCreating(false); setEditingProduct(null); setError(null) }}
                maxWidth="lg"
            >
                {error && (
                    <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded text-sm">
                        {error}
                    </div>
                )}

                <ProductForm
                    product={editingProduct}
                    onSubmit={async (data, files) => {
                        setError(null)
                        try {
                            if (editingProduct) {
                                await updateProduct({ id: editingProduct.id, ...data })
                            } else {
                                const id = await addProduct({ ...data, files })
                                if (files && files.length > 0 && editingProduct === null) {
                                    // Файлы уже переданы в addProduct
                                }
                                void id
                            }
                            queryClient.invalidateQueries({ queryKey: ['admin', 'products'] })
                            setIsCreating(false)
                            setEditingProduct(null)
                        } catch (err: any) {
                            setError(err?.response?.data?.error ?? 'Ошибка сохранения')
                        }
                    }}
                    onCancel={() => { setIsCreating(false); setEditingProduct(null) }}
                />
            </AdminModal>

            {/* ✅ Подтверждение удаления */}
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

// ✅ Форма товара
function ProductForm({
    product,
    onSubmit,
    onCancel,
}: {
    product: ProductDto | null
    onSubmit: (data: Omit<AddProductRequest, 'files'>, files?: File[]) => Promise<void>
    onCancel: () => void
}) {
    const [form, setForm] = useState({
        name: product?.name ?? '',
        description: product?.description ?? '',
        price: product?.price ?? 0,
        purchasePrice: 0,
        stockQuantity: product?.availableQuantity ?? 0,
        categoryId: product?.categoryId ?? undefined,
    })
    const [files, setFiles] = useState<File[]>([])
    const [isSubmitting, setIsSubmitting] = useState(false)

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        setIsSubmitting(true)
        try {
            await onSubmit(form, files)
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            <div>
                <label className="block text-sm font-medium mb-1">Название *</label>
                <input
                    type="text"
                    value={form.name}
                    onChange={(e) => setForm({ ...form, name: e.target.value })}
                    required
                    className="w-full px-3 py-2 border rounded-lg"
                />
            </div>

            <div>
                <label className="block text-sm font-medium mb-1">Описание</label>
                <textarea
                    value={form.description}
                    onChange={(e) => setForm({ ...form, description: e.target.value })}
                    rows={3}
                    className="w-full px-3 py-2 border rounded-lg resize-none"
                />
            </div>

            <div className="grid grid-cols-2 gap-4">
                <div>
                    <label className="block text-sm font-medium mb-1">Цена *</label>
                    <input
                        type="number"
                        step="0.01"
                        value={form.price}
                        onChange={(e) => setForm({ ...form, price: Number(e.target.value) })}
                        required
                        className="w-full px-3 py-2 border rounded-lg"
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium mb-1">Закупочная цена</label>
                    <input
                        type="number"
                        step="0.01"
                        value={form.purchasePrice}
                        onChange={(e) => setForm({ ...form, purchasePrice: Number(e.target.value) })}
                        className="w-full px-3 py-2 border rounded-lg"
                    />
                </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
                <div>
                    <label className="block text-sm font-medium mb-1">Количество *</label>
                    <input
                        type="number"
                        value={form.stockQuantity}
                        onChange={(e) => setForm({ ...form, stockQuantity: Number(e.target.value) })}
                        required
                        className="w-full px-3 py-2 border rounded-lg"
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium mb-1">ID категории</label>
                    <input
                        type="number"
                        value={form.categoryId ?? ''}
                        onChange={(e) => setForm({ ...form, categoryId: e.target.value ? Number(e.target.value) : undefined })}
                        className="w-full px-3 py-2 border rounded-lg"
                    />
                </div>
            </div>

            {!product && (
                <div>
                    <label className="block text-sm font-medium mb-1">Файлы (фото/видео)</label>
                    <input
                        type="file"
                        multiple
                        accept="image/*,video/*"
                        onChange={(e) => setFiles(Array.from(e.target.files ?? []))}
                        className="w-full text-sm"
                    />
                    {files.length > 0 && (
                        <div className="text-xs text-gray-500 mt-1">Выбрано: {files.length}</div>
                    )}
                </div>
            )}

            <div className="flex gap-2 pt-4">
                <button
                    type="submit"
                    disabled={isSubmitting}
                    className="flex-1 px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50"
                >
                    {isSubmitting ? '...' : 'Сохранить'}
                </button>
                <button
                    type="button"
                    onClick={onCancel}
                    className="flex-1 px-4 py-2 bg-gray-200 rounded-lg hover:bg-gray-300"
                >
                    Отмена
                </button>
            </div>
        </form>
    )
}