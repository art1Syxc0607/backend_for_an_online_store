import { useState } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { useParams, useNavigate, Link } from 'react-router'
import {
    getAdminProductById,
    updateProduct,
    uploadProductFiles,
    deleteProductFiles,
} from '../api/adminProductsApi'
import { getAllReviews } from '../api/adminReviewsApi'
import AdminModal from '../components/AdminModal'
import ProductForm from '../components/ProductForm'
import StarRating from '@/shared/ui/StarRating'

export default function AdminProductDetailPage() {
    const { id } = useParams<{ id: string }>()
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const [isEditing, setIsEditing] = useState(false)

    const { data: product, isLoading, error } = useQuery({
        queryKey: ['admin', 'product', id],
        queryFn: () => getAdminProductById(Number(id)),
        enabled: !!id,
    })

    if (isLoading) {
        return <div className="text-center py-20 text-gray-500">Загрузка...</div>
    }

    if (error || !product) {
        return (
            <div className="text-center py-20">
                <p className="text-red-500 mb-4">Товар не найден</p>
                <button
                    onClick={() => navigate('/admin/products')}
                    className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                >
                    ← К списку товаров
                </button>
            </div>
        )
    }

    const images = product.imageUrls ?? []
    const videos = product.videoUrls ?? []

    return (
        <div>
            <div className="flex items-center justify-between mb-6">
                <div>
                    <Link to="/admin/products" className="text-blue-500 hover:underline text-sm">
                        ← К списку товаров
                    </Link>
                    <h1 className="text-3xl font-bold mt-2">{product.name}</h1>
                </div>
                <div className="flex gap-2">
                    <button
                        onClick={() => setIsEditing(true)}
                        className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                    >
                        ✏️ Редактировать
                    </button>
                    <button
                        onClick={() => navigate('/admin/products')}
                        className="px-4 py-2 bg-gray-200 rounded-lg hover:bg-gray-300"
                    >
                        Закрыть
                    </button>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Фото и видео */}
                <div className="lg:col-span-1">
                    <div className="bg-white rounded-lg shadow p-4">
                        <h3 className="font-medium mb-3">Фото ({images.length})</h3>
                        {images.length > 0 ? (
                            <div className="grid grid-cols-2 gap-2">
                                {images.map((url, i) => (
                                    <img
                                        key={i}
                                        src={url}
                                        alt={`${product.name} ${i + 1}`}
                                        className="w-full h-32 object-cover rounded-lg"
                                    />
                                ))}
                            </div>
                        ) : (
                            <div className="w-full h-40 bg-gray-100 rounded-lg flex items-center justify-center text-gray-400">
                                Нет фото
                            </div>
                        )}

                        {videos.length > 0 && (
                            <>
                                <h3 className="font-medium mt-4 mb-3">Видео ({videos.length})</h3>
                                <div className="space-y-2">
                                    {videos.map((url, i) => (
                                        <video
                                            key={i}
                                            src={url}
                                            controls
                                            className="w-full rounded-lg"
                                        />
                                    ))}
                                </div>
                            </>
                        )}
                    </div>
                </div>

                {/* Основная информация */}
                <div className="lg:col-span-2 space-y-4">
                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Основная информация</h3>
                        <dl className="grid grid-cols-2 gap-4 text-sm">
                            <div>
                                <dt className="text-gray-500">ID</dt>
                                <dd className="font-medium">#{product.id}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Категория</dt>
                                <dd className="font-medium">{product.categoryName || '—'}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Цена для покупателей</dt>
                                <dd className="font-medium text-green-600">
                                    {product.price.toLocaleString('ru-RU')} ₽
                                </dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Цена закупки</dt>
                                <dd className="font-medium text-red-600">
                                    {product.purchasePrice.toLocaleString('ru-RU')} ₽
                                </dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Маржа</dt>
                                <dd className="font-medium">
                                    {(product.price - product.purchasePrice).toLocaleString('ru-RU')} ₽
                                </dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">SKU</dt>
                                <dd className="font-medium">—</dd>
                            </div>
                        </dl>
                    </div>

                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Описание</h3>
                        <p className="text-gray-700 whitespace-pre-wrap">
                            {product.description || 'Нет описания'}
                        </p>
                    </div>

                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Склад</h3>
                        <dl className="grid grid-cols-3 gap-4 text-sm">
                            <div>
                                <dt className="text-gray-500">На складе</dt>
                                <dd className="font-medium text-lg">{product.stockQuantity}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Зарезервировано</dt>
                                <dd className="font-medium text-lg text-yellow-600">
                                    {product.reservedQuantity}
                                </dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Доступно</dt>
                                <dd className={`font-medium text-lg ${
                                    product.availableQuantity > 10 ? 'text-green-600' :
                                    product.availableQuantity > 0 ? 'text-yellow-600' : 'text-red-600'
                                }`}>
                                    {product.availableQuantity}
                                </dd>
                            </div>
                        </dl>
                    </div>

                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Рейтинг и отзывы</h3>
                        <div className="flex items-center gap-4">
                            <StarRating rating={product.averageRating} size="lg" showValue />
                            <span className="text-gray-500">
                                {product.countOfReviews} отзывов
                            </span>
                        </div>

                        <ProductReviews productId={product.id} />
                    </div>

                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Статистика продаж</h3>
                        <dl className="grid grid-cols-2 gap-4 text-sm">
                            <div>
                                <dt className="text-gray-500">Всего в заказах</dt>
                                <dd className="font-medium">{product.countOfOrdersContainThisProduct}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Оплачено единиц</dt>
                                <dd className="font-medium text-green-600">{product.amountOfPaid}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Получено единиц</dt>
                                <dd className="font-medium">{product.amountOfRecieved}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Отменено единиц</dt>
                                <dd className="font-medium text-red-600">{product.amountOfCanceled}</dd>
                            </div>
                        </dl>
                    </div>

                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Даты</h3>
                        <dl className="grid grid-cols-2 gap-4 text-sm">
                            <div>
                                <dt className="text-gray-500">Создан</dt>
                                <dd className="font-medium">
                                    {new Date(product.createdAt).toLocaleString('ru-RU')}
                                </dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Обновлён</dt>
                                <dd className="font-medium">
                                    {new Date(product.updatedAt).toLocaleString('ru-RU')}
                                </dd>
                            </div>
                        </dl>
                    </div>
                </div>
            </div>

            {/* Модалка редактирования товара */}
            <AdminModal
                isOpen={isEditing}
                title="Редактировать товар"
                onClose={() => setIsEditing(false)}
                maxWidth="lg"
            >
                <ProductForm
                    product={product}
                    onSubmit={async (data, media) => {
                        await updateProduct({ id: product.id, ...data })
                        if (media.removedUrls.length)
                            await deleteProductFiles(product.id, media.removedUrls)
                        if (media.newFiles.length)
                            await uploadProductFiles(product.id, media.newFiles)

                        await queryClient.invalidateQueries({ queryKey: ['admin', 'product', id] })
                        queryClient.invalidateQueries({ queryKey: ['admin', 'products'] })
                        setIsEditing(false)
                    }}
                    onCancel={() => setIsEditing(false)}
                />
            </AdminModal>
        </div>
    )
}

