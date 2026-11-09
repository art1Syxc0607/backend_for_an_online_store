import type { ReactNode } from 'react'
import StarRating from '@/shared/ui/StarRating'
import type { ReviewDto } from '../api/reviewApi'

interface Props {
    review: ReviewDto
    id?: string
    highlighted?: boolean
    onCardClick?: () => void
    actions?: ReactNode
    showProductName?: boolean
    compact?: boolean
    className?: string
}

const statusLabels: Record<string, string> = {
    Pending: 'На модерации',
    Approved: 'Опубликован',
    Hidden: 'Скрыт',
    Rejected: 'Отклонён',
    // на случай, если enum придёт числом
    '0': 'На модерации',
    '1': 'Опубликован',
    '2': 'Скрыт',
    '3': 'Отклонён',
}

export default function ReviewCard({
    review,
    id,
    highlighted = false,
    onCardClick,
    actions,
    showProductName = false,
    compact = false,
    className = '',
}: Props) {
    const images = review.imagesUrls ?? []
    const videos = review.videosUrls ?? []
    const statusLabel = showProductName ? statusLabels[String(review.status)] : undefined

    return (
        <div
            id={id}
            onClick={onCardClick}
            className={[
                'bg-white rounded-lg shadow p-4 transition-shadow',
                highlighted ? 'ring-2 ring-amber-400 bg-amber-50' : '',
                onCardClick ? 'cursor-pointer hover:shadow-md' : '',
                className,
            ]
                .filter(Boolean)
                .join(' ')}
        >
            <div className="flex items-start justify-between gap-3 mb-2">
                <div className="min-w-0">
                    <div className="flex items-center gap-2 flex-wrap">
                        <span className="font-bold truncate">
                            {showProductName
                                ? review.productName || `Товар #${review.productId}`
                                : review.userName || 'Аноним'}
                        </span>

                        {review.isVerifiedPurchase && (
                            <span className="text-xs bg-green-100 text-green-700 px-2 py-0.5 rounded">
                                ✅ Купил
                            </span>
                        )}

                        {statusLabel && (
                            <span className="text-xs bg-gray-100 text-gray-600 px-2 py-0.5 rounded">
                                {statusLabel}
                            </span>
                        )}
                    </div>

                    {highlighted && (
                        <div className="text-xs text-amber-700 mt-0.5">
                            Отзыв, который вы выбрали
                        </div>
                    )}
                </div>

                <div className="flex items-center gap-2 shrink-0">
                    {actions}
                    <span className="text-xs text-gray-500 whitespace-nowrap">
                        {new Date(review.createdAt).toLocaleDateString('ru-RU')}
                    </span>
                </div>
            </div>

            <StarRating rating={review.rating || 0} size="sm" />

            <p
                className={`mt-2 text-gray-700 whitespace-pre-wrap ${compact ? 'line-clamp-3' : ''
                    }`}
            >
                {review.text}
            </p>

            {images.length > 0 && (
                <div className="flex gap-2 mt-3 flex-wrap">
                    {images.map((url, i) => (
                        <img
                            key={i}
                            src={url}
                            alt={`Фото ${i + 1}`}
                            className={`object-cover rounded cursor-pointer hover:opacity-80 border ${compact ? 'w-16 h-16' : 'w-24 h-24'
                                }`}
                            onClick={(e) => {
                                e.stopPropagation()
                                window.open(url, '_blank')
                            }}
                            onError={(e) => {
                                e.currentTarget.style.display = 'none'
                            }}
                        />
                    ))}
                </div>
            )}

            {videos.length > 0 && (
                <div className="flex gap-2 mt-3 flex-wrap">
                    {videos.map((url, i) => (
                        <video
                            key={i}
                            src={url}
                            controls
                            onClick={(e) => e.stopPropagation()}
                            className={`object-cover rounded border ${compact ? 'w-28 h-16' : 'w-40 h-24'
                                }`}
                        />
                    ))}
                </div>
            )}

            {review.adminResponse && (
                <div className="mt-3 ml-4 p-3 bg-blue-50 rounded border-l-4 border-blue-500">
                    <div className="text-xs font-bold text-blue-700 mb-1">
                        💬 Ответ магазина
                    </div>
                    <p className="text-sm text-gray-700">{review.adminResponse}</p>
                    {review.adminResponseAt && (
                        <div className="text-xs text-gray-500 mt-1">
                            {new Date(review.adminResponseAt).toLocaleDateString('ru-RU')}
                        </div>
                    )}
                </div>
            )}
        </div>
    )
}
