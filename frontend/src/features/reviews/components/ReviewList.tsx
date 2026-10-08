import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getProductReviews } from '../api/reviewApi'
import StarRating from '@/shared/ui/StarRating'
// ✅ Здесь нет импорта типов — всё ок

interface Props {
    productId: number
}

export default function ReviewList({ productId }: Props) {
    const [pageNumber, setPageNumber] = useState(1)

    const { data, isLoading } = useQuery({
        queryKey: ['reviews', productId, pageNumber],
        queryFn: () => getProductReviews(productId, pageNumber, 10),
    })

    if (isLoading) return <div>Загрузка отзывов...</div>
    if (!data || data.items.length === 0) {
        return <div className="text-gray-500 py-8">Пока нет отзывов</div>
    }

    return (
        <div>
            <div className="space-y-4">
                {data.items.map((review) => (
                    <div key={review.id} className="bg-white rounded-lg shadow p-4">
                        <div className="flex items-center justify-between mb-2">
                            <div className="flex items-center gap-2">
                                <span className="font-bold">{review.userName}</span>
                                {review.isVerifiedPurchase && (
                                    <span className="text-xs bg-green-100 text-green-700 px-2 py-0.5 rounded">
                                        ✅ Купил
                                    </span>
                                )}
                            </div>
                            <span className="text-xs text-gray-500">
                                {new Date(review.createdAt).toLocaleDateString('ru-RU')}
                            </span>
                        </div>
                        <StarRating rating={review.rating} size="sm" />
                        <p className="mt-2 text-gray-700">{review.text}</p>

                        {/* Медиа отзыва */}
                        {review.imageUrls.length > 0 && (
                            <div className="flex gap-2 mt-2">
                                {review.imageUrls.map((url, i) => (
                                    <img key={i} src={url} className="w-20 h-20 object-cover rounded" />
                                ))}
                            </div>
                        )}

                        {/* Ответ админа */}
                        {review.adminResponse && (
                            <div className="mt-3 ml-4 p-3 bg-blue-50 rounded border-l-4 border-blue-500">
                                <div className="text-xs font-bold text-blue-700 mb-1">
                                    Ответ магазина
                                </div>
                                <p className="text-sm text-gray-700">{review.adminResponse}</p>
                            </div>
                        )}
                    </div>
                ))}
            </div>

            {/* Пагинация */}
            <div className="flex justify-center items-center gap-2 mt-6">
                <button
                    onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                    disabled={!data.hasPreviousPage}
                    className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
                >
                    ←
                </button>
                <span className="text-sm">
                    {data.pageNumber} / {data.totalPages}
                </span>
                <button
                    onClick={() => setPageNumber((p) => p + 1)}
                    disabled={!data.hasNextPage}
                    className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
                >
                    →
                </button>
            </div>
        </div>
    )
}