export interface FavoriteDto {
    userId: number
    productId: number
    createdAt: string
    productName?: string
    productPrice?: number
    productImageUrl?: string
}

export interface IsFavoritedDto {
    isFavorited: boolean
}
