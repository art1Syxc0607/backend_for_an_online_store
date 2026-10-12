import { apiClient } from '@/shared/api/client'
import type { PagedResult } from '@/shared/api/types'

export interface NumberOfNewOrdersDto {
    numberOfPending: number
    numberOfPaid: number
    numberOfShipped: number
    numberOfDelivered: number
    numberOfReceived: number
    numberOfCancelled: number
    totalOrders: number
}

export interface RevenueForPeriodDto {
    revenue: number
    cost: number
    income: number
}

export interface PopularProductDto {
    productId: number
    name: string
    price: number
    imageUrls: string[]
    totalPurchases: number
    presenceInOrders: number
    averageRating: number
    reviewCount: number
    amountOfPaid: number
}

export async function getNumberOfNewOrders(
    firstDay: string,
    lastDay: string
): Promise<NumberOfNewOrdersDto> {
    const { data } = await apiClient.get<NumberOfNewOrdersDto>(
        '/admin/dashboard/numberOfNewOrdersForThePeriod',
        { params: { firstDayOfThePriod: firstDay, lastDayOfThePriod: lastDay } }
    )
    return data
}

export async function getRevenue(
    firstDay: string,
    lastDay: string
): Promise<RevenueForPeriodDto> {
    const { data } = await apiClient.get<RevenueForPeriodDto>(
        '/admin/dashboard/revenue',
        { params: { firstDayOfThePriod: firstDay, lastDayOfThePriod: lastDay } }
    )
    return data
}

export async function getPopularProducts(
    firstDay: string,
    lastDay: string,
    pageNumber = 1,
    pageSize = 20
): Promise<PagedResult<PopularProductDto>> {
    const { data } = await apiClient.get<PagedResult<PopularProductDto>>(
        '/admin/dashboard/popularProducts',
        { params: { firstDayOfThePriod: firstDay, lastDayOfThePriod: lastDay, pageNumber, pageSize } }
    )
    return data
}