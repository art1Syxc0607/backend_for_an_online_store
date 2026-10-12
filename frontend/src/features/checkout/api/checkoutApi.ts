import { apiClient } from '@/shared/api/client'

export interface CheckoutRequest {
    shippingAddress: string
}

// ✅ POST /api/cart/Checkout
export async function checkout(shippingAddress: string): Promise<number> {
    const { data } = await apiClient.post<number>('/cart/Checkout', {
        shippingAddress,
    })
    return data
}