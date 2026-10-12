import { Outlet } from 'react-router'
import Navbar from '@/shared/ui/Navbar'
import CartDrawer from '@/features/cart/components/CartDrawer'
import CurrentOrderDrawer from '@/features/current-order/components/CurrentOrderDrawer'
import AIAssistantWidget from '@/features/agent-chat/components/AIAssistantWidget'
import { CartProvider } from '@/features/cart/context/CartContext'
import { CurrentOrderProvider } from '@/features/current-order/context/CurrentOrderContext'
import ErrorBoundary from '@/shared/ui/ErrorBoundary'

export default function App() {
    return (
        <CartProvider>
            <CurrentOrderProvider>
                <div className="min-h-screen bg-gray-50">
                    <Navbar />
                    <main className="container mx-auto px-4 py-8">
                        <ErrorBoundary>
                            <Outlet />
                        </ErrorBoundary>
                    </main>
                    <CartDrawer />
                    <CurrentOrderDrawer />

                    {/* ✅ AI Assistant Widget */}
                    <AIAssistantWidget />
                </div>
            </CurrentOrderProvider>
        </CartProvider>
    )
}