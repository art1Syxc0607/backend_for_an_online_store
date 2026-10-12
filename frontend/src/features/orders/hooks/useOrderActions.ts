import { useMutation, useQueryClient } from '@tanstack/react-query'
import { initiatePayment, receiveOrder, cancelOrder } from '../api/orderApi'

export function useOrderActions() {
    const queryClient = useQueryClient()

    // ✅ Общая функция инвалидации
    const invalidateAll = (orderId: number) => {
        // ✅ Все списки заказов
        queryClient.invalidateQueries({
            queryKey: ['order-history'],
            exact: false,   // ← ВАЖНО: все ключи, начинающиеся с 'order-history'
        })

        // ✅ Конкретный заказ
        queryClient.invalidateQueries({
            queryKey: ['order', orderId],
            exact: false,
        })
    }

    // ✅ Оплатить
    const payMutation = useMutation({
        mutationFn: ({ orderId, method }: { orderId: number; method?: number }) =>
            initiatePayment(orderId, method),
        onSuccess: (_, variables) => {
            invalidateAll(variables.orderId)
        },
    })

    // ✅ Получить
    const receiveMutation = useMutation({
        mutationFn: (orderId: number) => receiveOrder(orderId),
        onSuccess: (_, orderId) => {
            invalidateAll(orderId)
        },
    })

    // ✅ Отменить
    const cancelMutation = useMutation({
        mutationFn: (orderId: number) => cancelOrder(orderId),
        onSuccess: (_, orderId) => {
            invalidateAll(orderId)
        },
    })

    return {
        payOrder: payMutation.mutateAsync,
        isPaying: payMutation.isPending,

        receiveOrder: receiveMutation.mutateAsync,
        isReceiving: receiveMutation.isPending,

        cancelOrder: cancelMutation.mutateAsync,
        isCancelling: cancelMutation.isPending,
    }
}