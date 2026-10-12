import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
// ✅ Стало
import { getProductReviews } from '../api/reviewApi'
import type { GetProductReviewsParams } from '../api/reviewApi'
import StarRating from '@/shared/ui/StarRating'
import ReviewFilters from './ReviewFilters'

interface Props {
    productId: number
}

export default function ReviewList({ productId }: Props) {
    const [pageNumber, setPageNumber] = useState(1)
    const [filters, setFilters] = useState<GetProductReviewsParams>({
        sortBy: 'DateOfCreation',
        descending: true,
    })

    const { data, isLoading, error } = useQuery({
        queryKey: ['reviews', productId, pageNumber, filters],
        queryFn: () => getProductReviews(productId, {
            ...filters,
            pageNumber,
            pageSize: 10,
        }),
    })

    if (isLoading) return <div className="text-center py-8">Загрузка отзывов...</div>
    if (error) return <div className="text-center py-8 text-red-500">Ошибка загрузки отзывов</div>

    const handleApplyFilters = (newFilters: GetProductReviewsParams) => {
        setFilters(newFilters)
        setPageNumber(1)
    }

    const reviews = data?.items ?? []

    return (
        <div>
            {/* ✅ Фильтры */}
            <ReviewFilters
                filters={filters}
                onApply={handleApplyFilters}
                totalCount={data?.totalCount ?? 0}
            />

            {/* Список отзывов */}
            {reviews.length === 0 ? (
                <div className="text-gray-500 py-8 text-center">
                    Отзывов пока нет
                </div>
            ) : (
                <div className="space-y-4 mt-4">
                    {reviews.map((review) => (
                        <div key={review.id} className="bg-white rounded-lg shadow p-4">
                            {/* Header */}
                            <div className="flex items-center justify-between mb-2">
                                <div className="flex items-center gap-2">
                                    <span className="font-bold">{review.userName || 'Аноним'}</span>
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

                            {/* Рейтинг */}
                            <StarRating rating={review.rating || 0} size="sm" />

                            {/* Текст */}
                            <p className="mt-2 text-gray-700">{review.text}</p>

                            {/* ✅ Фото — imagesUrls */}
                            {review.imagesUrls && review.imagesUrls.length > 0 && (
                                <div className="flex gap-2 mt-3 flex-wrap">
                                    {review.imagesUrls.map((url, i) => (
                                        <img
                                            key={i}
                                            src={url}
                                            alt={`Фото ${i + 1}`}
                                            className="w-24 h-24 object-cover rounded cursor-pointer hover:opacity-80 border"
                                            onClick={() => window.open(url, '_blank')}
                                            onError={(e) => {
                                                e.currentTarget.style.display = 'none'
                                            }}
                                        />
                                    ))}
                                </div>
                            )}

                            {/* ✅ Видео — videosUrls */}
                            {review.videosUrls && review.videosUrls.length > 0 && (
                                <div className="flex gap-2 mt-3 flex-wrap">
                                    {review.videosUrls.map((url, i) => (
                                        <video
                                            key={i}
                                            src={url}
                                            controls
                                            className="w-40 h-24 object-cover rounded border"
                                        />
                                    ))}
                                </div>
                            )}

                            {/* Ответ админа */}
                            {review.adminResponse && (
                                <div className="mt-3 ml-4 p-3 bg-blue-50 rounded border-l-4 border-blue-500">
                                    <div className="text-xs font-bold text-blue-700 mb-1">
                                        💬 Ответ магазина
                                    </div>
                                    <p className="text-sm text-gray-700">{review.adminResponse}</p>
                                    {review.adminResponseAt && (
                                        <div className="text-xs text-gray-500 mt-1">
                                            {new Date(review.adminResponseAt).toLocaleDateString('ru-RU')}
                                        </div>
                                    )}
                                </div>
                            )}
                        </div>
                    ))}
                </div>
            )}

            {/* Пагинация */}
            {data && data.totalPages > 1 && (
                <div className="flex justify-center items-center gap-2 mt-6">
                    <button
                        onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                        disabled={!data.hasPreviousPage}
                        className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50 hover:bg-gray-300"
                    >
                        ←
                    </button>
                    <span className="text-sm text-gray-600">
                        {data.pageNumber} / {data.totalPages}
                    </span>
                    <button
                        onClick={() => setPageNumber((p) => p + 1)}
                        disabled={!data.hasNextPage}
                        className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50 hover:bg-gray-300"
                    >
                        →
                    </button>
                </div>
            )}
        </div>
    )
}