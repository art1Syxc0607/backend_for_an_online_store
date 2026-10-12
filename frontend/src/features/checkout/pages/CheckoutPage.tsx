import { useState } from 'react'
import { useNavigate } from 'react-router'
import { useQueryClient } from '@tanstack/react-query'
import { useCart } from '@/features/cart/hooks/useCart'
import { checkout } from '../api/checkoutApi'

export default function CheckoutPage() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const { cart } = useCart()
    const [address, setAddress] = useState('')
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [error, setError] = useState<string | null>(null)

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        setError(null)
        setIsSubmitting(true)

        try {
            const orderId = await checkout(address)
            queryClient.invalidateQueries({ queryKey: ['cart'] })
            navigate(`/orders/${orderId}`)
        } catch (err: any) {
            setError(err?.response?.data?.error ?? 'Ошибка оформления заказа')
        } finally {
            setIsSubmitting(false)
        }
    }

    if (!cart || cart.items.length === 0) {
        return (
            <div className="text-center py-20">
                <p className="text-gray-500">Корзина пуста</p>
            </div>
        )
    }

    return (
        <div className="max-w-2xl mx-auto">
            <h1 className="text-3xl font-bold mb-6">Оформление заказа</h1>

            {error && (
                <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
                    {error}
                </div>
            )}

            {/* Товары */}
            <div className="bg-white rounded-lg shadow p-6 mb-6">
                <h2 className="font-bold mb-4">Товары ({cart.totalItems})</h2>
                <div className="space-y-2">
                    {cart.items.map((item) => (
                        <div key={item.id} className="flex justify-between text-sm">
                            <span>{item.productName} × {item.quantity}</span>
                            <span className="font-medium">
                                {((item.productPrice ?? 0) * item.quantity).toLocaleString('ru-RU')} ₽
                            </span>
                        </div>
                    ))}
                    <div className="border-t pt-2 mt-2 flex justify-between font-bold text-lg">
                        <span>Итого:</span>
                        <span className="text-blue-600">
                            {cart.totalPrice.toLocaleString('ru-RU')} ₽
                        </span>
                    </div>
                </div>
            </div>

            {/* Форма адреса */}
            <form onSubmit={handleSubmit} className="bg-white rounded-lg shadow p-6">
                <h2 className="font-bold mb-4">Адрес доставки</h2>
                <textarea
                    value={address}
                    onChange={(e) => setAddress(e.target.value)}
                    placeholder="Введите полный адрес доставки"
                    rows={3}
                    required
                    className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
                />

                <button
                    type="submit"
                    disabled={isSubmitting || !address.trim()}
                    className="w-full mt-4 px-6 py-3 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50"
                >
                    {isSubmitting ? 'Оформление...' : 'Подтвердить заказ'}
                </button>
            </form>
        </div>
    )
}