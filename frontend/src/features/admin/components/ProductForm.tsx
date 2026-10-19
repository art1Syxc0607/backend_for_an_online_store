import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import type { AdminProductDto } from '../api/adminProductsApi'
import { getAdminCategories } from '../api/adminCategoriesApi'
import ProductMediaEditor from './ProductMediaEditor'
import type { MediaChange } from './ProductMediaEditor'

export interface ProductFormData {
    name: string
    description?: string
    price: number
    purchasePrice: number
    stockQuantity: number
    reservedQuantity?: number
    categoryId?: number
    sku?: string
}

interface Props {
    /** null — создание нового товара */
    product: AdminProductDto | null
    onSubmit: (data: ProductFormData, media: MediaChange) => Promise<void>
    onCancel: () => void
}

/**
 * Форма создания/редактирования товара.
 * Используется и в списке товаров (модалка), и на странице товара.
 */
export default function ProductForm({ product, onSubmit, onCancel }: Props) {
    const [form, setForm] = useState({
        name: product?.name ?? '',
        description: product?.description ?? '',
        price: product?.price ?? 0,
        purchasePrice: product?.purchasePrice ?? 0,
        stockQuantity: product?.stockQuantity ?? 0,
        reservedQuantity: product?.reservedQuantity ?? 0,
        categoryId: product?.categoryId ?? undefined,
        sku: '',
    })
    const [media, setMedia] = useState<MediaChange>({ newFiles: [], removedUrls: [] })
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [error, setError] = useState<string | null>(null)

    const categoriesQuery = useQuery({
        queryKey: ['admin', 'categories'],
        queryFn: getAdminCategories,
        staleTime: 5 * 60 * 1000,
        enabled: true,
    })
    const categories = categoriesQuery.data ?? []

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        setError(null)

        if (form.purchasePrice > form.price) {
            setError('Закупочная цена не может быть выше цены продажи')
            return
        }
        if (form.reservedQuantity > form.stockQuantity) {
            setError('Резерв не может быть больше остатка на складе')
            return
        }
        if (form.stockQuantity < 0 || form.reservedQuantity < 0) {
            setError('Остаток и резерв не могут быть отрицательными')
            return
        }

        setIsSubmitting(true)
        try {
            await onSubmit(
                {
                    ...form,
                    // Пустое описание нельзя сохранить (домен запрещает) — не отправляем его.
                    description: form.description.trim() ? form.description : undefined,
                    // SKU не приходит в админском DTO — не затираем существующий пустой строкой.
                    sku: form.sku.trim() ? form.sku : undefined,
                },
                media
            )
        } catch (err: any) {
            const data = err?.response?.data
            setError(
                (typeof data === 'string' ? data : data?.error ?? data?.message) ??
                'Ошибка сохранения'
            )
        } finally {
            setIsSubmitting(false)
        }
    }

    const currentCategoryMissing =
        !!form.categoryId && !categories.some((c) => c.id === form.categoryId)

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            {error && (
                <div className="p-3 bg-red-100 border border-red-400 text-red-700 rounded text-sm">
                    {error}
                </div>
            )}

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
                        min={0}
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
                        min={0}
                        value={form.purchasePrice}
                        onChange={(e) => setForm({ ...form, purchasePrice: Number(e.target.value) })}
                        className="w-full px-3 py-2 border rounded-lg"
                    />
                </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
                <div>
                    <label className="block text-sm font-medium mb-1">Количество на складе *</label>
                    <input
                        type="number"
                        min={0}
                        value={form.stockQuantity}
                        onChange={(e) => setForm({ ...form, stockQuantity: Number(e.target.value) })}
                        required
                        className="w-full px-3 py-2 border rounded-lg"
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium mb-1">Зарезервировано</label>
                    <input
                        type="number"
                        min={0}
                        value={form.reservedQuantity}
                        onChange={(e) => setForm({ ...form, reservedQuantity: Number(e.target.value) })}
                        disabled={!product}
                        className="w-full px-3 py-2 border rounded-lg disabled:bg-gray-100 disabled:text-gray-400"
                    />
                    <p className="text-xs text-gray-400 mt-1">
                        {product
                            ? `Доступно к продаже: ${Math.max(0, form.stockQuantity - form.reservedQuantity)}`
                            : 'Для нового товара резерв не задаётся — он формируется заказами.'}
                    </p>
                </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
                <div>
                    <label className="block text-sm font-medium mb-1">Категория</label>
                    <select
                        value={form.categoryId ?? ''}
                        onChange={(e) =>
                            setForm({
                                ...form,
                                categoryId: e.target.value ? Number(e.target.value) : undefined,
                            })
                        }
                        className="w-full px-3 py-2 border rounded-lg"
                    >
                        <option value="">— Без категории —</option>
                        {currentCategoryMissing && (
                            <option value={form.categoryId}>
                                {product?.categoryName ?? `#${form.categoryId}`}
                            </option>
                        )}
                        {categories.map((c) => (
                            <option key={c.id} value={c.id}>
                                {c.name}
                            </option>
                        ))}
                    </select>
                </div>
                <div>
                    <label className="block text-sm font-medium mb-1">SKU (артикул)</label>
                    <input
                        type="text"
                        value={form.sku}
                        onChange={(e) => setForm({ ...form, sku: e.target.value })}
                        disabled={!product}
                        placeholder={product ? '' : 'Задаётся при редактировании'}
                        className="w-full px-3 py-2 border rounded-lg disabled:bg-gray-100 disabled:text-gray-400"
                    />
                </div>
            </div>

            <ProductMediaEditor
                images={product?.imageUrls ?? []}
                videos={product?.videoUrls ?? []}
                newFiles={media.newFiles}
                removedUrls={media.removedUrls}
                onChange={setMedia}
                maxFiles={8}
            />

            <div className="flex gap-2 pt-4">
                <button
                    type="submit"
                    disabled={isSubmitting}
                    className="flex-1 px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50"
                >
                    {isSubmitting ? 'Сохранение...' : 'Сохранить'}
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
