import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
    addCategory,
    updateCategory,
    deleteCategory,
} from '../api/adminCategoriesApi'
import AdminTable from '../components/AdminTable'
import type { Column } from '../components/AdminTable'
import AdminModal from '../components/AdminModal'
import { apiClient } from '@/shared/api/client'

// ✅ DTO категории
interface CategoryDto {
    id: number
    name: string
    description?: string
    productsCount?: number
    createdAt?: string
}

// ✅ Получить все категории
async function getCategories(): Promise<CategoryDto[]> {
    const { data } = await apiClient.get<CategoryDto[]>('/categories')   // ← множественное!
    return data
}

export default function AdminCategoriesPage() {
    const queryClient = useQueryClient()
    const [isCreating, setIsCreating] = useState(false)
    const [editingCategory, setEditingCategory] = useState<CategoryDto | null>(null)
    const [deletingId, setDeletingId] = useState<number | null>(null)
    const [error, setError] = useState<string | null>(null)

    const categoriesQuery = useQuery({
        queryKey: ['admin', 'categories'],
        queryFn: getCategories,
    })

    // ✅ Удаление
    const deleteMutation = useMutation({
        mutationFn: deleteCategory,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin', 'categories'] })
            setDeletingId(null)
        },
    })

    // ✅ Колонки
    const columns: Column<CategoryDto>[] = [
        {
            key: 'id',
            title: 'ID',
            width: '80px',
            render: (c) => <span className="text-gray-500">#{c.id}</span>,
        },
        {
            key: 'name',
            title: 'Название',
            render: (c) => (
                <span className="font-medium">{c.name}</span>
            ),
        },
        {
            key: 'description',
            title: 'Описание',
            render: (c) => (
                <span className="text-sm text-gray-600">
                    {c.description || '—'}
                </span>
            ),
        },
        {
            key: 'actions',
            title: 'Действия',
            align: 'right',
            render: (c) => (
                <div className="flex gap-2 justify-end">
                    <button
                        onClick={() => setEditingCategory(c)}
                        className="px-3 py-1 text-xs bg-blue-500 text-white rounded hover:bg-blue-600"
                    >
                        ✏️
                    </button>
                    <button
                        onClick={() => setDeletingId(c.id)}
                        className="px-3 py-1 text-xs bg-red-500 text-white rounded hover:bg-red-600"
                    >
                        🗑
                    </button>
                </div>
            ),
        },
    ]

    return (
        <div>
            <div className="flex items-center justify-between mb-6">
                <h1 className="text-3xl font-bold">📁 Категории</h1>
                <button
                    onClick={() => setIsCreating(true)}
                    className="px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                >
                    + Добавить категорию
                </button>
            </div>

            <AdminTable
                columns={columns}
                data={categoriesQuery.data ?? []}
                isLoading={categoriesQuery.isLoading}
                keyExtractor={(c) => c.id}
                emptyMessage="Категорий нет"
            />

            {/* ✅ Модалка создания/редактирования */}
            <AdminModal
                isOpen={isCreating || !!editingCategory}
                title={isCreating ? 'Добавить категорию' : 'Редактировать категорию'}
                onClose={() => {
                    setIsCreating(false)
                    setEditingCategory(null)
                    setError(null)
                }}
                maxWidth="md"
            >
                {error && (
                    <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded text-sm">
                        {error}
                    </div>
                )}

                <CategoryForm
                    category={editingCategory}
                    onSubmit={async (data) => {
                        setError(null)
                        try {
                            if (editingCategory) {
                                await updateCategory({
                                    id: editingCategory.id,
                                    name: data.name,
                                    description: data.description,
                                })
                            } else {
                                await addCategory({
                                    name: data.name,
                                    description: data.description,
                                })
                            }
                            queryClient.invalidateQueries({ queryKey: ['admin', 'categories'] })
                            setIsCreating(false)
                            setEditingCategory(null)
                        } catch (err: any) {
                            setError(err?.response?.data?.error ?? 'Ошибка сохранения')
                        }
                    }}
                    onCancel={() => {
                        setIsCreating(false)
                        setEditingCategory(null)
                    }}
                />
            </AdminModal>

            {/* ✅ Подтверждение удаления */}
            <AdminModal
                isOpen={!!deletingId}
                title="Удалить категорию?"
                onClose={() => setDeletingId(null)}
                maxWidth="sm"
                footer={
                    <div className="flex gap-2">
                        <button
                            onClick={() => deletingId && deleteMutation.mutate(deletingId)}
                            disabled={deleteMutation.isPending}
                            className="flex-1 px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600 disabled:opacity-50"
                        >
                            {deleteMutation.isPending ? '...' : 'Да, удалить'}
                        </button>
                        <button
                            onClick={() => setDeletingId(null)}
                            className="flex-1 px-4 py-2 bg-gray-200 rounded-lg hover:bg-gray-300"
                        >
                            Отмена
                        </button>
                    </div>
                }
            >
                <p>Вы уверены, что хотите удалить эту категорию? Действие нельзя отменить.</p>
            </AdminModal>
        </div>
    )
}

// ✅ Форма категории
function CategoryForm({
    category,
    onSubmit,
    onCancel,
}: {
    category: CategoryDto | null
    onSubmit: (data: { name: string; description?: string }) => Promise<void>
    onCancel: () => void
}) {
    const [form, setForm] = useState({
        name: category?.name ?? '',
        description: category?.description ?? '',
    })
    const [isSubmitting, setIsSubmitting] = useState(false)

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        setIsSubmitting(true)
        try {
            await onSubmit(form)
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <form onSubmit={handleSubmit} className="space-y-4">
            <div>
                <label className="block text-sm font-medium mb-1">Название *</label>
                <input
                    type="text"
                    value={form.name}
                    onChange={(e) => setForm({ ...form, name: e.target.value })}
                    required
                    className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
            </div>

            <div>
                <label className="block text-sm font-medium mb-1">Описание</label>
                <textarea
                    value={form.description}
                    onChange={(e) => setForm({ ...form, description: e.target.value })}
                    rows={3}
                    className="w-full px-3 py-2 border rounded-lg resize-none focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
            </div>

            <div className="flex gap-2 pt-4">
                <button
                    type="submit"
                    disabled={isSubmitting}
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