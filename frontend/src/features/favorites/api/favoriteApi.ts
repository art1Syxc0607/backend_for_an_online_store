import { apiClient } from '@/shared/api/client'
import type { FavoriteDto, IsFavoritedDto } from '../types'

// POST /api/favorites/{productId}
export async function addToFavorites(productId: number): Promise<void> {
    await apiClient.post(`/favorites/${productId}`)
}

// DELETE /api/favorites/{productId}
export async function removeFromFavorites(productId: number): Promise<void> {
    await apiClient.delete(`/favorites/${productId}`)
}

// GET /api/favorites
export async function getFavorites(
    pageNumber = 1,
    pageSize = 20
): Promise<{ data: FavoriteDto[]; totalCount: number; pageNumber: number; pageSize: number }> {
    const { data } = await apiClient.get<{
        items: FavoriteDto[]
        totalCount: number
        pageNumber: number
        pageSize: number
        totalPages: number
        hasPreviousPage: boolean
        hasNextPage: boolean
    }>('/favorites', {
        params: { pageNumber, pageSize },
    })
    return {
        data: data.items,
        totalCount: data.totalCount,
        pageNumber: data.pageNumber,
        pageSize: data.pageSize,
    }
}

// GET /api/favorites/{productId}/check
export async function isProductFavorited(productId: number): Promise<boolean> {
    const { data } = await apiClient.get<IsFavoritedDto>(`/favorites/${productId}/check`)
    return data.isFavorited
}
