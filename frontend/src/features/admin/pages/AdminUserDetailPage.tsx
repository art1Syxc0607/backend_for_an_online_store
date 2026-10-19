import { useMemo, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useParams, Link } from 'react-router'
import { getAdminUserById } from '../api/adminUsersApi'
import type {
    AdminUserOrderBriefDto,
    AdminUserReviewBriefDto,
} from '../api/adminUsersApi'
import { getOrderStatusLabel } from '@/features/orders/api/orderApi'
import StarRating from '@/shared/ui/StarRating'

function formatDateTime(value?: string | null) {
    if (!value) return '—'
    return new Date(value).toLocaleString('ru-RU', {
        day: 'numeric', month: 'long', year: 'numeric',
        hour: '2-digit', minute: '2-digit',
    })
}

export default function AdminUserDetailPage() {
    const { id } = useParams<{ id: string }>()
    const userId = Number(id)

    const { data: user, isLoading, error } = useQuery({
        queryKey: ['admin', 'user', userId],
        queryFn: () => getAdminUserById(userId),
        enabled: !!userId,
    })

    if (isLoading) {
        return <div className="text-center py-20 text-gray-500">Загрузка...</div>
    }

    if (error || !user) {
        return (
            <div className="text-center py-20">
                <p className="text-red-500 mb-4">Пользователь не найден</p>
                <Link
                    to="/admin/users"
                    className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                >
                    ← К списку пользователей
                </Link>
            </div>
        )
    }

    const isAdmin = user.role === 'Admin'
    const orders = user.orders ?? []
    const reviews = user.reviews ?? []

    return (
        <div>
            <div className="mb-6">
                <Link to="/admin/users" className="text-blue-500 hover:underline text-sm">
                    ← К списку пользователей
                </Link>
                <div className="flex items-center justify-between mt-2">
                    <h1 className="text-3xl font-bold">{user.userName}</h1>
                    <div className="flex items-center gap-2">
                        <span className={`px-3 py-1 rounded-full text-sm font-medium ${
                            isAdmin ? 'bg-purple-100 text-purple-700' : 'bg-gray-100 text-gray-700'
                        }`}>
                            {isAdmin ? '👑 Admin' : '👤 User'}
                        </span>
                        <span className={`px-3 py-1 rounded-full text-sm font-medium ${
                            user.isActive ? 'bg-green-100 text-green-700' : 'bg-red-100 text-red-700'
                        }`}>
                            {user.isActive ? '✅ Активен' : '🚫 Заблокирован'}
                        </span>
                    </div>
                </div>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
                {/* Основная информация */}
                <div className="lg:col-span-2 space-y-4">
                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Профиль</h3>
                        <dl className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                            <div>
                                <dt className="text-gray-500">ID</dt>
                                <dd className="font-medium">#{user.id}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Имя пользователя</dt>
                                <dd className="font-medium">{user.userName}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Email</dt>
                                <dd className="font-medium">{user.email}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Роль</dt>
                                <dd className="font-medium">{isAdmin ? 'Администратор' : 'Пользователь'}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Дата регистрации</dt>
                                <dd className="font-medium">{formatDateTime(user.createdAt)}</dd>
                            </div>
                        </dl>
                    </div>

                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Статус аккаунта</h3>
                        <dl className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
                            <div>
                                <dt className="text-gray-500">Активен</dt>
                                <dd className="font-medium">{user.isActive ? 'Да' : 'Нет'}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Email подтверждён</dt>
                                <dd className="font-medium">{user.isEmailConfirmed ? 'Да' : 'Нет'}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Дата подтверждения email</dt>
                                <dd className="font-medium">{formatDateTime(user.emailConfirmedAt)}</dd>
                            </div>
                            <div>
                                <dt className="text-gray-500">Заблокирован</dt>
                                <dd className="font-medium">{formatDateTime(user.blockedAt)}</dd>
                            </div>
                            <div className="md:col-span-2">
                                <dt className="text-gray-500">Причина блокировки</dt>
                                <dd className="font-medium">{user.blockReason || '—'}</dd>
                            </div>
                        </dl>
                    </div>

                    <UserOrders key={user.id} orders={orders} />

                    <UserReviews key={user.id} reviews={reviews} />
                </div>

                {/* Статистика */}
                <div className="lg:col-span-1 space-y-4">
                    <div className="bg-white rounded-lg shadow p-6">
                        <h3 className="text-lg font-bold mb-4">Статистика</h3>
                        <dl className="space-y-3 text-sm">
                            <div className="flex justify-between">
                                <dt className="text-gray-500">Заказов</dt>
                                <dd className="font-bold">{user.ordersCount}</dd>
                            </div>
                            <div className="flex justify-between">
                                <dt className="text-gray-500">Всего потрачено</dt>
                                <dd className="font-bold text-green-600">
                                    {user.totalSpent.toLocaleString('ru-RU')} ₽
                                </dd>
                            </div>
                            <div className="flex justify-between border-t pt-3">
                                <dt className="text-gray-500">Отзывов</dt>
                                <dd className="font-bold">{user.reviewsCount}</dd>
                            </div>
                        </dl>
                    </div>
                </div>
            </div>
        </div>
    )
}

