import axios from 'axios'

export const apiClient = axios.create({
    baseURL: '/api',  // ← ОТНОСИТЕЛЬНЫЙ путь, НЕ http://localhost:7197
    headers: {
        'Content-Type': 'application/json',
    },
    withCredentials: true,
})

// Interceptor для JWT
apiClient.interceptors.request.use((config) => {
    const token = localStorage.getItem('access_token')
    if (token) {
        config.headers.Authorization = `Bearer ${token}`
    }
    return config
})

// Interceptor для 401
apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('access_token')
            // window.location.href = '/login'  // Раскомментируйте, если нужен редирект
        }
        return Promise.reject(error)
    }
)