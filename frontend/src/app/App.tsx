import { Outlet } from 'react-router'
import Navbar from '@/shared/ui/Navbar'

export default function App() {
    return (
        <div className="min-h-screen bg-gray-50">
            <Navbar />
            <main className="container mx-auto px-4 py-8">
                <Outlet />
            </main>
        </div>
    )
}