// ✅ Общий блок пагинации
function Pagination({
    page,
    totalPages,
    onChange,
}: {
    page: number
    totalPages: number
    onChange: (page: number) => void
}) {
    if (totalPages <= 1) return null

    return (
        <div className="flex justify-center items-center gap-2 mt-4">
            <button
                onClick={() => onChange(1)}
                disabled={page <= 1}
                title="Первая страница"
                className="px-2 py-1 bg-gray-200 rounded disabled:opacity-50 text-sm"
            >
                «
            </button>
            <button
                onClick={() => onChange(page - 1)}
                disabled={page <= 1}
                className="px-3 py-1 bg-gray-200 rounded disabled:opacity-50 text-sm"
            >
                ←
            </button>
            <span className="text-sm">
                Страница <strong>{page}</strong> из <strong>{totalPages}</strong>
            </span>
            <button
                onClick={() => onChange(page + 1)}
                disabled={page >= totalPages}
                className="px-3 py-1 bg-gray-200 rounded disabled:opacity-50 text-sm"
            >
                →
            </button>
            <button
                onClick={() => onChange(totalPages)}
                disabled={page >= totalPages}
                title="Последняя страница"
                className="px-2 py-1 bg-gray-200 rounded disabled:opacity-50 text-sm"
            >
                »
            </button>
        </div>
    )
}

const PAGE_SIZE = 5

