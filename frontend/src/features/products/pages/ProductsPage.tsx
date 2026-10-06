import { useQuery } from '@tanstack/react-query'
import { getProducts } from '../api/productApi'

export default function ProductsPage() {
    const { data, isLoading, error } = useQuery({
        queryKey: ['products'],
        queryFn: () => getProducts(1, 20),
    })

    if (isLoading) {
        return <div className="text-center py-20">Загрузка товаров...</div>
    }

    if (error) {
        return <div className="text-center py-20 text-red-500">Ошибка загрузки</div>
    }

    return (
        <div>
            <h1 className="text-3xl font-bold mb-6">Товары</h1>
            <div className="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-4">
                {data?.items.map((product) => (
                    <div key={product.id} className="bg-white rounded-lg shadow p-4">
                        <h3 className="font-bold">{product.name}</h3>
                        <p className="text-gray-600 text-sm">{product.description}</p>
                        <p className="text-blue-600 font-bold mt-2">{product.price} ₽</p>
                        <p className="text-xs text-gray-500">В наличии: {product.availableQuantity}</p>
                    </div>
                ))}
            </div>
        </div>
    )
}