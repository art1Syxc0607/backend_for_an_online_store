import { useEffect, useRef, useState } from 'react'
import { useInfiniteQuery } from '@tanstack/react-query'
import { getPopularProducts, getFilteredProducts } from '../api/productApi'
import type { ProductFilterRequest } from '../api/productApi'
import ProductCard from '../components/ProductCard'
import ProductFilters from '../components/ProductFilters'
import type { ProductDto } from '@/shared/api/types'

export default function ProductsPage() {
    const [showFilters, setShowFilters] = useState(false)
    const [filters, setFilters] = useState<ProductFilterRequest | null>(null)
    const loadMoreRef = useRef<HTMLDivElement>(null)

    const isFiltered = filters !== null

    const {
        data,
        isLoading,
        error,
        fetchNextPage,
        hasNextPage,
        isFetchingNextPage,
    } = useInfiniteQuery({
        queryKey: ['products', isFiltered ? filters : 'popular'],
        queryFn: ({ pageParam = 1 }) => {
            if (isFiltered) {
                return getFilteredProducts({
                    ...filters,
                    pageNumber: pageParam,
                    pageSize: 20,
                })
            }
            return getPopularProducts(pageParam, 20)
        },
        getNextPageParam: (lastPage) =>
            lastPage.hasNextPage ? lastPage.pageNumber + 1 : undefined,
        initialPageParam: 1,
    })

    useEffect(() => {
        const observer = new IntersectionObserver(
            (entries) => {
                if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
                    fetchNextPage()
                }
            },
            { threshold: 0.5 }
        )

        if (loadMoreRef.current) observer.observe(loadMoreRef.current)
        return () => observer.disconnect()
    }, [hasNextPage, isFetchingNextPage, fetchNextPage])

    if (isLoading) return <div className="text-center py-20">Загрузка товаров...</div>
    if (error) return <div className="text-center py-20 text-red-500">Ошибка загрузки</div>

    const allItems = data?.pages.flatMap((page) => page.items) ?? []
    const totalCount = data?.pages[0]?.totalCount ?? 0

    // ✅ Нормализация: productId → id
    const normalizedProducts: ProductDto[] = allItems.map((item: any) => ({
        ...item,
        id: item.id ?? item.productId,  // ✅ fallback
    }))

    return (
        <div>
            <div className="flex items-center justify-between mb-6">
                <div>
                    <h1 className="text-3xl font-bold">
                        {isFiltered ? 'Результаты поиска' : 'Популярные товары'}
                    </h1>
                    <p className="text-gray-500 text-sm mt-1">
                        {isFiltered ? `Найдено: ${totalCount}` : `За 2 недели: ${totalCount}`}
                    </p>
                </div>
                <div className="flex gap-2">
                    <button
                        onClick={() => setShowFilters(!showFilters)}
                        className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                    >
                        🔍 Фильтры
                    </button>
                    {isFiltered && (
                        <button
                            onClick={() => setFilters(null)}
                            className="px-4 py-2 bg-gray-200 rounded-lg hover:bg-gray-300"
                        >
                            ✖ Сбросить
                        </button>
                    )}
                </div>
            </div>

            {showFilters && (
                <ProductFilters
                    initialFilters={filters ?? {}}
                    onApply={(f) => { setFilters(f); setShowFilters(false) }}
                    onCancel={() => setShowFilters(false)}
                />
            )}

            {/* ✅ Товары с правильным ID */}
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                {normalizedProducts.map((product) => (
                    <ProductCard key={product.id} product={product} />
                ))}
            </div>

            {normalizedProducts.length === 0 && (
                <div className="text-center py-20 text-gray-500">Ничего не найдено</div>
            )}

            <div ref={loadMoreRef} className="py-8 text-center">
                {isFetchingNextPage && <div className="text-gray-500">Загрузка...</div>}
                {!hasNextPage && normalizedProducts.length > 0 && (
                    <div className="text-gray-400 text-sm">Все товары загружены</div>
                )}
            </div>
        </div>
    )
}