import { apiClient } from '@/shared/api/client'
import type { PagedResult } from '@/shared/api/types'
import type { ReviewDto } from '@/features/reviews/api/reviewApi'

export interface GetAllReviewsParams {
    isResponded?: boolean
    productId?: number
    userId?: number
    sortReviewBy?: number
    descending?: boolean
    pageNumber?: number
    pageSize?: number
    searchTerm?: string
    minRating?: number
    maxRating?: number
    status?: number
}

export async function getAllReviews(
    params: GetAllReviewsParams = {}
): Promise<PagedResult<ReviewDto>> {
    const { data } = await apiClient.get<PagedResult<ReviewDto>>(
        '/admin/reviews',
        { params }
    )
    return data
}

export async function respondToReview(
    reviewId: number,
    response: string
): Promise<void> {
    await apiClient.post(`/admin/reviews/${reviewId}/respond`, { response })
}

export async function updateReviewResponse(
    reviewId: number,
    newText: string
): Promise<void> {
    await apiClient.put('/admin/reviews/response', { reviewId, newText })
}

export async function removeReviewResponse(reviewId: number): Promise<void> {
    await apiClient.delete(`/admin/reviews/${reviewId}/response`)
}