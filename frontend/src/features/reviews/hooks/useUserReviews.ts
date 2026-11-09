import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { deleteReview, getUserReviews } from '../api/reviewApi'

/** Отзывы текущего пользователя для личного кабинета и подсветки на странице товара. */
export function useUserReviews(enabled = true) {
    return useQuery({
        queryKey: ['userReviews'],
        queryFn: getUserReviews,
        enabled,
    })
}

interface DeleteReviewVariables {
    reviewId: number
    productId?: number
}

/** Удаление отзыва с обновлением связанных кэшей. */
export function useDeleteReview() {
    const queryClient = useQueryClient()

    return useMutation({
        mutationFn: ({ reviewId }: DeleteReviewVariables) => deleteReview(reviewId),
        onSuccess: (_data, variables) => {
            queryClient.invalidateQueries({ queryKey: ['userReviews'] })
            queryClient.invalidateQueries({ queryKey: ['reviews'] })
            queryClient.invalidateQueries({ queryKey: ['products'] })

            if (variables.productId !== undefined) {
                queryClient.invalidateQueries({
                    queryKey: ['product', variables.productId],
                })
            } else {
                queryClient.invalidateQueries({ queryKey: ['product'] })
            }
        },
    })
}
