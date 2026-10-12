import { useState } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { addReview } from '../api/reviewApi'
import { useAuth } from '@/features/auth/hooks/useAuth'
import StarRating from '@/shared/ui/StarRating'

interface Props {
    productId: number
    onSuccess?: () => void
}

export default function AddReviewForm({ productId, onSuccess }: Props) {
    const { isAuthenticated } = useAuth()
    const queryClient = useQueryClient()

    const [rating, setRating] = useState(5)
    const [text, setText] = useState('')
    const [files, setFiles] = useState<File[]>([])
    const [errorMessage, setErrorMessage] = useState<string | null>(null)
    const [successMessage, setSuccessMessage] = useState<string | null>(null)

    const mutation = useMutation({
        mutationFn: () => addReview(productId, text, rating, files),
        onSuccess: () => {
            setSuccessMessage('Отзыв успешно добавлен!')
            setErrorMessage(null)
            setText('')
            setRating(5)
            setFiles([])

            // ✅ Обновляем список отзывов
            queryClient.invalidateQueries({ queryKey: ['reviews', productId] })

            setTimeout(() => {
                setSuccessMessage(null)
                onSuccess?.()
            }, 2000)
        },
        onError: (error: any) => {
            // ✅ Обработка ошибок
            const backendError = error?.response?.data?.error
                ?? error?.response?.data?.message
                ?? 'Ошибка при добавлении отзыва'

            // ✅ Специальная обработка "пользователь не получил товар"
            if (
                backendError.includes("didn't recieve") ||
                backendError.includes('не получили') ||
                backendError.includes('received')
            ) {
                setErrorMessage(
                    '❌ Вы не можете оставить отзыв на этот товар, ' +
                    'так как ещё не получили его. ' +
                    'Отзыв можно оставить только после подтверждения получения.'
                )
            } else {
                setErrorMessage(backendError)
            }
            setSuccessMessage(null)
        },
    })

    const handleFilesChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        if (e.target.files) {
            const newFiles = Array.from(e.target.files)
            if (newFiles.length > 8) {
                setErrorMessage('Максимум 8 файлов')
                return
            }
            setFiles(newFiles)
            setErrorMessage(null)
        }
    }

    if (!isAuthenticated) {
        return (
            <div className="bg-gray-50 rounded-lg p-4 text-center">
                <p className="text-gray-600">
                    <a href="/login" className="text-blue-600 hover:underline">
                        Войдите
                    </a>, чтобы оставить отзыв
                </p>
            </div>
        )
    }

    return (
        <div className="bg-white rounded-lg shadow p-6 mb-6">
            <h3 className="font-bold text-lg mb-4">✍️ Оставить отзыв</h3>

            {/* Сообщение об успехе */}
            {successMessage && (
                <div className="mb-4 p-3 bg-green-100 border border-green-400 text-green-700 rounded">
                    ✅ {successMessage}
                </div>
            )}

            {/* Сообщение об ошибке */}
            {errorMessage && (
                <div className="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded">
                    {errorMessage}
                </div>
            )}

            {/* Рейтинг */}
            <div className="mb-4">
                <label className="block text-sm font-medium mb-2">Ваша оценка</label>
                <div className="flex gap-1">
                    {[1, 2, 3, 4, 5].map((star) => (
                        <button
                            key={star}
                            type="button"
                            onClick={() => setRating(star)}
                            className={`text-3xl transition-colors ${star <= rating ? 'text-yellow-400' : 'text-gray-300'
                                } hover:text-yellow-500`}
                        >
                            ★
                        </button>
                    ))}
                    <span className="ml-2 self-center text-gray-600">{rating} / 5</span>
                </div>
            </div>

            {/* Текст */}
            <div className="mb-4">
                <label className="block text-sm font-medium mb-2">Отзыв</label>
                <textarea
                    value={text}
                    onChange={(e) => setText(e.target.value)}
                    placeholder="Расскажите о товаре..."
                    rows={4}
                    minLength={3}
                    maxLength={2000}
                    className="w-full px-4 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none"
                />
                <div className="text-xs text-gray-500 text-right mt-1">
                    {text.length} / 2000
                </div>
            </div>

            {/* Файлы */}
            <div className="mb-4">
                <label className="block text-sm font-medium mb-2">
                    Фото / Видео (макс. 8)
                </label>
                <input
                    type="file"
                    multiple
                    accept="image/*,video/*"
                    onChange={handleFilesChange}
                    className="w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-medium file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
                />
                {files.length > 0 && (
                    <div className="mt-2 text-xs text-gray-600">
                        Выбрано: {files.length} файл(ов)
                    </div>
                )}
            </div>

            {/* Кнопка */}
            <button
                onClick={() => mutation.mutate()}
                disabled={mutation.isPending || text.length < 3}
                className="w-full px-6 py-3 bg-blue-500 text-white rounded-lg hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
            >
                {mutation.isPending ? 'Отправка...' : 'Отправить отзыв'}
            </button>
        </div>
    )
}