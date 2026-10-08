interface Props {
    rating: number
    size?: 'sm' | 'md' | 'lg'
    showValue?: boolean
}

export default function StarRating({ rating, size = 'md', showValue = false }: Props) {
    const sizes = { sm: 'text-sm', md: 'text-base', lg: 'text-xl' }
    const fullStars = Math.floor(rating)
    const hasHalf = rating - fullStars >= 0.5

    return (
        <span className={`inline-flex items-center gap-0.5 ${sizes[size]}`}>
            {[1, 2, 3, 4, 5].map((i) => (
                <span
                    key={i}
                    className={
                        i <= fullStars
                            ? 'text-yellow-400'
                            : i === fullStars + 1 && hasHalf
                                ? 'text-yellow-400'
                                : 'text-gray-300'
                    }
                >
                    ★
                </span>
            ))}
            {showValue && (
                <span className="text-gray-600 text-xs ml-1">
                    {rating.toFixed(1)}
                </span>
            )}
        </span>
    )
}