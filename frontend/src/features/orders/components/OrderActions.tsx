import { useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import { useOrderActions } from '../hooks/useOrderActions'
import ConfirmDialog from '@/shared/ui/ConfirmDialog'
import type { OrderStatus, OrderResponseDto } from '../api/orderApi'
import type { PagedResult } from '@/shared/api/types'

interface Props {
    orderId: number
    status: OrderStatus
    size?: 'sm' | 'md'
}

export default function OrderActions({ orderId, status, size = 'md' }: Props) {
    const queryClient = useQueryClient()
    const {
        payOrder, isPaying,
        receiveOrder, isReceiving,
        cancelOrder, isCancelling,
    } = useOrderActions()

    const [dialog, setDialog] = useState<'receive' | 'cancel' | null>(null)
    const [error, setError] = useState<string | null>(null)

    const sizeClasses = size === 'sm'
        ? 'px-3 py-1.5 text-xs'
        : 'px-4 py-2 text-sm'

    // ✅ Optimistic update — меняем статус СРАЗУ в кэше
    const optimisticUpdateStatus = (newStatus: OrderStatus) => {
        // ✅ Все списки заказов
        queryClient.setQueriesData<PagedResult<OrderResponseDto>>(
            { queryKey: ['order-history'] },
            (old) => {
                if (!old) return old
                return {
                    ...old,
                    items: old.items.map((o) =>
                        o.orderId === orderId ? { ...o, status: newStatus } : o
                    ),
                }
            }
        )

        // ✅ Конкретный заказ
        queryClient.setQueryData<OrderResponseDto>(
            ['order', orderId],
            (old) => (old ? { ...old, status: newStatus } : old)
        )
    }

    // ✅ Оплатить
    const handlePay = async (e: React.MouseEvent) => {
        e.stopPropagation()
        setError(null)
        try {
            const result = await payOrder({ orderId, method: 0 })
            if (result.paymentUrl) {
                window.location.href = result.paymentUrl
            }
        } catch (err: any) {
            setError(err?.response?.data?.error ?? 'Ошибка оплаты')
        }
    }

    // ✅ Получить
    const handleReceiveConfirm = async () => {
        setError(null)
        // ✅ Optimistic: сразу меняем статус
        optimisticUpdateStatus('Received')

        try {
            await receiveOrder(orderId)
            setDialog(null)
        } catch (err: any) {
            setError(err?.response?.data?.error ?? 'Ошибка')
            setDialog(null)
            // ✅ Откат при ошибке
            queryClient.invalidateQueries({ queryKey: ['order-history'] })
        }
    }

    // ✅ Отменить
    const handleCancelConfirm = async () => {
        setError(null)
        // ✅ Optimistic: сразу меняем статус
        optimisticUpdateStatus('Cancelled')

        try {
            await cancelOrder(orderId)
            setDialog(null)
        } catch (err: any) {
            setError(err?.response?.data?.error ?? 'Ошибка')
            setDialog(null)
            // ✅ Откат при ошибке
            queryClient.invalidateQueries({ queryKey: ['order-history'] })
        }
    }

    return (
        <>
            <div
                className="flex gap-2 flex-wrap"
                onClick={(e) => e.stopPropagation()}
            >
                {status === 'Pending' && (
                    <button
                        onClick={handlePay}
                        disabled={isPaying}
                        className={`${sizeClasses} bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50 font-medium transition`}
                    >
                        {isPaying ? '...' : '💳 Оплатить'}
                    </button>
                )}

                {status === 'Delivered' && (
                    <button
                        onClick={() => setDialog('receive')}
                        className={`${sizeClasses} bg-green-500 text-white rounded-lg hover:bg-green-600 font-medium transition`}
                    >
                        ✅ Получить
                    </button>
                )}

                {(status === 'Pending' || status === 'Paid') && (
                    <button
                        onClick={() => setDialog('cancel')}
                        className={`${sizeClasses} bg-red-500 text-white rounded-lg hover:bg-red-600 font-medium transition`}
                    >
                        ❌ Отменить
                    </button>
                )}
            </div>

            {error && (
                <div className="mt-2 text-xs text-red-600 bg-red-50 px-2 py-1 rounded">
                    {error}
                </div>
            )}

            <ConfirmDialog
                isOpen={dialog === 'receive'}
                title="Подтвердить получение?"
                message="Вы уверены, что получили этот заказ? Действие нельзя отменить."
                confirmLabel="Да, получил"
                cancelLabel="Нет"
                onConfirm={handleReceiveConfirm}
                onCancel={() => setDialog(null)}
                isLoading={isReceiving}
            />

            <ConfirmDialog
                isOpen={dialog === 'cancel'}
                title="Отменить заказ?"
                message="Вы уверены, что хотите отменить этот заказ? Действие нельзя отменить."
                confirmLabel="Да, отменить"
                cancelLabel="Нет"
                onConfirm={handleCancelConfirm}
                onCancel={() => setDialog(null)}
                isLoading={isCancelling}
            />
        </>
    )
}