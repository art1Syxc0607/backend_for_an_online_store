import { createBrowserRouter } from 'react-router'
import App from './App'
import ProductsPage from '@/features/products/pages/ProductsPage'
import ProductDetailsPage from '@/features/products/pages/ProductDetailsPage'
import LoginPage from '@/features/auth/pages/LoginPage'
import ProfilePage from '@/features/auth/pages/ProfilePage'
import NotFound from '@/shared/ui/NotFound'
import CheckoutPage from '@/features/checkout/pages/CheckoutPage'
import OrderDetailsPage from '@/features/orders/pages/OrderDetailsPage'
import OrdersHistoryPage from '@/features/orders/pages/OrdersHistoryPage'
import AdminLayout from '@/features/admin/components/AdminLayout'
import AdminDashboardPage from '@/features/admin/pages/AdminDashboardPage'
import AdminProductsPage from '@/features/admin/pages/AdminProductsPage'
import AdminOrdersPage from '@/features/admin/pages/AdminOrdersPage'
import AdminCategoriesPage from '@/features/admin/pages/AdminCategoriesPage'

export const router = createBrowserRouter([
    {
        path: '/',
        element: <App />,
        children: [
            { index: true, element: <ProductsPage /> },
            { path: 'products/:id', element: <ProductDetailsPage /> },
            { path: 'login', element: <LoginPage /> },
            { path: 'profile', element: <ProfilePage /> },
            { path: '*', element: <NotFound /> },
            { path: 'checkout', element: <CheckoutPage /> },
            { path: 'orders', element: <OrdersHistoryPage /> },      // ✅ история
            { path: 'orders/:id', element: <OrderDetailsPage /> },   // ✅ детали
        ],
    },
    {
        path: '/admin',
        element: <AdminLayout />,
        children: [
            { index: true, element: <AdminDashboardPage /> },
            { path: 'products', element: <AdminProductsPage /> },
            { path: 'orders', element: <AdminOrdersPage /> },
            { path: 'categories', element: <AdminCategoriesPage /> },
            // { path: 'reviews', element: <AdminReviewsPage /> },
            // { path: 'users', element: <AdminUsersPage /> },
        ],
    },
])