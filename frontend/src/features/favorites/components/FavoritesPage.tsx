import { useFavorites } from '../hooks/useFavorites'

export default function FavoritesPage() {
    const {
        favorites,
        totalCount,
        isLoading,
        error,
        removeFromFavorites,
        isRemoving,
    } = useFavorites()

    if (isLoading) {
        return (
            <div className="flex items-center justify-center min-h-[60vh]">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600" />
            </div>
        )
    }

    if (error) {
        return (
            <div className="flex items-center justify-center min-h-[60vh]">
                <p className="text-red-600 text-center">
                    Ошибка загрузки избранного: {(error as Error)?.message ?? 'Неизвестная ошибка'}
                </p>
            </div>
        )
    }

    if (favorites.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center min-h-[60vh] text-center px-4">
                <div className="w-16 h-16 bg-gray-100 rounded-full flex items-center justify-center mb-4">
                    <svg
                        fill="currentColor"
                        viewBox="0 0 24 24"
                        className="w-8 h-8 text-gray-400 mx-auto"
                    >
                        <path d="M12 21.35l-1.45-1.32C5.4 15.36 2 12.28 2 8.5 2 5.42 4.42 3 7.5 3c1.74 0 3.41.81 4.5 2.09C13.09 3.81 14.76 3 16.5 3 19.58 3 22 5.42 22 8.5c0 3.78-3.4 6.86-8.55 11.54L12 21.35z" />
                    </svg>
                </div>
                <h2 className="text-xl font-semibold text-gray-900">Избранное пусто</h2>
                <p className="text-gray-500 mt-2">
                    Добавьте товары в избранное, нажав на сердечко в карточке товара
                </p>
            </div>
        )
    }

    return (
        <div className="max-w-7xl mx-auto px-4 py-8">
            <div className="flex items-center justify-between mb-6">
                <h1 className="text-2xl font-bold text-gray-900">
                    Избранное ({totalCount})
                </h1>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
                {favorites.map((favorite) => (
                    <div
                        key={favorite.productId}
                        className="bg-white rounded-lg shadow hover:shadow-md transition-shadow overflow-hidden flex flex-col group"
                    >
                        <div className="aspect-square bg-gray-100 relative">
                            <img
                                src={favorite.productImageUrl ?? 'https://via.placeholder.com/300'}
                                alt={favorite.productName ?? 'Товар'}
                                className="w-full h-full object-cover"
                                onError={(e) => {
                                    e.currentTarget.src = 'https://via.placeholder.com/300'
                                }}
                            />
                            <button
                                onClick={(e) => {
                                    e.stopPropagation()
                                    void removeFromFavorites(favorite.productId)
                                }}
                                className="absolute top-2 right-2 bg-red-500/90 text-white rounded-full p-1.5 hover:bg-red-600 transition-colors"
                                aria-label="Удалить из избранного"
                                disabled={isRemoving}
                            >
                                <svg fill="currentColor" viewBox="0 0 24 24" className="w-5 h-5">
                                    <path d="M6 19c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7H6v12zM19 4h-3.5l-1-1h-5l-1 1H5v2h14V4z" />
                                </svg>
                            </button>
                        </div>

                        <div className="p-4 flex flex-col flex-1">
                            <h3 className="font-bold text-gray-900 truncate">
                                {favorite.productName ?? 'Товар'}
                            </h3>
                            {favorite.productPrice != null && (
                                <span className="text-blue-600 font-bold text-lg mt-1">
                                    {favorite.productPrice.toLocaleString('ru-RU')} ₽
                                </span>
                            )}
                        </div>
                    </div>
                ))}
            </div>
        </div>
    )
}
