import { apiClient } from '@/shared/api/client'
import type { UserProfileDto } from '@/shared/api/types'  // ✅

export interface LoginRequest {
    email: string
    password: string
}

export interface RegisterRequest {
    email: string
    userName: string
    password: string
}

export interface AuthResponse {
    token: string
    email: string
    userName: string
    role: string
}

export async function login(request: LoginRequest): Promise<AuthResponse> {
    const { data } = await apiClient.post<AuthResponse>('/auth/login', request)
    return data
}

export async function register(request: RegisterRequest): Promise<AuthResponse> {
    const { data } = await apiClient.post<AuthResponse>('/auth/register', request)
    return data
}

export async function getProfile(): Promise<UserProfileDto> {
    const { data } = await apiClient.get<UserProfileDto>('/auth/profile')
    return data
}