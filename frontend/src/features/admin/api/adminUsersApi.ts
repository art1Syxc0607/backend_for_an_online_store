import { apiClient } from '@/shared/api/client'

export enum UserRole {
    User = 'User',
    Admin = 'Admin',
}

export enum SortUserBy {
    CreatedAt = 'CreatedAt',
    UserName = 'UserName',
    Email = 'Email',
    OrdersCount = 'OrdersCount',
    TotalSpent = 'TotalSpent',
}

export interface UserAdminDto {
    id: number
    email: string
    userName: string
    role: UserRole
    isEmailConfirmed: boolean
    isActive: boolean
    createdAt: string
    ordersCount: number
    totalSpent: number
}

export interface GetAllUsersParams {
    search?: string
    role?: UserRole
    isActive?: boolean
    pageNumber?: number
    pageSize?: number
    sortBy?: SortUserBy
    sortDesc?: boolean
}

export async function getAllUsers(
    params: GetAllUsersParams = {}
): Promise<UserAdminDto[]> {
    const { data } = await apiClient.get<UserAdminDto[]>('/admin/users', {
        params,
    })
    return data
}

export async function promoteToAdmin(userId: number): Promise<void> {
    await apiClient.post(`/admin/users/${userId}/promote`)
}

export async function demoteFromAdmin(userId: number): Promise<void> {
    await apiClient.post(`/admin/users/${userId}/demote`)
}