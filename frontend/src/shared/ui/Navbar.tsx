import { Link } from 'react-router'

export default function Navbar() {
    return (
        <nav className="bg-white shadow">
            <div className="container mx-auto px-4 py-3 flex items-center justify-between">
                <Link to="/" className="text-xl font-bold text-blue-600">
                    ArtWebStore
                </Link>
                <div className="flex gap-4">
                    <Link to="/" className="text-gray-600 hover:text-gray-900">
                        Товары
                    </Link>
                    <Link to="/chat" className="text-gray-600 hover:text-gray-900">
                        AI Помощник
                    </Link>
                    <Link to="/login" className="text-gray-600 hover:text-gray-900">
                        Войти
                    </Link>
                </div>
            </div>
        </nav>
    )
}