import { apiClient } from '@/shared/api/client'
import type { ProductDto, PagedResult, PopularProductDto } from '@/shared/api/types'

// ✅ По умолчанию — популярные
export async function getPopularProducts(
    pageNumber = 1,
    pageSize = 20
): Promise<PagedResult<PopularProductDto>> {
    const { data } = await apiClient.get<PagedResult<PopularProductDto>>(
        '/products/popularProducts',
        { params: { pageNumber, pageSize } }
    )
    return data
}

// ✅ С фильтрами
export interface ProductFilterRequest {
    searchText?: string
    categoryId?: number
    priceLimitMin?: number
    priceLimitMax?: number
    onlyAvailable?: boolean
    pageNumber?: number
    pageSize?: number
    sortBy?: string
    sortDesc?: boolean
}

export async function getFilteredProducts(
    filter: ProductFilterRequest
): Promise<PagedResult<ProductDto>> {
    const { data } = await apiClient.get<PagedResult<ProductDto>>(
        '/products/filter',
        { params: filter }
    )
    return data
}

export async function getProductById(id: number): Promise<ProductDto> {
    const { data } = await apiClient.get<ProductDto>(`/products/${id}`)
    return data
}