import { apiClient } from '@/shared/api/client'
import type { PagedResult } from '@/shared/api/types'

// ═══════════════════════════════════════════
// ✅ OrderStatus — СТРОКИ
// ═══════════════════════════════════════════
export type OrderStatus =
    | 'Pending'
    | 'Paid'
    | 'Shipped'
    | 'Delivered'
    | 'Received'
    | 'Cancelled'

// ═══════════════════════════════════════════
// ✅ SortOrderBy — ЧИСЛА (точно как в backend)
// ═══════════════════════════════════════════
export enum SortOrderBy {
    Status = 0,
    DateOfCreation = 1,
    UserId = 2,
    Price = 3,
}

// ═══════════════════════════════════════════
// DTO
// ═══════════════════════════════════════════

export interface OrderItemDto {
    id: number
    productId: number
    productNameAtPurchase: string
    priceAtPurchase: number
    purchasePriceAtPurchase: number
    quantity: number
    productImage?: string
    productImages: string[]
    productDescription?: string
    productSku?: string
    categoryName?: string
    totalPrice: number
}

export interface OrderResponseDto {
    orderId: number
    userId: number
    totalAmount: number
    shippingAddress: string
    status: OrderStatus
    createdAt: string
    paidAt?: string
    shippedAt?: string
    deliveredAt?: string
    receivedAt?: string
    cancelledAt?: string
    items: OrderItemDto[]
}

export interface OrderHistoryParams {
    orderStatus?: OrderStatus
    lastDayOfThePeriod?: string
    firstDayOfThePeriod?: string
    pageNumber?: number
    pageSize?: number
    orderBy?: SortOrderBy
    sortDesc?: boolean
}

// ═══════════════════════════════════════════
// API — Заказы
// ═══════════════════════════════════════════

export async function getOrderHistory(
    params: OrderHistoryParams = {}
): Promise<PagedResult<OrderResponseDto>> {
    const { data } = await apiClient.get<PagedResult<OrderResponseDto>>(
        '/order/history',
        { params }
    )
    return data
}

export async function getOrderById(id: number): Promise<OrderResponseDto> {
    const { data } = await apiClient.get<OrderResponseDto>(`/order/${id}`)
    return data
}

export async function createOrder(request: {
    items: { productId: number; quantity: number }[]
    shippingAddress: string
}): Promise<number> {
    const { data } = await apiClient.post<number>('/order', request)
    return data
}

// ═══════════════════════════════════════════
// ✅ API — Действия с заказом
// ═══════════════════════════════════════════

export interface PaymentResult {
    success: boolean
    paymentIntentId?: string
    paymentUrl?: string
    errorMessage?: string
}

export async function initiatePayment(
    orderId: number,
    method: number = 0
): Promise<PaymentResult> {
    const { data } = await apiClient.post<PaymentResult>('/order/initiate', {
        orderId,
        method,
    })
    return data
}

export async function receiveOrder(orderId: number): Promise<void> {
    await apiClient.post(`/order/receive/${orderId}`)
}

export async function cancelOrder(orderId: number): Promise<void> {
    await apiClient.put('/order/Cancel', { orderId })
}

// ═══════════════════════════════════════════
// ✅ Хелпер статусов
// ═══════════════════════════════════════════

export function getOrderStatusLabel(status: OrderStatus | string | number): {
    label: string
    color: string
} {
    const map: Record<string, { label: string; color: string }> = {
        // ✅ Строки (основной случай)
        'Pending': { label: '⏳ Ожидает оплаты', color: 'bg-yellow-100 text-yellow-700' },
        'Paid': { label: '💳 Оплачен', color: 'bg-blue-100 text-blue-700' },
        'Shipped': { label: '📦 Отправлен', color: 'bg-indigo-100 text-indigo-700' },
        'Delivered': { label: '🚚 Доставлен', color: 'bg-purple-100 text-purple-700' },
        'Received': { label: '✅ Получен', color: 'bg-green-100 text-green-700' },
        'Cancelled': { label: '❌ Отменён', color: 'bg-red-100 text-red-700' },

        // ✅ Числа (fallback)
        '0': { label: '⏳ Ожидает оплаты', color: 'bg-yellow-100 text-yellow-700' },
        '1': { label: '💳 Оплачен', color: 'bg-blue-100 text-blue-700' },
        '2': { label: '📦 Отправлен', color: 'bg-indigo-100 text-indigo-700' },
        '3': { label: '🚚 Доставлен', color: 'bg-purple-100 text-purple-700' },
        '4': { label: '✅ Получен', color: 'bg-green-100 text-green-700' },
        '5': { label: '❌ Отменён', color: 'bg-red-100 text-red-700' },
    }

    const key = String(status)
    return map[key] ?? {
        label: `Неизвестный статус (${key})`,
        color: 'bg-gray-100 text-gray-700',
    }
}