import { useParams, Link } from 'react-router'
import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { getProductById } from '../api/productApi'
import StarRating from '@/shared/ui/StarRating'
import ReviewList from '@/features/reviews/components/ReviewList'

export default function ProductDetailsPage() {
    const { id } = useParams<{ id: string }>()
    const productId = Number(id)
    const [selectedMedia, setSelectedMedia] = useState(0)

    const { data: product, isLoading, error } = useQuery({
        queryKey: ['product', productId],
        queryFn: () => getProductById(productId),
        enabled: !!productId,
    })

    if (isLoading) return <div className="text-center py-20">Загрузка...</div>
    if (error || !product) return <div className="text-center py-20 text-red-500">Товар не найден</div>

    const allMedia = [
        ...product.imageUrls.map((url) => ({ type: 'image' as const, url })),
        ...product.videoUrls.map((url) => ({ type: 'video' as const, url })),
    ]

    return (
        <div>
            {/* Breadcrumbs */}
            <div className="text-sm text-gray-500 mb-4">
                <Link to="/" className="hover:text-blue-600">Товары</Link>
                {product.categoryName && (
                    <> / <span>{product.categoryName}</span></>
                )}
                {' '} / <span>{product.name}</span>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                {/* Медиа */}
                <div>
                    {/* Главное медиа */}
                    <div className="aspect-square bg-gray-100 rounded-lg overflow-hidden mb-4">
                        {allMedia.length > 0 ? (
                            allMedia[selectedMedia].type === 'image' ? (
                                <img
                                    src={allMedia[selectedMedia].url}
                                    alt={product.name}
                                    className="w-full h-full object-contain"
                                />
                            ) : (
                                <video
                                    src={allMedia[selectedMedia].url}
                                    controls
                                    className="w-full h-full object-contain"
                                />
                            )
                        ) : (
                            <div className="w-full h-full flex items-center justify-center text-gray-400">
                                Нет изображения
                            </div>
                        )}
                    </div>

                    {/* Thumbnails */}
                    {allMedia.length > 1 && (
                        <div className="grid grid-cols-5 gap-2">
                            {allMedia.map((media, idx) => (
                                <button
                                    key={idx}
                                    onClick={() => setSelectedMedia(idx)}
                                    className={`aspect-square rounded overflow-hidden border-2 ${idx === selectedMedia ? 'border-blue-500' : 'border-transparent'
                                        }`}
                                >
                                    {media.type === 'image' ? (
                                        <img src={media.url} className="w-full h-full object-cover" />
                                    ) : (
                                        <div className="w-full h-full bg-gray-800 flex items-center justify-center text-white">
                                            🎥
                                        </div>
                                    )}
                                </button>
                            ))}
                        </div>
                    )}
                </div>

                {/* Инфо */}
                <div>
                    <h1 className="text-3xl font-bold mb-2">{product.name}</h1>

                    {/* Рейтинг */}
                    <div className="flex items-center gap-2 mb-4">
                        <StarRating rating={product.averageRating} size="md" showValue />
                        <span className="text-gray-500 text-sm">
                            ({product.reviewCount} отзывов)
                        </span>
                    </div>

                    {/* Цена */}
                    <div className="text-3xl font-bold text-blue-600 mb-4">
                        {product.price.toLocaleString('ru-RU')} ₽
                    </div>

                    {/* Наличие */}
                    <div className={`mb-4 ${product.availableQuantity > 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {product.availableQuantity > 0
                            ? `✅ В наличии: ${product.availableQuantity} шт.`
                            : '❌ Нет в наличии'}
                    </div>

                    {/* SKU */}
                    {product.sku && (
                        <div className="text-sm text-gray-500 mb-4">
                            Артикул: {product.sku}
                        </div>
                    )}

                    {/* Кнопки */}
                    <div className="flex gap-2 mb-6">
                        <button
                            className="flex-1 px-6 py-3 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50"
                            disabled={product.availableQuantity === 0}
                        >
                            🛒 Добавить в корзину
                        </button>
                    </div>

                    {/* Описание */}
                    {product.description && (
                        <div className="border-t pt-4">
                            <h2 className="font-bold mb-2">Описание</h2>
                            <p className="text-gray-700">{product.description}</p>
                        </div>
                    )}

                    {/* Покупки */}
                    {product.totalPurchases > 0 && (
                        <div className="mt-4 text-sm text-gray-500">
                            🛒 Куплено: {product.totalPurchases} раз
                        </div>
                    )}
                </div>
            </div>

            {/* Отзывы */}
            <div className="mt-12">
                <h2 className="text-2xl font-bold mb-4">Отзывы ({product.reviewCount})</h2>
                <ReviewList productId={productId} />
            </div>
        </div>
    )
}