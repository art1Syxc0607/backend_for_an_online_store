import { createBrowserRouter } from 'react-router'
import App from './App'
import ProductsPage from '@/features/products/pages/ProductsPage'
import ProductDetailsPage from '@/features/products/pages/ProductDetailsPage'
import ChatPage from '@/features/agent-chat/pages/ChatPage'
import LoginPage from '@/features/auth/pages/LoginPage'
import ProfilePage from '@/features/auth/pages/ProfilePage'
import NotFound from '@/shared/ui/NotFound'

export const router = createBrowserRouter([
    {
        path: '/',
        element: <App />,
        children: [
            { index: true, element: <ProductsPage /> },
            { path: 'products/:id', element: <ProductDetailsPage /> },
            { path: 'chat', element: <ChatPage /> },
            { path: 'login', element: <LoginPage /> },
            { path: 'profile', element: <ProfilePage /> },
            { path: '*', element: <NotFound /> },
        ],
    },
])