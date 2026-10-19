import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
    getAllReviews,
    respondToReview,
    updateReviewResponse,
    removeReviewResponse,
} from '../api/adminReviewsApi'
import AdminTable from '../components/AdminTable'
import type { Column } from '../components/AdminTable'
import AdminModal from '../components/AdminModal'
import type { ReviewDto } from '@/features/reviews/api/reviewApi'

export default function AdminReviewsPage() {
    const queryClient = useQueryClient()
    const [filters, setFilters] = useState({
        isResponded: undefined as boolean | undefined,
        minRating: undefined as number | undefined,
        maxRating: undefined as number | undefined,
        searchTerm: '',
        pageNumber: 1,
        pageSize: 20,
    })
    const [respondingReview, setRespondingReview] = useState<ReviewDto | null>(null)
    const [removingId, setRemovingId] = useState<number | null>(null)
    const [error, setError] = useState<string | null>(null)

    const reviewsQuery = useQuery({
        queryKey: ['admin', 'reviews', filters],
        queryFn: () => getAllReviews(filters),
    })

    const respondMutation = useMutation({
        mutationFn: ({ id, text }: { id: number; text: string }) =>
            respondToReview(id, text),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin', 'reviews'] })
            setRespondingReview(null)
        },
        onError: (e: any) => setError(e?.response?.data?.error ?? 'Ошибка'),
    })

    const updateMutation = useMutation({
        mutationFn: ({ id, text }: { id: number; text: string }) =>
            updateReviewResponse(id, text),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin', 'reviews'] })
            setRespondingReview(null)
        },
        onError: (e: any) => setError(e?.response?.data?.error ?? 'Ошибка'),
    })

    const removeMutation = useMutation({
        mutationFn: removeReviewResponse,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin', 'reviews'] })
            setRemovingId(null)
        },
        onError: (e: any) => setError(e?.response?.data?.error ?? 'Ошибка'),
    })

    const columns: Column<ReviewDto>[] = [
        {
            key: 'id',
            title: 'ID',
            width: '60px',
            render: (r) => <span className="text-gray-500">#{r.id}</span>,
        },
        {
            key: 'product',
            title: 'Товар',
            render: (r) => (
                <a
                    href={`/admin/products/${r.productId}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-sm text-blue-500 hover:underline"
                >
                    {r.productName || `ID: ${r.productId}`} ↗
                </a>
            ),
        },
        {
            key: 'user',
            title: 'Пользователь',
            render: (r) => (
                <a
                    href={`/admin/users/${r.userId}`}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-sm text-blue-500 hover:underline"
                >
                    {r.userName || `ID: ${r.userId}`} ↗
                </a>
            ),
        },
        {
            key: 'rating',
            title: 'Оценка',
            align: 'center',
            render: (r) => <span>⭐ {r.rating}</span>,
        },
        {
            key: 'text',
            title: 'Текст',
            render: (r) => (
                <span className="text-sm text-gray-700 line-clamp-2">{r.text}</span>
            ),
        },
        {
            key: 'response',
            title: 'Ответ',
            render: (r) =>
                r.adminResponse ? (
                    <span className="text-xs text-green-600">✅ Есть</span>
                ) : (
                    <span className="text-xs text-gray-400">—</span>
                ),
        },
        {
            key: 'actions',
            title: 'Действия',
            align: 'right',
            render: (r) => (
                <div className="flex gap-2 justify-end">
                    <button
                        onClick={() => setRespondingReview(r)}
                        className="px-3 py-1 text-xs bg-blue-500 text-white rounded hover:bg-blue-600"
                    >
                        {r.adminResponse ? '✏️' : '💬'}
                    </button>
                    {r.adminResponse && (
                        <button
                            onClick={() => setRemovingId(r.id)}
                            className="px-3 py-1 text-xs bg-red-500 text-white rounded hover:bg-red-600"
                        >
                            🗑
                        </button>
                    )}
                </div>
            ),
        },
    ]

    return (
        <div>
            <h1 className="text-3xl font-bold mb-6">⭐ Отзывы</h1>

            {/* Фильтры */}
            <div className="bg-white rounded-lg shadow p-4 mb-6">
                <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Поиск</label>
                        <input
                            type="text"
                            value={filters.searchTerm}
                            onChange={(e) => setFilters({ ...filters, searchTerm: e.target.value, pageNumber: 1 })}
                            placeholder="Текст отзыва..."
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        />
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Есть ответ</label>
                        <select
                            value={filters.isResponded === undefined ? '' : String(filters.isResponded)}
                            onChange={(e) => setFilters({
                                ...filters,
                                isResponded: e.target.value === '' ? undefined : e.target.value === 'true',
                                pageNumber: 1,
                            })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        >
                            <option value="">Все</option>
                            <option value="true">С ответом</option>
                            <option value="false">Без ответа</option>
                        </select>
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Мин. оценка</label>
                        <input
                            type="number"
                            min={1}
                            max={5}
                            value={filters.minRating ?? ''}
                            onChange={(e) => setFilters({
                                ...filters,
                                minRating: e.target.value ? Number(e.target.value) : undefined,
                                pageNumber: 1,
                            })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        />
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Макс. оценка</label>
                        <input
                            type="number"
                            min={1}
                            max={5}
                            value={filters.maxRating ?? ''}
                            onChange={(e) => setFilters({
                                ...filters,
                                maxRating: e.target.value ? Number(e.target.value) : undefined,
                                pageNumber: 1,
                            })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        />
                    </div>
                </div>
            </div>

            {reviewsQuery.isError && (
                <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded text-sm">
                    Ошибка загрузки отзывов
                </div>
            )}

            <AdminTable
                columns={columns}
                data={reviewsQuery.data?.items ?? []}
                isLoading={reviewsQuery.isLoading}
                keyExtractor={(r) => r.id}
                emptyMessage="Отзывов нет"
            />

            {/* Пагинация */}
            {reviewsQuery.data && reviewsQuery.data.totalPages > 1 && (
                <div className="flex justify-center items-center gap-2 mt-6">
                    <button
                        onClick={() => setFilters((f) => ({ ...f, pageNumber: Math.max(1, f.pageNumber - 1) }))}
                        disabled={!reviewsQuery.data.hasPreviousPage}
                        className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
                    >
                        ←
                    </button>
                    <span className="text-sm">
                        {reviewsQuery.data.pageNumber} / {reviewsQuery.data.totalPages}
                    </span>
                    <button
                        onClick={() => setFilters((f) => ({ ...f, pageNumber: f.pageNumber + 1 }))}
                        disabled={!reviewsQuery.data.hasNextPage}
                        className="px-4 py-2 bg-gray-200 rounded disabled:opacity-50"
                    >
                        →
                    </button>
                </div>
            )}

            {/* Модалка ответа */}
            <AdminModal
                isOpen={!!respondingReview}
                title={respondingReview?.adminResponse ? 'Редактировать ответ' : 'Ответить на отзыв'}
                onClose={() => { setRespondingReview(null); setError(null) }}
                maxWidth="md"
                footer={null}
            >
                {error && (
                    <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded text-sm">
                        {error}
                    </div>
                )}
                {respondingReview && (
                    <ReviewResponseForm
                        review={respondingReview}
                        onSubmit={async (text) => {
                            setError(null)
                            if (respondingReview.adminResponse) {
                                await updateMutation.mutateAsync({ id: respondingReview.id, text })
                            } else {
                                await respondMutation.mutateAsync({ id: respondingReview.id, text })
                            }
                        }}
                        onCancel={() => setRespondingReview(null)}
                    />
                )}
            </AdminModal>

            {/* Удаление ответа */}
            <AdminModal
                isOpen={!!removingId}
                title="Удалить ответ?"
                onClose={() => setRemovingId(null)}
                maxWidth="sm"
                footer={
                    <div className="flex gap-2">
                        <button
                            onClick={() => removingId && removeMutation.mutate(removingId)}
                            disabled={removeMutation.isPending}
                            className="flex-1 px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 disabled:opacity-50"
                        >
                            {removeMutation.isPending ? '...' : 'Да, удалить'}
                        </button>
                        <button
                            onClick={() => setRemovingId(null)}
                            className="flex-1 px-4 py-2 bg-gray-200 rounded-lg hover:bg-gray-300"
                        >
                            Отмена
                        </button>
                    </div>
                }
            >
                <p>Вы уверены, что хотите удалить ответ на этот отзыв?</p>
            </AdminModal>
        </div>
    )
}

function ReviewResponseForm({
    review,
    onSubmit,
    onCancel,
}: {
    review: ReviewDto
    onSubmit: (text: string) => Promise<void>
    onCancel: () => void
}) {
    const [text, setText] = useState(review.adminResponse ?? '')
    const [isSubmitting, setIsSubmitting] = useState(false)

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        setIsSubmitting(true)
        try {
            await onSubmit(text)
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            <div className="bg-gray-50 rounded-lg p-3 text-sm">
                <div className="text-gray-500 mb-1">Отзыв:</div>
                <div>{review.text}</div>
                <div className="text-yellow-600 mt-2">⭐ {review.rating}/5</div>
            </div>

            <div>
                <label className="block text-sm font-medium mb-1">Ваш ответ *</label>
                <textarea
                    value={text}
                    onChange={(e) => setText(e.target.value)}
                    required
                    rows={4}
                    className="w-full px-3 py-2 border rounded-lg resize-none focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
            </div>

            <div className="flex gap-2 pt-2">
                <button
                    type="submit"
                    disabled={isSubmitting || !text.trim()}
                    className="flex-1 px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50"
                >
                    {isSubmitting ? '...' : 'Сохранить'}
                </button>
                <button
                    type="button"
                    onClick={onCancel}
                    className="flex-1 px-4 py-2 bg-gray-200 rounded-lg hover:bg-gray-300"
                >
                    Отмена
                </button>
            </div>
        </form>
    )
}