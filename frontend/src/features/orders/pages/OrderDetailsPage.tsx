import { useParams, Link } from 'react-router'
import { useQuery } from '@tanstack/react-query'
import { getOrderById, getOrderStatusLabel } from '../api/orderApi'
import OrderItemRow from '../components/OrderItemRow'
import OrderActions from '../components/OrderActions'

// ❌ УБРАТЬ неиспользуемый import OrderStatus
// import { OrderStatus } from '../api/orderApi'

export default function OrderDetailsPage() {
    const { id } = useParams<{ id: string }>()
    const orderId = Number(id)

    const { data: order, isLoading, error } = useQuery({
        queryKey: ['order', orderId],
        queryFn: () => getOrderById(orderId),
        enabled: !!orderId,
    })

    if (isLoading) return <div className="text-center py-20">Загрузка...</div>
    if (error || !order) {
        return (
            <div className="text-center py-20 text-red-500">
                Заказ не найден
            </div>
        )
    }

    const status = getOrderStatusLabel(order.status)

    return (
        <div className="max-w-4xl mx-auto">
            <Link to="/orders" className="text-blue-600 hover:underline mb-4 inline-block">
                ← Все заказы
            </Link>

            {/* ✅ Header + Actions в одном блоке */}
            <div className="bg-white rounded-lg shadow p-6 mb-6">
                <div className="flex items-center justify-between mb-4">
                    <h1 className="text-2xl font-bold">Заказ #{order.orderId}</h1>
                    <span className={`px-4 py-2 rounded-full font-medium ${status.color}`}>
                        {status.label}
                    </span>
                </div>

                {/* ✅ Действия */}
                <OrderActions orderId={order.orderId} status={order.status} />

                {/* Информация */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm mt-6">
                    <div>
                        <span className="text-gray-500">Дата создания:</span>{' '}
                        {new Date(order.createdAt).toLocaleDateString('ru-RU', {
                            day: 'numeric', month: 'long', year: 'numeric',
                            hour: '2-digit', minute: '2-digit',
                        })}
                    </div>
                    <div>
                        <span className="text-gray-500">Адрес доставки:</span>{' '}
                        {order.shippingAddress}
                    </div>
                    {order.paidAt && (
                        <div>
                            <span className="text-gray-500">Оплачен:</span>{' '}
                            {new Date(order.paidAt).toLocaleDateString('ru-RU')}
                        </div>
                    )}
                    {order.shippedAt && (
                        <div>
                            <span className="text-gray-500">Отправлен:</span>{' '}
                            {new Date(order.shippedAt).toLocaleDateString('ru-RU')}
                        </div>
                    )}
                    {order.deliveredAt && (
                        <div>
                            <span className="text-gray-500">Доставлен:</span>{' '}
                            {new Date(order.deliveredAt).toLocaleDateString('ru-RU')}
                        </div>
                    )}
                    {order.receivedAt && (
                        <div>
                            <span className="text-gray-500">Получен:</span>{' '}
                            {new Date(order.receivedAt).toLocaleDateString('ru-RU')}
                        </div>
                    )}
                    {order.cancelledAt && (
                        <div>
                            <span className="text-gray-500">Отменён:</span>{' '}
                            {new Date(order.cancelledAt).toLocaleDateString('ru-RU')}
                        </div>
                    )}
                </div>
            </div>

            {/* Товары */}
            <div className="bg-white rounded-lg shadow p-6">
                <h2 className="font-bold text-lg mb-4">
                    Товары в заказе ({order.items?.length ?? 0})
                </h2>

                <div className="space-y-4">
                    {order.items?.map((item) => (
                        <div key={item.id} className="border-b pb-4 last:border-0 last:pb-0">
                            <OrderItemRow item={item} size="md" />
                        </div>
                    ))}
                </div>

                <div className="border-t pt-4 mt-4 flex justify-between text-lg font-bold">
                    <span>Итого:</span>
                    <span className="text-blue-600">
                        {order.totalAmount.toLocaleString('ru-RU')} ₽
                    </span>
                </div>
            </div>
        </div>
    )
}