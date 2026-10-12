import { useState, useRef, useCallback, useEffect } from 'react'
import AIChatWindow from './AIChatWindow'

type ResizeHandle =
    | 'top' | 'bottom' | 'left' | 'right'
    | 'top-left' | 'top-right' | 'bottom-left' | 'bottom-right'

export default function AIAssistantWidget() {
    const [isOpen, setIsOpen] = useState(false)
    const [position, setPosition] = useState({
        x: window.innerWidth - 440,
        y: window.innerHeight - 600,
    })
    const [size, setSize] = useState({ width: 400, height: 550 })

    const isDraggingRef = useRef(false)
    const dragOffsetRef = useRef({ x: 0, y: 0 })

    // ✅ Resize state
    const activeResizeRef = useRef<ResizeHandle | null>(null)
    const resizeStartRef = useRef({
        x: 0,
        y: 0,
        w: 0,
        h: 0,
        posX: 0,
        posY: 0,
    })

    const MIN_WIDTH = 320
    const MIN_HEIGHT = 400

    // ═══════════════════════════════════════════
    // Drag
    // ═══════════════════════════════════════════
    const handleDragStart = useCallback((e: React.MouseEvent) => {
        const target = e.target as HTMLElement
        if (target.closest('button')) return

        isDraggingRef.current = true
        dragOffsetRef.current = {
            x: e.clientX - position.x,
            y: e.clientY - position.y,
        }
        e.preventDefault()
    }, [position])

    // ═══════════════════════════════════════════
    // Resize
    // ═══════════════════════════════════════════
    const handleResizeStart = useCallback(
        (e: React.MouseEvent, handle: ResizeHandle) => {
            e.preventDefault()
            e.stopPropagation()

            activeResizeRef.current = handle
            resizeStartRef.current = {
                x: e.clientX,
                y: e.clientY,
                w: size.width,
                h: size.height,
                posX: position.x,
                posY: position.y,
            }
        },
        [size, position]
    )

    // ═══════════════════════════════════════════
    // Global mouse move/up
    // ═══════════════════════════════════════════
    useEffect(() => {
        const handleMouseMove = (e: MouseEvent) => {
            // ✅ Drag
            if (isDraggingRef.current) {
                const newX = e.clientX - dragOffsetRef.current.x
                const newY = e.clientY - dragOffsetRef.current.y
                const maxX = window.innerWidth - 100
                const maxY = window.innerHeight - 100

                setPosition({
                    x: Math.max(0, Math.min(newX, maxX)),
                    y: Math.max(0, Math.min(newY, maxY)),
                })
            }

            // ✅ Resize
            if (activeResizeRef.current) {
                const dx = e.clientX - resizeStartRef.current.x
                const dy = e.clientY - resizeStartRef.current.y

                const startW = resizeStartRef.current.w
                const startH = resizeStartRef.current.h
                const startX = resizeStartRef.current.posX
                const startY = resizeStartRef.current.posY

                let newW = startW
                let newH = startH
                let newX = startX
                let newY = startY

                const handle = activeResizeRef.current

                // ✅ Правые стороны — увеличивают ширину
                if (handle.includes('right')) {
                    newW = Math.max(MIN_WIDTH, startW + dx)
                }

                // ✅ Левые стороны — увеличивают ширину + сдвигают X
                if (handle.includes('left')) {
                    const possibleWidth = startW - dx
                    if (possibleWidth >= MIN_WIDTH) {
                        newW = possibleWidth
                        newX = startX + dx
                    } else {
                        newW = MIN_WIDTH
                        newX = startX + startW - MIN_WIDTH
                    }
                }

                // ✅ Нижние стороны — увеличивают высоту
                if (handle.includes('bottom')) {
                    newH = Math.max(MIN_HEIGHT, startH + dy)
                }

                // ✅ Верхние стороны — увеличивают высоту + сдвигают Y
                if (handle.includes('top')) {
                    const possibleHeight = startH - dy
                    if (possibleHeight >= MIN_HEIGHT) {
                        newH = possibleHeight
                        newY = startY + dy
                    } else {
                        newH = MIN_HEIGHT
                        newY = startY + startH - MIN_HEIGHT
                    }
                }

                // ✅ Ограничения окна
                newW = Math.min(newW, window.innerWidth)
                newH = Math.min(newH, window.innerHeight)
                newX = Math.max(0, newX)
                newY = Math.max(0, newY)

                setSize({ width: newW, height: newH })
                setPosition({ x: newX, y: newY })
            }
        }

        const handleMouseUp = () => {
            isDraggingRef.current = false
            activeResizeRef.current = null
        }

        if (isOpen) {
            document.addEventListener('mousemove', handleMouseMove)
            document.addEventListener('mouseup', handleMouseUp)

            return () => {
                document.removeEventListener('mousemove', handleMouseMove)
                document.removeEventListener('mouseup', handleMouseUp)
            }
        }
    }, [isOpen])

    useEffect(() => {
        const handleKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Escape' && isOpen) setIsOpen(false)
        }
        document.addEventListener('keydown', handleKeyDown)
        return () => document.removeEventListener('keydown', handleKeyDown)
    }, [isOpen])

    // ✅ Стили для resize-хэндлов
    const resizeHandleClasses: Record<ResizeHandle, string> = {
        top: 'top-0 left-2 right-2 h-1 cursor-ns-resize',
        bottom: 'bottom-0 left-2 right-2 h-1 cursor-ns-resize',
        left: 'left-0 top-2 bottom-2 w-1 cursor-ew-resize',
        right: 'right-0 top-2 bottom-2 w-1 cursor-ew-resize',
        'top-left': 'top-0 left-0 w-3 h-3 cursor-nwse-resize',
        'top-right': 'top-0 right-0 w-3 h-3 cursor-nesw-resize',
        'bottom-left': 'bottom-0 left-0 w-3 h-3 cursor-nesw-resize',
        'bottom-right': 'bottom-0 right-0 w-3 h-3 cursor-nwse-resize',
    }

    return (
        <>
            {isOpen && (
                <div
                    className="fixed z-[100] bg-white rounded-lg shadow-2xl border border-gray-200 flex flex-col overflow-hidden"
                    style={{
                        left: `${position.x}px`,
                        top: `${position.y}px`,
                        width: `${size.width}px`,
                        height: `${size.height}px`,
                    }}
                >
                    <AIChatWindow
                        onClose={() => setIsOpen(false)}
                        onDragStart={handleDragStart}
                    />

                    {/* ✅ Resize handles — 4 стороны + 4 угла */}
                    {(Object.keys(resizeHandleClasses) as ResizeHandle[]).map((handle) => (
                        <div
                            key={handle}
                            onMouseDown={(e) => handleResizeStart(e, handle)}
                            className={`absolute z-10 group ${resizeHandleClasses[handle]}`}
                        >
                            {/* Визуальные индикаторы для углов */}
                            {handle === 'bottom-right' && (
                                <svg
                                    viewBox="0 0 16 16"
                                    className="absolute bottom-0 right-0 w-4 h-4 text-gray-400 group-hover:text-blue-500 pointer-events-none"
                                    fill="currentColor"
                                >
                                    <path d="M16 16h-6v-1h5v-5h1v6zM16 10h-1V6h-4V5h5v5zM16 4h-1V1h-3V0h4v4z" />
                                </svg>
                            )}
                            {handle === 'bottom-left' && (
                                <svg
                                    viewBox="0 0 16 16"
                                    className="absolute bottom-0 left-0 w-4 h-4 text-gray-400 group-hover:text-blue-500 pointer-events-none"
                                    fill="currentColor"
                                    style={{ transform: 'scaleX(-1)' }}
                                >
                                    <path d="M16 16h-6v-1h5v-5h1v6zM16 10h-1V6h-4V5h5v5zM16 4h-1V1h-3V0h4v4z" />
                                </svg>
                            )}
                        </div>
                    ))}
                </div>
            )}

            <button
                onClick={() => setIsOpen((v) => !v)}
                className={`
                    fixed bottom-6 right-6 z-[110]
                    flex items-center gap-2
                    px-5 py-3 rounded-full
                    text-white font-medium
                    shadow-lg hover:shadow-xl
                    transition-all duration-300
                    ${isOpen
                        ? 'bg-red-500 hover:bg-red-600'
                        : 'bg-gradient-to-r from-blue-500 to-purple-600 hover:from-blue-600 hover:to-purple-700'
                    }
                `}
                title={isOpen ? 'Закрыть ассистента' : 'Открыть ассистента'}
            >
                {isOpen ? (
                    <>
                        <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                        </svg>
                        <span className="hidden sm:inline">Закрыть</span>
                    </>
                ) : (
                    <>
                        <span className="text-xl">🤖</span>
                        <span className="hidden sm:inline">Ассистент</span>
                    </>
                )}
            </button>
        </>
    )
}