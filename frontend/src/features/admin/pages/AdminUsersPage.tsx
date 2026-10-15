import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
    getAllUsers,
    promoteToAdmin,
    demoteFromAdmin,
    UserRole,
    SortUserBy,
} from '../api/adminUsersApi'
import AdminTable from '../components/AdminTable'
import type { Column } from '../components/AdminTable'
import AdminModal from '../components/AdminModal'
import type { UserAdminDto } from '../api/adminUsersApi'

export default function AdminUsersPage() {
    const queryClient = useQueryClient()
    const [filters, setFilters] = useState({
        search: '',
        role: undefined as UserRole | undefined,
        isActive: undefined as boolean | undefined,
        pageNumber: 1,
        pageSize: 20,
        sortBy: SortUserBy.CreatedAt,
        sortDesc: true,
    })
    const [promotingId, setPromotingId] = useState<number | null>(null)
    const [demotingId, setDemotingId] = useState<number | null>(null)

    const usersQuery = useQuery({
        queryKey: ['admin', 'users', filters],
        queryFn: () => getAllUsers(filters),
    })

    const promoteMutation = useMutation({
        mutationFn: promoteToAdmin,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin', 'users'] })
            setPromotingId(null)
        },
    })

    const demoteMutation = useMutation({
        mutationFn: demoteFromAdmin,
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['admin', 'users'] })
            setDemotingId(null)
        },
    })

    const columns: Column<UserAdminDto>[] = [
        {
            key: 'id',
            title: 'ID',
            width: '60px',
            render: (u) => <span className="text-gray-500">#{u.id}</span>,
        },
        {
            key: 'userName',
            title: 'Пользователь',
            render: (u) => (
                <div>
                    <div className="font-medium">{u.userName}</div>
                    <div className="text-xs text-gray-500">{u.email}</div>
                </div>
            ),
        },
        {
            key: 'role',
            title: 'Роль',
            render: (u) => (
                <span className={`px-2 py-1 rounded text-xs font-medium ${u.role === UserRole.Admin
                        ? 'bg-purple-100 text-purple-700'
                        : 'bg-gray-100 text-gray-700'
                    }`}>
                    {u.role === UserRole.Admin ? '👑 Admin' : '👤 User'}
                </span>
            ),
        },
        {
            key: 'isActive',
            title: 'Активен',
            align: 'center',
            render: (u) => u.isActive ? '✅' : '❌',
        },
        {
            key: 'isEmailConfirmed',
            title: 'Email',
            align: 'center',
            render: (u) => u.isEmailConfirmed ? '✅' : '⏳',
        },
        {
            key: 'stats',
            title: 'Заказы',
            align: 'right',
            render: (u) => (
                <div className="text-sm">
                    <div>{u.ordersCount} шт.</div>
                    <div className="text-gray-500">{u.totalSpent.toLocaleString('ru-RU')} ₽</div>
                </div>
            ),
        },
        {
            key: 'createdAt',
            title: 'Регистрация',
            render: (u) => (
                <span className="text-xs text-gray-500">
                    {new Date(u.createdAt).toLocaleDateString('ru-RU')}
                </span>
            ),
        },
        {
            key: 'actions',
            title: 'Действия',
            align: 'right',
            render: (u) => (
                <div className="flex gap-2 justify-end">
                    {u.role === UserRole.User ? (
                        <button
                            onClick={() => setPromotingId(u.id)}
                            className="px-3 py-1 text-xs bg-purple-500 text-white rounded hover:bg-purple-600"
                        >
                            👑 Назначить
                        </button>
                    ) : (
                        <button
                            onClick={() => setDemotingId(u.id)}
                            className="px-3 py-1 text-xs bg-orange-500 text-white rounded hover:bg-orange-600"
                        >
                            ⬇️ Снять
                        </button>
                    )}
                </div>
            ),
        },
    ]

    return (
        <div>
            <h1 className="text-3xl font-bold mb-6">👥 Пользователи</h1>

            {/* Фильтры */}
            <div className="bg-white rounded-lg shadow p-4 mb-6">
                <div className="grid grid-cols-1 md:grid-cols-4 gap-3">
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Поиск</label>
                        <input
                            type="text"
                            value={filters.search}
                            onChange={(e) => setFilters({ ...filters, search: e.target.value, pageNumber: 1 })}
                            placeholder="Имя или email..."
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        />
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Роль</label>
                        <select
                            value={filters.role ?? ''}
                            onChange={(e) => setFilters({
                                ...filters,
                                role: e.target.value ? (e.target.value as UserRole) : undefined,
                                pageNumber: 1,
                            })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        >
                            <option value="">Все</option>
                            <option value={UserRole.User}>Пользователи</option>
                            <option value={UserRole.Admin}>Администраторы</option>
                        </select>
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Статус</label>
                        <select
                            value={filters.isActive === undefined ? '' : String(filters.isActive)}
                            onChange={(e) => setFilters({
                                ...filters,
                                isActive: e.target.value === '' ? undefined : e.target.value === 'true',
                                pageNumber: 1,
                            })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        >
                            <option value="">Все</option>
                            <option value="true">Активные</option>
                            <option value="false">Заблокированные</option>
                        </select>
                    </div>
                    <div>
                        <label className="block text-xs font-medium mb-1 text-gray-600">Сортировка</label>
                        <select
                            value={filters.sortBy}
                            onChange={(e) => setFilters({ ...filters, sortBy: e.target.value as SortUserBy })}
                            className="w-full px-3 py-2 border rounded-lg text-sm"
                        >
                            <option value={SortUserBy.CreatedAt}>По дате</option>
                            <option value={SortUserBy.UserName}>По имени</option>
                            <option value={SortUserBy.Email}>По email</option>
                            <option value={SortUserBy.OrdersCount}>По заказам</option>
                            <option value={SortUserBy.TotalSpent}>По тратам</option>
                        </select>
                    </div>
                </div>
            </div>

            <AdminTable
                columns={columns}
                data={usersQuery.data ?? []}
                isLoading={usersQuery.isLoading}
                keyExtractor={(u) => u.id}
                emptyMessage="Пользователей нет"
            />

            {/* Promote */}
            <AdminModal
                isOpen={!!promotingId}
                title="Назначить администратором?"
                onClose={() => setPromotingId(null)}
                maxWidth="sm"
                footer={
                    <div className="flex gap-2">
                        <button
                            onClick={() => promotingId && promoteMutation.mutate(promotingId)}
                            disabled={promoteMutation.isPending}
                            className="flex-1 px-4 py-2 bg-purple-500 text-white rounded-lg hover:bg-purple-600 disabled:opacity-50"
                        >
                            {promoteMutation.isPending ? '...' : 'Да, назначить'}
                        </button>
                        <button
                            onClick={() => setPromotingId(null)}
                            className="flex-1 px-4 py-2 bg-gray-200 rounded-lg hover:bg-gray-300"
                        >
                            Отмена
                        </button>
                    </div>
                }
            >
                <p>Пользователь получит полный доступ к админ-панели.</p>
            </AdminModal>

            {/* Demote */}
            <AdminModal
                isOpen={!!demotingId}
                title="Снять права администратора?"
                onClose={() => setDemotingId(null)}
                maxWidth="sm"
                footer={
                    <div className="flex gap-2">
                        <button
                            onClick={() => demotingId && demoteMutation.mutate(demotingId)}
                            disabled={demoteMutation.isPending}
                            className="flex-1 px-4 py-2 bg-orange-500 text-white rounded-lg hover:bg-orange-600 disabled:opacity-50"
                        >
                            {demoteMutation.isPending ? '...' : 'Да, снять'}
                        </button>
                        <button
                            onClick={() => setDemotingId(null)}
                            className="flex-1 px-4 py-2 bg-gray-200 rounded-lg hover:bg-gray-300"
                        >
                            Отмена
                        </button>
                    </div>
                }
            >
                <p>Пользователь потеряет доступ к админ-панели.</p>
            </AdminModal>
        </div>
    )
}