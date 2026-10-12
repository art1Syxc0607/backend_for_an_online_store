import { useState } from 'react'
import { useCurrentOrder } from '../context/CurrentOrderContext'
import { useAuth } from '@/features/auth/hooks/useAuth'
import { useNavigate } from 'react-router'
import QuantityDialog from '@/shared/ui/QuantityDialog'

interface Props {
    productId: number
    productName: string
    productImage?: string
    productPrice: number
    maxQuantity: number
    className?: string
    size?: 'sm' | 'lg'
}

export default function AddToCurrentOrderButton({
    productId,
    productName,
    productImage,
    productPrice,
    maxQuantity,
    className = '',
    size = 'lg',
}: Props) {
    const { isAuthenticated } = useAuth()
    const { addItem, openDrawer } = useCurrentOrder()
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

    const handleConfirm = (quantity: number) => {
        addItem({
            productId,
            productName,
            productImage,
            price: productPrice,
            quantity,
        })
        setShowDialog(false)
        openDrawer()  // ✅ Открываем drawer после добавления
    }

    const sizeClasses = size === 'sm'
        ? 'px-3 py-1.5 text-sm'
        : 'px-6 py-3 text-base'

    return (
        <>
            <button
                onClick={handleClick}
                disabled={maxQuantity <= 0}
                className={`${sizeClasses} bg-green-500 text-white rounded-lg hover:bg-green-600 disabled:opacity-50 disabled:cursor-not-allowed transition-colors ${className}`}
            >
                {maxQuantity > 0 ? '📦 В заказ' : 'Нет в наличии'}
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