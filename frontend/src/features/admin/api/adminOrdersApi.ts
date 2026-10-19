import { apiClient } from '@/shared/api/client'
import type { OrderResponseDto, OrderStatus, SortOrderBy } from '@/features/orders/api/orderApi'

export interface GetAllOrdersParams {
    orderStatus?: OrderStatus
    lastDayOfThePeriod?: string
    firstDayOfThePeriod?: string
    userId?: number
    pageNumber?: number
    pageSize?: number
    orderBy?: SortOrderBy
    sortDesc?: boolean
}

export async function getAllOrders(
    params: GetAllOrdersParams = {}
): Promise<OrderResponseDto[]> {
    const { data } = await apiClient.get<OrderResponseDto[]>('/admin/order', {
        params,
    })
    return data
}

// ✅ Админский заказ по ID (без проверки владельца)
export async function getAdminOrderById(orderId: number): Promise<OrderResponseDto> {
    const { data } = await apiClient.get<OrderResponseDto>(`/admin/order/${orderId}`)
    return data
}

export async function shipOrder(orderId: number): Promise<void> {
    await apiClient.post(`/admin/order/ship/${orderId}`)
}

export async function deliverOrder(orderId: number): Promise<void> {
    await apiClient.post(`/admin/order/deliver/${orderId}`)
}