// ✅ Отзывы товара с пагинацией и поиском (для админки)
const REVIEWS_PAGE_SIZE = 5

function ProductReviews({ productId }: { productId: number }) {
    const [page, setPage] = useState(1)
    const [searchTerm, setSearchTerm] = useState('')

    const reviewsQuery = useQuery({
        queryKey: ['admin', 'product-reviews', productId, searchTerm, page],
        queryFn: () => getAllReviews({
            productId,
            searchTerm: searchTerm || undefined,
            pageNumber: page,
            pageSize: REVIEWS_PAGE_SIZE,
            descending: true,
        }),
        placeholderData: (prev) => prev,
    })

    const data = reviewsQuery.data
    const reviews = data?.items ?? []
    const totalPages = data?.totalPages ?? 0

    return (
        <div className="mt-6 border-t pt-4">
            <input
                type="text"
                value={searchTerm}
                onChange={(e) => { setSearchTerm(e.target.value); setPage(1) }}
                placeholder="Поиск по тексту отзыва или имени пользователя..."
                className="w-full px-3 py-2 border rounded-lg text-sm mb-4"
            />

            {reviewsQuery.isLoading ? (
                <div className="text-center py-4 text-gray-500">Загрузка...</div>
            ) : reviews.length === 0 ? (
                <p className="text-sm text-gray-400 py-4">Отзывов нет</p>
            ) : (
                <div className="space-y-4">
                    {reviews.map((r) => (
                        <div key={r.id} className="border-b pb-4 last:border-0 last:pb-0">
                            <div className="flex items-center justify-between gap-2">
                                <a
                                    href={`/admin/users/${r.userId}`}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="font-medium text-blue-500 hover:underline"
                                >
                                    {r.userName || `Пользователь #${r.userId}`} ↗
                                </a>
                                <StarRating rating={r.rating} size="sm" />
                            </div>
                            <p className="text-sm text-gray-700 mt-2 whitespace-pre-wrap">{r.text}</p>
                            {r.adminResponse && (
                                <div className="mt-3 ml-4 p-3 bg-blue-50 rounded-lg text-sm">
                                    <div className="text-xs text-blue-600 font-medium mb-1">
                                        Ответ администратора
                                    </div>
                                    <div className="text-gray-700 whitespace-pre-wrap">{r.adminResponse}</div>
                                </div>
                            )}
                            <div className="text-xs text-gray-400 mt-2">
                                {new Date(r.createdAt).toLocaleString('ru-RU')}
                            </div>
                        </div>
                    ))}
                </div>
            )}

            {data && totalPages > 1 && (
                <div className="flex justify-center items-center gap-2 mt-4">
                    <button
                        onClick={() => setPage((p) => Math.max(1, p - 1))}
                        disabled={page <= 1}
                        className="px-3 py-1 bg-gray-200 rounded disabled:opacity-50 text-sm"
                    >
                        ←
                    </button>
                    <span className="text-sm">
                        Страница {data.pageNumber} из {data.totalPages}
                    </span>
                    <button
                        onClick={() => setPage((p) => p + 1)}
                        disabled={page >= totalPages}
                        className="px-3 py-1 bg-gray-200 rounded disabled:opacity-50 text-sm"
                    >
                        →
                    </button>
                </div>
            )}
        </div>
    )
}
