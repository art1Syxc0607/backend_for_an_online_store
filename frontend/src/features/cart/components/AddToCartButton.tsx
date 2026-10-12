import { useState } from 'react'
import { useCart } from '../hooks/useCart'
import { useCartDrawer } from '../context/CartContext'
import { useAuth } from '@/features/auth/hooks/useAuth'
import QuantityDialog from '@/shared/ui/QuantityDialog'
import { useNavigate } from 'react-router'

interface Props {
    productId: number
    productName: string
    maxQuantity: number
    className?: string
    size?: 'sm' | 'lg'
}

export default function AddToCartButton({
    productId,
    productName,
    maxQuantity,
    className = '',
    size = 'lg',
}: Props) {
    const { isAuthenticated } = useAuth()
    const { addToCart, isAdding } = useCart()
    const { openCart } = useCartDrawer()
    const navigate = useNavigate()
    const [showDialog, setShowDialog] = useState(false)

    const handleClick = (e: React.MouseEvent) => {
        e.preventDefault()
        e.stopPropagation()

        if (!isAuthenticated) {
            navigate('/login')
            return
        }

        if (maxQuantity <= 0) return

        setShowDialog(true)
    }

    // ✅ Обновлённый handleConfirm
    const handleConfirm = async (quantity: number) => {
        try {
            await addToCart({ productId, quantity })
            setShowDialog(false)
            openCart()
        } catch (error) {
            console.error('Failed to add to cart:', error)
        }
    }

    const sizeClasses = size === 'sm'
        ? 'px-3 py-1.5 text-sm'
        : 'px-6 py-3 text-base'

    return (
        <>
            <button
                onClick={handleClick}
                disabled={maxQuantity <= 0 || isAdding}
                className={`${sizeClasses} bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors ${className}`}
            >
                {isAdding ? '...' : maxQuantity > 0 ? '🛒 В корзину' : 'Нет в наличии'}
            </button>

            <QuantityDialog
                isOpen={showDialog}
                productName={productName}
                maxQuantity={maxQuantity}
                onConfirm={handleConfirm}
                onCancel={() => setShowDialog(false)}
            />
        </>
    )
}