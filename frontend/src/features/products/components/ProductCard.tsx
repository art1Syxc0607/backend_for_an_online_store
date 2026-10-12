import { Link } from 'react-router'
import type { ProductDto, PopularProductDto } from '@/shared/api/types'
import StarRating from '@/shared/ui/StarRating'
import AddToCartButton from '@/features/cart/components/AddToCartButton'
import AddToCurrentOrderButton from '@/features/current-order/components/AddToCurrentOrderButton'

type ProductLike = ProductDto | PopularProductDto

interface Props {
    product: ProductLike
}

export default function ProductCard({ product }: Props) {
    const productId = 'id' in product ? product.id : product.productId
    const mainImage = product.imageUrls?.[0] || 'https://via.placeholder.com/300'

    const averageRating = 'averageRating' in product ? (product.averageRating ?? 0) : 0
    const reviewCount = 'reviewCount' in product ? (product.reviewCount ?? 0) : 0
    const availableQuantity =
        'availableQuantity' in product
            ? product.availableQuantity
            : product.stockQuantity - product.reservedQuantity

    return (
        <div className="bg-white rounded-lg shadow hover:shadow-lg transition-shadow overflow-hidden flex flex-col">
            <Link to={`/products/${productId}`} className="block">
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
            </Link>

            <div className="p-4 flex flex-col flex-1">
                <Link to={`/products/${productId}`}>
                    <h3 className="font-bold text-gray-900 truncate hover:text-blue-600">
                        {product.name}
                    </h3>
                </Link>
                <p className="text-gray-600 text-sm line-clamp-2 min-h-[40px]">
                    {product.description}
                </p>

                <div className="flex items-center gap-2 mt-2">
                    <StarRating rating={averageRating} size="sm" />
                    <span className="text-xs text-gray-500">
                        {reviewCount > 0 ? `(${reviewCount})` : 'Нет отзывов'}
                    </span>
                </div>

                <div className="mt-2 flex items-center justify-between">
                    <span className="text-blue-600 font-bold text-lg">
                        {product.price?.toLocaleString('ru-RU')} ₽
                    </span>
                    <span className={`text-xs ${availableQuantity > 0 ? 'text-green-600' : 'text-red-600'}`}>
                        {availableQuantity > 0 ? `${availableQuantity} шт.` : 'Нет'}
                    </span>
                </div>

                {/* ✅ Кнопка добавления в корзину */}
                <div className="mt-3">
                    <AddToCartButton
                        productId={productId}
                        productName={product.name}
                        maxQuantity={availableQuantity}
                        size="sm"
                        className="w-full"
                    />
                </div>

                {/* ✅ Кнопка добавления в текущий заказ */}
                <div className="mt-2">
                    <AddToCurrentOrderButton
                        productId={productId}
                        productName={product.name}
                        productImage={mainImage}
                        productPrice={product.price}
                        maxQuantity={availableQuantity}
                        size="sm"
                        className="w-full"
                    />
                </div>
            </div>
        </div>
    )
}