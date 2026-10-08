import axios from 'axios'

export const apiClient = axios.create({
    baseURL: '/api',
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true,
})

// ✅ Interceptor: добавляет JWT в каждый запрос
apiClient.interceptors.request.use((config) => {
    const token = localStorage.getItem('access_token')
    if (token) {
        config.headers.Authorization = `Bearer ${token}`
    }
    return config
})

// ✅ Interceptor: 401 → logout
apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('access_token')
            localStorage.removeItem('user')
            window.dispatchEvent(new Event('auth:logout'))
        }
        return Promise.reject(error)
    }
)