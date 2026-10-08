// ✅ Товар с статистикой
export interface ProductDto {
    id: number
    name: string
    description?: string
    price: number
    availableQuantity: number
    sku?: string
    categoryId?: number
    categoryName?: string
    imageUrls: string[]
    videoUrls: string[]
    averageRating: number
    reviewCount: number
    totalPurchases: number
    createdAt: string
    updatedAt?: string
}

// ✅ Популярный товар (с статистикой за период)
export interface PopularProductDto {
    productId: number
    name: string
    description?: string
    price: number
    stockQuantity: number
    reservedQuantity: number
    imageUrls: string[]
    videoUrls: string[]
    categoryId?: number
    createdAt: string
    updatedAt?: string

    // ✅ Статистика за период
    totalPurchases: number
    presenceInOrders: number
    amountOfPendingForThePeriod: number
    amountOfPaidForThePeriod: number
    amountOfShippedForThePeriod: number
    amountOfDeliveredForThePeriod: number
    amountOfReceivedForThePeriod: number
    amountOfCancelledForThePeriod: number
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