import { apiClient } from '@/shared/api/client'
import type { PagedResult } from '@/shared/api/types'

export interface ReviewDto {
    id: number
    userId: number
    userName: string
    productId: number
    productName: string
    text: string
    rating: number
    imagesUrls?: string[]   // ✅ с "s"
    videosUrls?: string[]   // ✅ с "s"
    isVerifiedPurchase?: boolean
    status: string | number
    adminResponse?: string
    adminResponseAt?: string
    createdAt: string
    updatedAt: string
}

export interface GetProductReviewsParams {
    rating?: number
    minRating?: number
    hasMedia?: boolean
    isVerifiedPurchase?: boolean
    hasAdminResponse?: boolean
    fromDate?: string
    toDate?: string
    sortBy?: string
    descending?: boolean
    pageNumber?: number
    pageSize?: number
}

export async function getProductReviews(
    productId: number,
    params: GetProductReviewsParams = {}
): Promise<PagedResult<ReviewDto>> {
    const { data } = await apiClient.get<PagedResult<ReviewDto>>(
        `/review/${productId}`,
        { params }
    )
    return data
}

export interface AddReviewRequest {
    productId: number
    text: string
    rating: number
    files?: File[]
}

export async function addReview(
    productId: number,
    text: string,
    rating: number,
    files?: File[]
): Promise<number> {
    const formData = new FormData()
    formData.append('ProductId', productId.toString())
    formData.append('Text', text)
    formData.append('Rating', rating.toString())

    if (files && files.length > 0) {
        files.forEach((file) => formData.append('Files', file))
    }

    const { data } = await apiClient.post<number>('/review', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
    })
    return data
}

export interface UploadedReviewFileDto {
    originalFileName: string
    fileUrl: string
    contentType: string
    size: number
}

export interface DeleteReviewFilesResponseDto {
    deletedUrls: string[]
    notFoundUrls: string[]
}

export async function editReview(
    reviewId: number,
    text: string,
    rating: number
): Promise<void> {
    await apiClient.put('/review', {
        reviewId,
        newText: text,
        newRating: rating,
    })
}

export async function uploadReviewFiles(
    reviewId: number,
    files: File[]
): Promise<UploadedReviewFileDto[]> {
    const formData = new FormData()
    files.forEach((file) => formData.append('files', file))

    const { data } = await apiClient.post<UploadedReviewFileDto[]>(
        `/review/${reviewId}/files`,
        formData,
        { headers: { 'Content-Type': 'multipart/form-data' } }
    )
    return data
}

export async function deleteReviewFiles(
    reviewId: number,
    urls: string[]
): Promise<DeleteReviewFilesResponseDto> {
    const { data } = await apiClient.delete<DeleteReviewFilesResponseDto>(
        `/review/${reviewId}/files`,
        { data: urls }
    )
    return data
}

/**
 * Отзывы текущего авторизованного пользователя.
 * Возвращаются отзывы во всех статусах (включая скрытые и отклонённые).
 */
export async function getUserReviews(): Promise<ReviewDto[]> {
    const { data } = await apiClient.get<ReviewDto[]>('/review')
    return data
}

/** Удаление собственного отзыва. */
export async function deleteReview(reviewId: number): Promise<void> {
    await apiClient.delete(`/review/${reviewId}`)
}