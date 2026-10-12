import { Link } from 'react-router'
import type { OrderItemDto } from '../api/orderApi'

interface Props {
    item: OrderItemDto
    size?: 'sm' | 'md'   // sm — для превью, md — для детальной
}

export default function OrderItemRow({ item, size = 'md' }: Props) {
    const imageSrc = item.productImage || item.productImages?.[0]
    const imageSize = size === 'sm' ? 'w-12 h-12' : 'w-20 h-20'
    const placeholderSize = size === 'sm' ? 'text-xs' : 'text-sm'

    // ✅ Hover-эффект: сине-красный градиент + нижняя синяя линия
    const linkClasses = `
        relative inline-block font-medium transition-all duration-300
        bg-gradient-to-r from-blue-600 via-purple-500 to-red-500
        bg-clip-text text-transparent
        hover:from-red-500 hover:via-purple-500 hover:to-blue-600
        after:content-[''] after:absolute after:left-0 after:-bottom-0.5
        after:h-0.5 after:w-0 after:bg-blue-600
        after:transition-all after:duration-300
        hover:after:w-full
    `

    return (
        <div className={`flex gap-3 ${size === 'md' ? 'gap-4' : ''}`}>
            {/* ✅ Медиа */}
            <Link
                to={`/products/${item.productId}`}
                className={`${imageSize} bg-gray-200 rounded overflow-hidden flex-shrink-0 block`}
            >
                {imageSrc ? (
                    <img
                        src={imageSrc}
                        alt={item.productNameAtPurchase}
                        className="w-full h-full object-cover"
                        onError={(e) => {
                            e.currentTarget.style.display = 'none'
                        }}
                    />
                ) : (
                    <div
                        className={`w-full h-full flex items-center justify-center text-gray-400 ${placeholderSize}`}
                    >
                        📷
                    </div>
                )}
            </Link>

            {/* ✅ Название + цена */}
            <div className="flex-1 min-w-0">
                <Link
                    to={`/products/${item.productId}`}
                    className={linkClasses}
                    title={item.productNameAtPurchase}
                >
                    <span className="line-clamp-1">
                        {item.productNameAtPurchase || 'Товар'}
                    </span>
                </Link>

                {/* ✅ Цена и количество */}
                <div className="text-sm text-gray-600 mt-1">
                    {item.priceAtPurchase?.toLocaleString('ru-RU') ?? 0} ₽ × {item.quantity}
                </div>

                {/* ✅ SKU (опционально) */}
                {item.productSku && size === 'md' && (
                    <div className="text-xs text-gray-400 mt-0.5">
                        Артикул: {item.productSku}
                    </div>
                )}

                {/* ✅ Категория (опционально) */}
                {item.categoryName && size === 'md' && (
                    <div className="text-xs text-gray-400">
                        {item.categoryName}
                    </div>
                )}
            </div>

            {/* ✅ Итого по товару */}
            <div
                className={`text-right font-medium whitespace-nowrap ${size === 'md' ? 'text-base' : 'text-sm'
                    }`}
            >
                {(item.totalPrice ?? (item.priceAtPurchase ?? 0) * item.quantity).toLocaleString('ru-RU')} ₽
            </div>
        </div>
    )
}