// ✅ Заказы пользователя с поиском и пагинацией
function UserOrders({ orders }: { orders: AdminUserOrderBriefDto[] }) {
    const [search, setSearch] = useState('')
    const [page, setPage] = useState(1)

    const filtered = useMemo(() => {
        const term = search.trim().toLowerCase()
        if (!term) return orders
        return orders.filter((o) => {
            const { label } = getOrderStatusLabel(o.status)
            return (
                String(o.orderId).includes(term) ||
                label.toLowerCase().includes(term) ||
                o.totalAmount.toLocaleString('ru-RU').includes(term) ||
                new Date(o.createdAt).toLocaleDateString('ru-RU').includes(term)
            )
        })
    }, [orders, search])

    const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE))
    const current = Math.min(page, totalPages)
    const pageItems = filtered.slice((current - 1) * PAGE_SIZE, current * PAGE_SIZE)

    return (
        <div className="bg-white rounded-lg shadow p-6">
            <div className="flex flex-wrap items-center justify-between gap-3 mb-4">
                <h3 className="text-lg font-bold">Заказы ({filtered.length})</h3>
                <input
                    type="text"
                    value={search}
                    onChange={(e) => { setSearch(e.target.value); setPage(1) }}
                    placeholder="Поиск: номер, статус, сумма, дата..."
                    className="px-3 py-2 border rounded-lg text-sm w-full sm:w-72"
                />
            </div>

            {pageItems.length > 0 ? (
                <div className="overflow-x-auto">
                    <table className="w-full text-sm">
                        <thead>
                            <tr className="text-gray-500 border-b text-left">
                                <th className="py-2 pr-3">Заказ</th>
                                <th className="py-2 px-3">Дата</th>
                                <th className="py-2 px-3">Статус</th>
                                <th className="py-2 pl-3 text-right">Сумма</th>
                            </tr>
                        </thead>
                        <tbody>
                            {pageItems.map((o) => {
                                const status = getOrderStatusLabel(o.status)
                                return (
                                    <tr key={o.orderId} className="border-b last:border-0">
                                        <td className="py-2 pr-3">
                                            <a
                                                href={`/admin/orders/${o.orderId}`}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                className="text-blue-500 hover:underline font-medium"
                                            >
                                                #{o.orderId} ↗
                                            </a>
                                        </td>
                                        <td className="py-2 px-3 text-gray-600">
                                            {new Date(o.createdAt).toLocaleDateString('ru-RU')}
                                        </td>
                                        <td className="py-2 px-3">
                                            <span className={`px-2 py-0.5 rounded-full text-xs font-medium ${status.color}`}>
                                                {status.label}
                                            </span>
                                        </td>
                                        <td className="py-2 pl-3 text-right font-medium whitespace-nowrap">
                                            {o.totalAmount.toLocaleString('ru-RU')} ₽
                                        </td>
                                    </tr>
                                )
                            })}
                        </tbody>
                    </table>
                </div>
            ) : (
                <p className="text-sm text-gray-400">
                    {orders.length === 0 ? 'Заказов нет' : 'Ничего не найдено'}
                </p>
            )}

            <Pagination page={current} totalPages={totalPages} onChange={setPage} />
        </div>
    )
}

// ✅ Отзывы пользователя с поиском и пагинацией
function UserReviews({ reviews }: { reviews: AdminUserReviewBriefDto[] }) {
    const [search, setSearch] = useState('')
    const [page, setPage] = useState(1)

    const filtered = useMemo(() => {
        const term = search.trim().toLowerCase()
        if (!term) return reviews
        return reviews.filter((r) =>
            (r.productName ?? '').toLowerCase().includes(term) ||
            (r.text ?? '').toLowerCase().includes(term) ||
            String(r.productId).includes(term)
        )
    }, [reviews, search])

    const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE))
    const current = Math.min(page, totalPages)
    const pageItems = filtered.slice((current - 1) * PAGE_SIZE, current * PAGE_SIZE)

    return (
        <div className="bg-white rounded-lg shadow p-6">
            <div className="flex flex-wrap items-center justify-between gap-3 mb-4">
                <h3 className="text-lg font-bold">Отзывы ({filtered.length})</h3>
                <input
                    type="text"
                    value={search}
                    onChange={(e) => { setSearch(e.target.value); setPage(1) }}
                    placeholder="Поиск по товару или тексту отзыва..."
                    className="px-3 py-2 border rounded-lg text-sm w-full sm:w-72"
                />
            </div>

            {pageItems.length > 0 ? (
                <div className="space-y-3">
                    {pageItems.map((r) => (
                        <div key={r.reviewId} className="border-b pb-3 last:border-0 last:pb-0">
                            <div className="flex items-center justify-between gap-2">
                                <a
                                    href={`/admin/products/${r.productId}`}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="text-blue-500 hover:underline font-medium line-clamp-1"
                                >
                                    {r.productName || `Товар #${r.productId}`} ↗
                                </a>
                                <StarRating rating={r.rating} size="sm" />
                            </div>
                            <p className="text-sm text-gray-700 mt-1 whitespace-pre-wrap">{r.text}</p>
                            <div className="text-xs text-gray-400 mt-1">
                                {formatDateTime(r.createdAt)}
                            </div>
                        </div>
                    ))}
                </div>
            ) : (
                <p className="text-sm text-gray-400">
                    {reviews.length === 0 ? 'Отзывов нет' : 'Ничего не найдено'}
                </p>
            )}

            <Pagination page={current} totalPages={totalPages} onChange={setPage} />
        </div>
    )
}
