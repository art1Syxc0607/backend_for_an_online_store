import { Link } from 'react-router'
import { useAuth } from '@/features/auth/hooks/useAuth'

export default function Navbar() {
    const { isAuthenticated, user } = useAuth()

    return (
        <nav className="bg-white shadow">
            <div className="container mx-auto px-4 py-3 flex items-center justify-between">
                <Link to="/" className="text-xl font-bold text-blue-600">
                    ArtWebStore
                </Link>
                <div className="flex gap-4 items-center">
                    <Link to="/" className="text-gray-600 hover:text-gray-900">
                        Товары
                    </Link>
                    <Link to="/chat" className="text-gray-600 hover:text-gray-900">
                        AI Помощник
                    </Link>
                    {isAuthenticated ? (
                        <Link
                            to="/profile"
                            className="flex items-center gap-2 text-gray-600 hover:text-gray-900"
                        >
                            <span>👤</span>
                            <span>{user?.userName}</span>
                        </Link>
                    ) : (
                        <Link to="/login" className="text-gray-600 hover:text-gray-900">
                            Войти
                        </Link>
                    )}
                </div>
            </div>
        </nav>
    )
}