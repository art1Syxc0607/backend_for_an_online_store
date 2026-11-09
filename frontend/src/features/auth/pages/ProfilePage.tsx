import { useState } from 'react'
import { useNavigate } from 'react-router'
import { useAuth } from '../hooks/useAuth'
import { useDeleteReview, useUserReviews } from '@/features/reviews/hooks/useUserReviews'
import ReviewCard from '@/features/reviews/components/ReviewCard'
import ConfirmDialog from '@/shared/ui/ConfirmDialog'
import type { ReviewDto } from '@/features/reviews/api/reviewApi'

export default function ProfilePage() {
    const { user, isLoadingProfile, logout } = useAuth()
    const navigate = useNavigate()
    const reviewsQuery = useUserReviews(!!user)
    const deleteMutation = useDeleteReview()
    const [reviewToDelete, setReviewToDelete] = useState<ReviewDto | null>(null)

    if (isLoadingProfile) return <div className="text-center py-20">Загрузка...</div>
    if (!user) return <div className="text-center py-20">Не авторизован</div>

    const reviews = reviewsQuery.data ?? []

    return (
        <div className="max-w-2xl mx-auto">
            <h1 className="text-3xl font-bold mb-6">Личный кабинет</h1>

            {/* Профиль */}
            <div className="bg-white rounded-lg shadow p-6 mb-4">
                <h2 className="font-bold mb-4">Профиль</h2>
                <div className="space-y-2">
                    <div className="flex justify-between">
                        <span className="text-gray-600">Имя:</span>
                        <span className="font-medium">{user.userName}</span>
                    </div>
                    <div className="flex justify-between">
                        <span className="text-gray-600">Email:</span>
                        <span className="font-medium">{user.email}</span>
                    </div>
                    <div className="flex justify-between">
                        <span className="text-gray-600">Роль:</span>
                        <span className="font-medium">{user.role}</span>
                    </div>
                    <div className="flex justify-between">
                        <span className="text-gray-600">Email подтверждён:</span>
                        <span>{user.isEmailConfirmed ? '✅' : '❌'}</span>
                    </div>
                    <div className="flex justify-between">
                        <span className="text-gray-600">Дата регистрации:</span>
                        <span>{new Date(user.createdAt).toLocaleDateString('ru-RU')}</span>
                    </div>
                </div>
            </div>

            {/* Мои отзывы */}
            <div className="bg-white rounded-lg shadow p-6 mb-4">
                <div className="flex items-center justify-between mb-4">
                    <h2 className="font-bold">Мои отзывы</h2>
                    {reviews.length > 0 && (
                        <span className="text-sm text-gray-500">
                            {reviews.length}
                        </span>
                    )}
                </div>

                {reviewsQuery.isLoading && (
                    <p className="text-sm text-gray-500">Загрузка отзывов...</p>
                )}

                {reviewsQuery.isError && (
                    <p className="text-sm text-red-600">
                        Не удалось загрузить отзывы. Попробуйте обновить страницу.
                    </p>
                )}

                {!reviewsQuery.isLoading && !reviewsQuery.isError && reviews.length === 0 && (
                    <p className="text-sm text-gray-500">
                        Вы ещё не оставляли отзывы.
                    </p>
                )}

                {deleteMutation.isError && (
                    <div className="mb-3 p-3 bg-red-100 border border-red-400 text-red-700 rounded text-sm">
                        Не удалось удалить отзыв. Попробуйте ещё раз.
                    </div>
                )}

                {reviews.length > 0 && (
                    <div className="space-y-3">
                        {reviews.map((review) => (
                            <ReviewCard
                                key={review.id}
                                review={review}
                                compact
                                showProductName
                                onCardClick={() =>
                                    navigate(
                                        `/products/${review.productId}?reviewId=${review.id}`,
                                    )
                                }
                                actions={
                                    <button
                                        type="button"
                                        onClick={(e) => {
                                            e.stopPropagation()
                                            setReviewToDelete(review)
                                        }}
                                        className="text-xs px-2 py-1 rounded border border-red-200 text-red-600 hover:bg-red-50 whitespace-nowrap"
                                    >
                                        🗑 Удалить
                                    </button>
                                }
                            />
                        ))}
                    </div>
                )}
            </div>

            <button
                onClick={logout}
                className="w-full px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600"
            >
                Выйти
            </button>

            <ConfirmDialog
                isOpen={!!reviewToDelete}
                title="Удалить отзыв?"
                message={
                    reviewToDelete
                        ? `Отзыв на «${reviewToDelete.productName}» будет удалён без возможности восстановления.`
                        : undefined
                }
                confirmLabel="Да, удалить"
                cancelLabel="Отмена"
                onConfirm={() => {
                    if (!reviewToDelete) return

                    deleteMutation.mutate(
                        {
                            reviewId: reviewToDelete.id,
                            productId: reviewToDelete.productId,
                        },
                        { onSuccess: () => setReviewToDelete(null) },
                    )
                }}
                onCancel={() => setReviewToDelete(null)}
                isLoading={deleteMutation.isPending}
            />
        </div>
    )
}
