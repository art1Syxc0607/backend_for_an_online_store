import { Link } from 'react-router'
import type { ProductDto } from '@/shared/api/types'
import StarRating from '@/shared/ui/StarRating'

interface Props {
    product: ProductDto
}

export default function ProductCard({ product }: Props) {
    const mainImage = product.imageUrls?.[0] || 'https://via.placeholder.com/300'

    return (
        <Link
            to={`/products/${product.id}`}
            className="bg-white rounded-lg shadow hover:shadow-lg transition-shadow overflow-hidden block"
        >
            {/* Медиа */}
            <div className="aspect-square bg-gray-100 relative">
                <img
                    src={mainImage}
                    alt={product.name}
                    className="w-full h-full object-cover"
                    onError={(e) => {
                        e.currentTarget.src = 'https://via.placeholder.com/300'
                    }}
                />
                {product.imageUrls?.length > 1 && (
                    <span className="absolute top-2 right-2 bg-black/70 text-white text-xs px-2 py-1 rounded">
                        📷 {product.imageUrls.length}
                    </span>
                )}
                {product.videoUrls?.length > 0 && (
                    <span className="absolute top-2 left-2 bg-black/70 text-white text-xs px-2 py-1 rounded">
                        🎥 {product.videoUrls.length}
                    </span>
                )}
            </div>

            {/* Инфо */}
            <div className="p-4">
                <h3 className="font-bold text-gray-900 truncate">{product.name}</h3>
                <p className="text-gray-600 text-sm line-clamp-2 min-h-[40px]">
                    {product.description}
                </p>

                {/* Рейтинг */}
                <div className="flex items-center gap-2 mt-2">
                    <StarRating rating={product.averageRating || 0} size="sm" />
                    <span className="text-xs text-gray-500">
                        ({product.reviewCount || 0})
                    </span>
                </div>

                {/* Цена */}
                <div className="mt-2 flex items-center justify-between">
                    <span className="text-blue-600 font-bold text-lg">
                        {product.price?.toLocaleString('ru-RU')} ₽
                    </span>
                    <span className={`text-xs ${(product.availableQuantity ?? 0) > 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {(product.availableQuantity ?? 0) > 0
                            ? `В наличии: ${product.availableQuantity}`
                            : 'Нет'}
                    </span>
                </div>
            </div>
        </Link>
    )
}