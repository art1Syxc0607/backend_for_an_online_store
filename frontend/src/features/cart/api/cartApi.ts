import { apiClient } from '@/shared/api/client'

export interface CartItemDto {
    id: number
    productId: number
    productName: string
    productImage?: string
    productPrice: number   // ← ✅ не "price", а "productPrice"
    quantity: number
}

export interface CartDto {
    id: number
    userId: number
    updatedAt: string
    totalPrice: number      // ← ✅ с backend
    totalItems: number      // ← ✅ с backend
    items: CartItemDto[]
}

// ✅ GET /api/cart
export async function getCart(): Promise<CartDto> {
    const { data } = await apiClient.get<CartDto>('/cart')
    return data
}

// ✅ POST /api/cart — добавление
export async function addToCart(productId: number, quantity: number): Promise<void> {
    await apiClient.post('/cart', { productId, quantity })
}

// ✅ PUT /api/cart/items — обновление количества
export async function updateQuantity(productId: number, quantity: number): Promise<void> {
    await apiClient.put('/cart/items', { productId, quantity })
}

// ✅ DELETE /api/cart/items/{productId} — удаление
export async function removeFromCart(productId: number): Promise<void> {
    await apiClient.delete(`/cart/items/${productId}`)
}

// ✅ DELETE /api/cart/clear
export async function clearCart(): Promise<void> {
    await apiClient.delete('/cart/clear')
}