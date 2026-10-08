import { useAuth } from '../hooks/useAuth'

export default function ProfilePage() {
    const { user, isLoadingProfile, logout } = useAuth()

    if (isLoadingProfile) return <div className="text-center py-20">Загрузка...</div>
    if (!user) return <div className="text-center py-20">Не авторизован</div>

    return (
        <div className="max-w-2xl mx-auto">
            <h1 className="text-3xl font-bold mb-6">Личный кабинет</h1>

            <div className="bg-white rounded-lg shadow p-6 mb-4">
                <h2 className="font-bold mb-4">Профиль</h2>
                <div className="space-y-2">
                    <div className="flex justify-between">
                        <span className="text-gray-600">Имя:</span>
                        <span className="font-medium">{user.userName}</span>
                    </div>
                    <div className="flex justify-between">
                        <span className="text-gray-600">Email:</span>
                        <span className="font-medium">{user.email}</span>
                    </div>
                    <div className="flex justify-between">
                        <span className="text-gray-600">Роль:</span>
                        <span className="font-medium">{user.role}</span>
                    </div>
                    <div className="flex justify-between">
                        <span className="text-gray-600">Email подтверждён:</span>
                        <span>{user.isEmailConfirmed ? '✅' : '❌'}</span>
                    </div>
                    <div className="flex justify-between">
                        <span className="text-gray-600">Дата регистрации:</span>
                        <span>{new Date(user.createdAt).toLocaleDateString('ru-RU')}</span>
                    </div>
                </div>
            </div>

            <button
                onClick={logout}
                className="w-full px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600"
            >
                Выйти
            </button>
        </div>
    )
}