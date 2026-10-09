import { apiClient } from '@/shared/api/client'
import type { PagedResult } from '@/shared/api/types'  // ✅

export interface ReviewDto {
    id: number
    userId: number
    userName: string
    productId: number
    productName: string
    text: string
    rating: number
    imageUrls?: string[]   // ✅ опционально
    videoUrls?: string[]   // ✅ опционально
    adminResponse?: string
    adminResponseAt?: string
    isVerifiedPurchase: boolean
    createdAt: string
}

export async function getProductReviews(
    productId: number,
    pageNumber = 1,
    pageSize = 10
): Promise<PagedResult<ReviewDto>> {
    const { data } = await apiClient.get<PagedResult<ReviewDto>>(
        `/review/${productId}`,
        { params: { pageNumber, pageSize } }
    )
    return data
}