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

// ✅ Детальная информация о пользователе (для админки)
export interface AdminUserOrderBriefDto {
    orderId: number
    totalAmount: number
    status: string
    createdAt: string
}

export interface AdminUserReviewBriefDto {
    reviewId: number
    productId: number
    productName: string
    rating: number
    text: string
    createdAt: string
}

export interface AdminUserDetailDto {
    id: number
    email: string
    userName: string
    role: string
    isEmailConfirmed: boolean
    emailConfirmedAt?: string
    isActive: boolean
    blockReason?: string
    blockedAt?: string
    createdAt: string
    ordersCount: number
    totalSpent: number
    reviewsCount: number
    orders: AdminUserOrderBriefDto[]
    reviews: AdminUserReviewBriefDto[]
}

export async function getAdminUserById(userId: number): Promise<AdminUserDetailDto> {
    const { data } = await apiClient.get<AdminUserDetailDto>(`/admin/users/${userId}`)
    return data
}

export async function promoteToAdmin(userId: number): Promise<void> {
    await apiClient.post(`/admin/users/${userId}/promote`)
}

export async function demoteFromAdmin(userId: number): Promise<void> {
    await apiClient.post(`/admin/users/${userId}/demote`)
}