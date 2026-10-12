import { createContext, useContext, useState, ReactNode } from 'react'

interface CartContextValue {
    isOpen: boolean
    openCart: () => void
    closeCart: () => void
    toggleCart: () => void
}

const CartContext = createContext<CartContextValue | undefined>(undefined)

export function CartProvider({ children }: { children: ReactNode }) {
    const [isOpen, setIsOpen] = useState(false)

    return (
        <CartContext.Provider
            value={{
                isOpen,
                openCart: () => setIsOpen(true),
                closeCart: () => setIsOpen(false),
                toggleCart: () => setIsOpen((v) => !v),
            }}
        >
            {children}
        </CartContext.Provider>
    )
}

export function useCartDrawer() {
    const context = useContext(CartContext)
    if (!context) {
        throw new Error('useCartDrawer must be used within CartProvider')
    }
    return context
}