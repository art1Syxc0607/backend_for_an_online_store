import { createBrowserRouter } from 'react-router'
import App from './App'
import ProductsPage from '@/features/products/pages/ProductsPage'
import ChatPage from '@/features/agent-chat/pages/ChatPage'
import LoginPage from '@/features/auth/pages/LoginPage'
import NotFound from '@/shared/ui/NotFound'

export const router = createBrowserRouter([
    {
        path: '/',
        element: <App />,
        children: [
            { index: true, element: <ProductsPage /> },
            { path: 'chat', element: <ChatPage /> },
            { path: 'login', element: <LoginPage /> },
            { path: '*', element: <NotFound /> },
        ],
    },
])