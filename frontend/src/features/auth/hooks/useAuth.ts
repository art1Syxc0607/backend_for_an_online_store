import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router'
import { login, register, getProfile } from '../api/authApi'
import type { LoginRequest, RegisterRequest } from '../api/authApi'  // ✅

const TOKEN_KEY = 'access_token'

export function useAuth() {
    const queryClient = useQueryClient()
    const navigate = useNavigate()

    const profileQuery = useQuery({
        queryKey: ['profile'],
        queryFn: getProfile,
        enabled: !!localStorage.getItem(TOKEN_KEY),
        retry: false,
    })

    const loginMutation = useMutation({
        mutationFn: (request: LoginRequest) => login(request),
        onSuccess: (data) => {
            localStorage.setItem(TOKEN_KEY, data.token)
            localStorage.setItem('user', JSON.stringify(data))
            queryClient.invalidateQueries({ queryKey: ['profile'] })
            navigate('/')
        },
    })

    const registerMutation = useMutation({
        mutationFn: (request: RegisterRequest) => register(request),
        onSuccess: (data) => {
            localStorage.setItem(TOKEN_KEY, data.token)
            localStorage.setItem('user', JSON.stringify(data))
            queryClient.invalidateQueries({ queryKey: ['profile'] })
            navigate('/')
        },
    })

    const logout = () => {
        localStorage.removeItem(TOKEN_KEY)
        localStorage.removeItem('user')
        queryClient.clear()
        navigate('/login')
    }

    return {
        user: profileQuery.data,
        isAuthenticated: !!localStorage.getItem(TOKEN_KEY),
        isLoadingProfile: profileQuery.isLoading,
        login: loginMutation.mutateAsync,
        loginError: loginMutation.error,
        isLoggingIn: loginMutation.isPending,
        register: registerMutation.mutateAsync,
        registerError: registerMutation.error,
        isRegistering: registerMutation.isPending,
        logout,
    }
}