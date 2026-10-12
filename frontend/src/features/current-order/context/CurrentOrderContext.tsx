import { createContext, useContext, useState, useEffect, ReactNode } from 'react'

export interface CurrentOrderItem {
    productId: number
    productName: string
    productImage?: string
    price: number
    quantity: number
}

interface CurrentOrderContextValue {
    items: CurrentOrderItem[]
    addItem: (item: CurrentOrderItem) => void
    removeItem: (productId: number) => void
    updateQuantity: (productId: number, quantity: number) => void
    clearOrder: () => void
    totalItems: number
    totalPrice: number
    isOpen: boolean
    openDrawer: () => void
    closeDrawer: () => void
    toggleDrawer: () => void
}

const CurrentOrderContext = createContext<CurrentOrderContextValue | undefined>(undefined)

const STORAGE_KEY = 'current_order_items'

export function CurrentOrderProvider({ children }: { children: ReactNode }) {
    // ✅ Загружаем из sessionStorage (исчезает при закрытии браузера)
    const [items, setItems] = useState<CurrentOrderItem[]>(() => {
        try {
            const stored = sessionStorage.getItem(STORAGE_KEY)
            return stored ? JSON.parse(stored) : []
        } catch {
            return []
        }
    })

    const [isOpen, setIsOpen] = useState(false)

    // ✅ Сохраняем в sessionStorage при изменении
    useEffect(() => {
        try {
            sessionStorage.setItem(STORAGE_KEY, JSON.stringify(items))
        } catch {
            // Игнорируем ошибки storage
        }
    }, [items])

    const addItem = (newItem: CurrentOrderItem) => {
        setItems((prev) => {
            const existing = prev.find((i) => i.productId === newItem.productId)
            if (existing) {
                return prev.map((i) =>
                    i.productId === newItem.productId
                        ? { ...i, quantity: i.quantity + newItem.quantity }
                        : i
                )
            }
            return [...prev, newItem]
        })
    }

    const removeItem = (productId: number) => {
        setItems((prev) => prev.filter((i) => i.productId !== productId))
    }

    const updateQuantity = (productId: number, quantity: number) => {
        if (quantity <= 0) {
            removeItem(productId)
            return
        }
        setItems((prev) =>
            prev.map((i) =>
                i.productId === productId ? { ...i, quantity } : i
            )
        )
    }

    const clearOrder = () => {
        setItems([])
        sessionStorage.removeItem(STORAGE_KEY)
    }

    const totalItems = items.reduce((sum, i) => sum + i.quantity, 0)
    const totalPrice = items.reduce((sum, i) => sum + i.price * i.quantity, 0)

    return (
        <CurrentOrderContext.Provider
            value={{
                items,
                addItem,
                removeItem,
                updateQuantity,
                clearOrder,
                totalItems,
                totalPrice,
                isOpen,
                openDrawer: () => setIsOpen(true),
                closeDrawer: () => setIsOpen(false),
                toggleDrawer: () => setIsOpen((v) => !v),
            }}
        >
            {children}
        </CurrentOrderContext.Provider>
    )
}

export function useCurrentOrder() {
    const context = useContext(CurrentOrderContext)
    if (!context) {
        throw new Error('useCurrentOrder must be used within CurrentOrderProvider')
    }
    return context
}