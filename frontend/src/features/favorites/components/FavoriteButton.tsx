import { useAuth } from '@/features/auth/hooks/useAuth'
import { useIsFavorited, useFavorites } from '../hooks/useFavorites'

interface Props {
    productId: number
    /** Размер кнопки: 'sm' (карточка товара) или 'lg' (страница товара) */
    size?: 'sm' | 'lg'
    className?: string
}

/**
 * Размеры повторяют AddToCartButton / AddToCurrentOrderButton,
 * чтобы кнопка была ровно такой же высоты и ширины.
 */
const SIZE_CLASSES: Record<'sm' | 'lg', string> = {
    sm: 'px-3 py-1.5 text-sm',
    lg: 'px-6 py-3 text-base',
}

const ICON_CLASSES: Record<'sm' | 'lg', string> = {
    sm: 'w-4 h-4',
    lg: 'w-5 h-5',
}

export default function FavoriteButton({
    productId,
    size = 'sm',
    className = '',
}: Props) {
    const { isAuthenticated } = useAuth()
    const { addToFavorites, removeFromFavorites, isAdding, isRemoving } =
        useFavorites({ enabled: isAuthenticated })
    const { data: isFavorited = false } = useIsFavorited(
        productId,
        isAuthenticated
    )

    const sizeClass = SIZE_CLASSES[size]
    const iconClass = ICON_CLASSES[size]

    // Неавторизованный пользователь: кнопка неактивна
    if (!isAuthenticated) {
        return (
            <button
                type="button"
                disabled
                title="Войдите, чтобы добавить в избранное"
                className={`${sizeClass} flex items-center justify-center gap-1.5 whitespace-nowrap rounded-lg bg-gray-100 text-gray-400 cursor-not-allowed ${className}`}
            >
                <HeartIcon filled={false} className={iconClass} />
                Добавить в избранное
            </button>
        )
    }

    const isLoading = isAdding || isRemoving

    const handleToggle = async () => {
        try {
            if (isFavorited) {
                await removeFromFavorites(productId)
            } else {
                await addToFavorites(productId)
            }
        } catch {
            // Ошибка уже обработана глобально (409 Conflict и т.д.),
            // оптимистичное состояние откатается в onError хука.
        }
    }

    return (
        <button
            type="button"
            onClick={handleToggle}
            disabled={isLoading}
            aria-pressed={isFavorited}
            aria-label={isFavorited ? 'Удалить из избранного' : 'Добавить в избранное'}
            className={`${sizeClass} flex items-center justify-center gap-1.5 whitespace-nowrap rounded-lg transition-colors disabled:opacity-50 disabled:cursor-not-allowed ${
                isFavorited
                    ? 'bg-red-50 text-red-600 ring-1 ring-inset ring-red-200 hover:bg-red-100'
                    : 'bg-white text-gray-700 ring-1 ring-inset ring-gray-300 hover:bg-red-50 hover:text-red-600'
            } ${className}`}
        >
            <HeartIcon filled={isFavorited} className={iconClass} />
            {isFavorited ? 'В избранном' : 'Добавить в избранное'}
        </button>
    )
}

function HeartIcon({
    filled,
    className,
}: {
    filled: boolean
    className: string
}) {
    return (
        <svg
            viewBox="0 0 24 24"
            fill={filled ? 'currentColor' : 'none'}
            stroke="currentColor"
            strokeWidth={1.8}
            strokeLinecap="round"
            strokeLinejoin="round"
            className={`${className} shrink-0`}
            aria-hidden="true"
        >
            <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z" />
        </svg>
    )
}
