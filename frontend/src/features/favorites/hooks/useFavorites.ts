import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
    addToFavorites,
    getFavorites,
    isProductFavorited,
    removeFromFavorites,
} from '../api/favoriteApi'

const FAVORITES_QUERY_KEY = ['favorites']

/** Ключ кэша для проверки "товар в избранном" по конкретному товару. */
const favoriteCheckKey = (productId: number) =>
    ['favorites', 'check', productId] as const

export function useFavorites(options: { enabled?: boolean } = {}) {
    const queryClient = useQueryClient()

    const favoritesQuery = useQuery({
        queryKey: FAVORITES_QUERY_KEY,
        queryFn: () => getFavorites(),
        enabled: options.enabled ?? true,
    })

    // Добавить в избранное (оптимистично)
    const addMutation = useMutation({
        mutationFn: addToFavorites,
        onMutate: async (productId: number) => {
            await queryClient.cancelQueries({
                queryKey: favoriteCheckKey(productId),
            })
            const previous = queryClient.getQueryData<boolean>(
                favoriteCheckKey(productId)
            )
            queryClient.setQueryData(favoriteCheckKey(productId), true)
            return { previous }
        },
        onError: (_error, productId, context) => {
            if (context?.previous !== undefined) {
                queryClient.setQueryData(
                    favoriteCheckKey(productId),
                    context.previous
                )
            }
        },
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: FAVORITES_QUERY_KEY })
        },
    })

    // Удалить из избранного (оптимистично)
    const removeMutation = useMutation({
        mutationFn: removeFromFavorites,
        onMutate: async (productId: number) => {
            await queryClient.cancelQueries({
                queryKey: favoriteCheckKey(productId),
            })
            const previous = queryClient.getQueryData<boolean>(
                favoriteCheckKey(productId)
            )
            queryClient.setQueryData(favoriteCheckKey(productId), false)
            return { previous }
        },
        onError: (_error, productId, context) => {
            if (context?.previous !== undefined) {
                queryClient.setQueryData(
                    favoriteCheckKey(productId),
                    context.previous
                )
            }
        },
        onSettled: () => {
            queryClient.invalidateQueries({ queryKey: FAVORITES_QUERY_KEY })
        },
    })

    return {
        favorites: favoritesQuery.data?.data ?? [],
        totalCount: favoritesQuery.data?.totalCount ?? 0,
        isLoading: favoritesQuery.isLoading,
        error: favoritesQuery.error,

        addToFavorites: addMutation.mutateAsync,
        isAdding: addMutation.isPending,

        removeFromFavorites: removeMutation.mutateAsync,
        isRemoving: removeMutation.isPending,
    }
}

/**
 * Хук для проверки, добавлен ли конкретный товар в избранное.
 * Используется в FavoriteButton для отображения состояния сердца.
 */
export function useIsFavorited(productId: number, enabled = true) {
    return useQuery({
        queryKey: favoriteCheckKey(productId),
        queryFn: () => isProductFavorited(productId),
        enabled: enabled && !!productId,
    })
}
