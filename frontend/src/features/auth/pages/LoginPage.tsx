import { useState } from 'react'
import { Link } from 'react-router'
import { useAuth } from '../hooks/useAuth'

export default function LoginPage() {
    const [mode, setMode] = useState<'login' | 'register'>('login')
    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [userName, setUserName] = useState('')

    const {
        login, loginError, isLoggingIn,
        register, registerError, isRegistering,
    } = useAuth()

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        try {
            if (mode === 'login') {
                await login({ email, password })
            } else {
                await register({ email, userName, password })
            }
        } catch (err) {
            // Ошибка отобразится через loginError/registerError
        }
    }

    const currentError = mode === 'login' ? loginError : registerError
    const isLoading = mode === 'login' ? isLoggingIn : isRegistering

    return (
        <div className="max-w-md mx-auto mt-20 bg-white rounded-lg shadow p-8">
            <h1 className="text-2xl font-bold mb-6 text-center">
                {mode === 'login' ? 'Вход' : 'Регистрация'}
            </h1>

            {/* Ошибка */}
            {currentError && (
                <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
                    {mode === 'login' ? 'Неверный email или пароль' : 'Ошибка регистрации'}
                </div>
            )}

            <form onSubmit={handleSubmit} className="space-y-4">
                {mode === 'register' && (
                    <div>
                        <label className="block text-sm font-medium mb-1">Имя пользователя</label>
                        <input
                            type="text"
                            value={userName}
                            onChange={(e) => setUserName(e.target.value)}
                            className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                            required
                        />
                    </div>
                )}

                <div>
                    <label className="block text-sm font-medium mb-1">Email</label>
                    <input
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                        required
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium mb-1">Пароль</label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                        required
                    />
                </div>

                <button
                    type="submit"
                    disabled={isLoading}
                    className="w-full px-4 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50"
                >
                    {isLoading ? '...' : (mode === 'login' ? 'Войти' : 'Зарегистрироваться')}
                </button>
            </form>

            {/* Переключение режима */}
            <div className="mt-4 text-center text-sm">
                {mode === 'login' ? (
                    <>
                        Нет аккаунта?{' '}
                        <button
                            onClick={() => setMode('register')}
                            className="text-blue-600 hover:underline"
                        >
                            Зарегистрироваться
                        </button>
                    </>
                ) : (
                    <>
                        Уже есть аккаунт?{' '}
                        <button
                            onClick={() => setMode('login')}
                            className="text-blue-600 hover:underline"
                        >
                            Войти
                        </button>
                    </>
                )}
            </div>
        </div>
    )
}