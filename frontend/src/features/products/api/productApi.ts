import { apiClient } from '@/shared/api/client'

export interface ProductDto {
    id: number
    name: string
    description?: string
    price: number
    availableQuantity: number
    imageUrls: string[]
}


export interface PagedResult<T> {
    items: T[]
    totalCount: number
    pageNumber: number
    pageSize: number
    totalPages: number
    hasPreviousPage: boolean
    hasNextPage: boolean
}

export async function getProducts(pageNumber = 1, pageSize = 20) {
    // ❌ Было: '/products' → 404
    // ✅ Стало: '/product'
    const { data } = await apiClient.get('/product', {
        params: { pageNumber, pageSize },
    })
    return data
}