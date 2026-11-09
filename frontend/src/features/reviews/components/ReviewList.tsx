import { useEffect, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { getProductReviews } from '../api/reviewApi'
import type { GetProductReviewsParams } from '../api/reviewApi'
import ReviewFilters from './ReviewFilters'
import EditReviewForm from './EditReviewForm'
import ReviewCard from './ReviewCard'
import ConfirmDialog from '@/shared/ui/ConfirmDialog'
import { useAuth } from '@/features/auth/hooks/useAuth'
import { useDeleteReview, useUserReviews } from '../hooks/useUserReviews'

interface Props {
    productId: number
    /** Отзыв, который нужно подсветить и прокрутить к нему (из личного кабинета). */
    highlightReviewId?: number
}

export default function ReviewList({ productId, highlightReviewId }: Props) {
    const { user } = useAuth()
    const [pageNumber, setPageNumber] = useState(1)
    const [editingReviewId, setEditingReviewId] = useState<number | null>(null)
    const [reviewToDelete, setReviewToDelete] = useState<number | null>(null)
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

    // Отзывы текущего пользователя нужны, чтобы показать выбранный отзыв,
    // даже если он не попал на текущую страницу или в фильтр.
    const { data: myReviews } = useUserReviews(!!user && !!highlightReviewId)
    const deleteMutation = useDeleteReview()

    const reviews = data?.items ?? []
    const highlightedReview = highlightReviewId
        ? myReviews?.find((review) => review.id === highlightReviewId)
        : undefined
    const isHighlightOnPage = reviews.some(
        (review) => review.id === highlightReviewId,
    )

    useEffect(() => {
        if (!highlightReviewId) return

        const element = document.getElementById(`review-${highlightReviewId}`)
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'center' })
        }
    }, [highlightReviewId, highlightedReview, data])

    const handleApplyFilters = (newFilters: GetProductReviewsParams) => {
        setFilters(newFilters)
        setPageNumber(1)
        setEditingReviewId(null)
    }

    const handleDelete = () => {
        if (reviewToDelete === null) return

        deleteMutation.mutate(
            { reviewId: reviewToDelete, productId },
            { onSuccess: () => setReviewToDelete(null) },
        )
    }

    if (isLoading) return <div className="text-center py-8">Загрузка отзывов...</div>
    if (error) return <div className="text-center py-8 text-red-500">Ошибка загрузки отзывов</div>

    return (
        <div>
            {/* ✅ Фильтры */}
            <ReviewFilters
                filters={filters}
                onApply={handleApplyFilters}
                totalCount={data?.totalCount ?? 0}
            />

            {deleteMutation.isError && (
                <div className="mt-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded text-sm">
                    Не удалось удалить отзыв. Попробуйте ещё раз.
                </div>
            )}

            {/* Выбранный отзыв, которого нет на текущей странице */}
            {highlightedReview && !isHighlightOnPage && (
                <div className="mt-4">
                    <div className="text-sm font-medium text-amber-700 mb-2">
                        Ваш отзыв
                    </div>
                    <ReviewCard
                        id={`review-${highlightedReview.id}`}
                        review={highlightedReview}
                        highlighted
                        actions={
                            <button
                                type="button"
                                onClick={() => setReviewToDelete(highlightedReview.id)}
                                className="text-xs px-2 py-1 rounded border border-red-200 text-red-600 hover:bg-red-50 whitespace-nowrap"
                            >
                                🗑 Удалить
                            </button>
                        }
                    />
                </div>
            )}

            {/* Список отзывов */}
            {reviews.length === 0 && !highlightedReview ? (
                <div className="text-gray-500 py-8 text-center">
                    Отзывов пока нет
                </div>
            ) : (
                <div className="space-y-4 mt-4">
                    {reviews.map((review) => {
                        const isOwner = user?.id === review.userId
                        const isEditing = editingReviewId === review.id

                        if (isEditing) {
                            return (
                                <div
                                    key={review.id}
                                    id={`review-${review.id}`}
                                    className="bg-white rounded-lg shadow p-4"
                                >
                                    <EditReviewForm
                                        review={review}
                                        onCancel={() => setEditingReviewId(null)}
                                        onSaved={() => setEditingReviewId(null)}
                                    />
                                </div>
                            )
                        }

                        return (
                            <ReviewCard
                                key={review.id}
                                id={`review-${review.id}`}
                                review={review}
                                highlighted={review.id === highlightReviewId}
                                actions={
                                    isOwner ? (
                                        <div className="flex items-center gap-1.5">
                                            <button
                                                type="button"
                                                onClick={() => setEditingReviewId(review.id)}
                                                className="text-xs px-2 py-1 rounded border border-blue-200 text-blue-600 hover:bg-blue-50 whitespace-nowrap"
                                            >
                                                ✏️ Редактировать
                                            </button>
                                            <button
                                                type="button"
                                                onClick={() => setReviewToDelete(review.id)}
                                                className="text-xs px-2 py-1 rounded border border-red-200 text-red-600 hover:bg-red-50 whitespace-nowrap"
                                            >
                                                🗑 Удалить
                                            </button>
                                        </div>
                                    ) : undefined
                                }
                            />
                        )
                    })}
                </div>
            )}

            {/* Пагинация */}
            {data && data.totalPages > 1 && (
                <div className="flex justify-center items-center gap-2 mt-6">
                    <button
                        onClick={() => {
                            setEditingReviewId(null)
                            setPageNumber((p) => Math.max(1, p - 1))
                        }}
                        disabled={!data.hasPreviousPage}
                        className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50 hover:bg-gray-300"
                    >
                        ←
                    </button>
                    <span className="text-sm text-gray-600">
                        {data.pageNumber} / {data.totalPages}
                    </span>
                    <button
                        onClick={() => {
                            setEditingReviewId(null)
                            setPageNumber((p) => p + 1)
                        }}
                        disabled={!data.hasNextPage}
                        className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50 hover:bg-gray-300"
                    >
                        →
                    </button>
                </div>
            )}

            <ConfirmDialog
                isOpen={reviewToDelete !== null}
                title="Удалить отзыв?"
                message="Отзыв будет удалён без возможности восстановления."
                confirmLabel="Да, удалить"
                cancelLabel="Отмена"
                onConfirm={handleDelete}
                onCancel={() => setReviewToDelete(null)}
                isLoading={deleteMutation.isPending}
            />
        </div>
    )
}
