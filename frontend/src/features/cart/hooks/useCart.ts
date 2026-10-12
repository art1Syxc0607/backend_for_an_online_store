import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
    getCart,
    addToCart,
    updateQuantity,
    removeFromCart,
    clearCart,
} from '../api/cartApi'
import { useAuth } from '@/features/auth/hooks/useAuth'

const CART_QUERY_KEY = ['cart']

export function useCart() {
    const queryClient = useQueryClient()
    const { isAuthenticated } = useAuth()

    const cartQuery = useQuery({
        queryKey: CART_QUERY_KEY,
        queryFn: getCart,
        enabled: isAuthenticated,
    })

    // ✅ Добавить — productId + quantity
    const addMutation = useMutation({
        mutationFn: ({ productId, quantity }: { productId: number; quantity: number }) =>
            addToCart(productId, quantity),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY })
        },
    })

    // ✅ Обновить — productId + quantity
    const updateMutation = useMutation({
        mutationFn: ({ productId, quantity }: { productId: number; quantity: number }) =>
            updateQuantity(productId, quantity),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY })
        },
    })

    // ✅ Удалить — productId
    const removeMutation = useMutation({
        mutationFn: (productId: number) => removeFromCart(productId),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY })
        },
    })

    const clearMutation = useMutation({
        mutationFn: clearCart,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY })
        },
    })

    return {
        cart: cartQuery.data,
        isLoading: cartQuery.isLoading,
        error: cartQuery.error,

        addToCart: addMutation.mutateAsync,
        isAdding: addMutation.isPending,

        updateQuantity: updateMutation.mutateAsync,
        isUpdating: updateMutation.isPending,

        removeFromCart: removeMutation.mutateAsync,
        isRemoving: removeMutation.isPending,

        clearCart: clearMutation.mutateAsync,
        isClearing: clearMutation.isPending,
    }
}