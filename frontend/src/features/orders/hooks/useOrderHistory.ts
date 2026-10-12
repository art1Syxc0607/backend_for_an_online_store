import { useQuery } from '@tanstack/react-query'
import { getOrderHistory } from '../api/orderApi'
import type { OrderHistoryParams } from '../api/orderApi'

export function useOrderHistory(params: OrderHistoryParams = {}) {
    // ✅ Стабильный ключ — сериализация параметров
    const paramsKey = JSON.stringify(params)

    return useQuery({
        queryKey: ['order-history', paramsKey],
        queryFn: () => getOrderHistory(params),
        staleTime: 0,
    })
}