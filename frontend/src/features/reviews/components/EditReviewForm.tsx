import { useState } from 'react'
import { useQueryClient } from '@tanstack/react-query'
import {
    deleteReviewFiles,
    editReview,
    uploadReviewFiles,
} from '../api/reviewApi'
import type { ReviewDto } from '../api/reviewApi'

const MAX_FILES = 8

interface Props {
    review: ReviewDto
    onCancel: () => void
    onSaved: () => void
}

export default function EditReviewForm({ review, onCancel, onSaved }: Props) {
    const queryClient = useQueryClient()

    const [text, setText] = useState(review.text)
    const [rating, setRating] = useState(review.rating)
    const [existingImages, setExistingImages] = useState(review.imagesUrls ?? [])
    const [existingVideos, setExistingVideos] = useState(review.videosUrls ?? [])
    const [removedUrls, setRemovedUrls] = useState<string[]>([])
    const [newFiles, setNewFiles] = useState<File[]>([])
    const [error, setError] = useState<string | null>(null)
    const [isSaving, setIsSaving] = useState(false)

    const totalFiles = existingImages.length + existingVideos.length + newFiles.length

    const handleFilesChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const selected = Array.from(e.target.files ?? [])
        e.target.value = ''

        if (selected.length === 0) return

        if (totalFiles + selected.length > MAX_FILES) {
            setError(`Максимум ${MAX_FILES} файлов. Сейчас: ${totalFiles}`)
            return
        }

        setNewFiles((prev) => [...prev, ...selected])
        setError(null)
    }

    const handleRemoveExisting = (url: string) => {
        setRemovedUrls((prev) => (prev.includes(url) ? prev : [...prev, url]))
        setExistingImages((prev) => prev.filter((item) => item !== url))
        setExistingVideos((prev) => prev.filter((item) => item !== url))
    }

    const handleRemoveNewFile = (index: number) => {
        setNewFiles((prev) => prev.filter((_, i) => i !== index))
    }

    const handleSave = async () => {
        const trimmedText = text.trim()

        if (trimmedText.length < 3) {
            setError('Текст отзыва должен содержать минимум 3 символа')
            return
        }

        if (trimmedText.length > 2000) {
            setError('Текст отзыва не должен превышать 2000 символов')
            return
        }

        setIsSaving(true)
        setError(null)

        try {
            await editReview(review.id, trimmedText, rating)

            if (removedUrls.length > 0) {
                await deleteReviewFiles(review.id, removedUrls)
            }

            if (newFiles.length > 0) {
                await uploadReviewFiles(review.id, newFiles)
            }

            await queryClient.invalidateQueries({
                queryKey: ['reviews', review.productId],
            })
            queryClient.invalidateQueries({
                queryKey: ['product', review.productId],
            })

            onSaved()
        } catch (err: any) {
            const backendError = err?.response?.data?.error
                ?? err?.response?.data?.message
                ?? 'Не удалось сохранить изменения'
            setError(backendError)
        } finally {
            setIsSaving(false)
        }
    }

    return (
        <div className="mt-3 rounded-lg border border-blue-200 bg-blue-50/50 p-4">
            <h4 className="font-bold text-gray-800 mb-3">Редактирование отзыва</h4>

            {error && (
                <div className="mb-3 p-2 bg-red-100 border border-red-300 text-red-700 rounded text-sm">
                    {error}
                </div>
            )}

            {/* Рейтинг */}
            <div className="mb-3">
                <label className="block text-sm font-medium mb-1">Оценка</label>
                <div className="flex items-center gap-1">
                    {[1, 2, 3, 4, 5].map((star) => (
                        <button
                            key={star}
                            type="button"
                            onClick={() => setRating(star)}
                            disabled={isSaving}
                            className={`text-2xl transition-colors disabled:opacity-50 ${
                                star <= rating ? 'text-yellow-400' : 'text-gray-300'
                            } hover:text-yellow-500`}
                        >
                            ★
                        </button>
                    ))}
                    <span className="ml-2 text-sm text-gray-600">{rating} / 5</span>
                </div>
            </div>

            {/* Текст */}
            <div className="mb-3">
                <label className="block text-sm font-medium mb-1">Текст отзыва</label>
                <textarea
                    value={text}
                    onChange={(e) => setText(e.target.value)}
                    rows={4}
                    maxLength={2000}
                    disabled={isSaving}
                    className="w-full px-3 py-2 border rounded-lg bg-white focus:outline-none focus:ring-2 focus:ring-blue-500 resize-none disabled:opacity-60"
                />
                <div className="text-xs text-gray-500 text-right mt-1">
                    {text.length} / 2000
                </div>
            </div>

            {/* Существующие фото и видео */}
            {(existingImages.length > 0 || existingVideos.length > 0) && (
                <div className="mb-3">
                    <label className="block text-sm font-medium mb-2">
                        Текущие файлы
                    </label>
                    <div className="flex gap-2 flex-wrap">
                        {existingImages.map((url) => (
                            <div key={url} className="relative">
                                <img
                                    src={url}
                                    alt="Фото отзыва"
                                    className="w-20 h-20 object-cover rounded border"
                                />
                                <button
                                    type="button"
                                    onClick={() => handleRemoveExisting(url)}
                                    disabled={isSaving}
                                    title="Удалить фото"
                                    className="absolute -top-2 -right-2 w-6 h-6 rounded-full bg-red-500 text-white text-xs leading-none hover:bg-red-600 disabled:opacity-50"
                                >
                                    ✕
                                </button>
                            </div>
                        ))}

                        {existingVideos.map((url) => (
                            <div key={url} className="relative">
                                <video
                                    src={url}
                                    className="w-28 h-20 object-cover rounded border bg-gray-900"
                                />
                                <button
                                    type="button"
                                    onClick={() => handleRemoveExisting(url)}
                                    disabled={isSaving}
                                    title="Удалить видео"
                                    className="absolute -top-2 -right-2 w-6 h-6 rounded-full bg-red-500 text-white text-xs leading-none hover:bg-red-600 disabled:opacity-50"
                                >
                                    ✕
                                </button>
                            </div>
                        ))}
                    </div>
                </div>
            )}

            {/* Новые файлы */}
            <div className="mb-3">
                <label className="block text-sm font-medium mb-2">
                    Добавить фото / видео
                </label>
                <input
                    type="file"
                    multiple
                    accept="image/*,video/*"
                    onChange={handleFilesChange}
                    disabled={isSaving || totalFiles >= MAX_FILES}
                    className="w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-medium file:bg-blue-100 file:text-blue-700 hover:file:bg-blue-200 disabled:opacity-50"
                />
                <div className="text-xs text-gray-500 mt-1">
                    Файлов: {totalFiles} / {MAX_FILES}
                </div>

                {newFiles.length > 0 && (
                    <div className="mt-2 flex gap-2 flex-wrap">
                        {newFiles.map((file, index) => (
                            <span
                                key={`${file.name}-${file.lastModified}-${index}`}
                                className="inline-flex items-center gap-1 rounded-full bg-white border px-2 py-1 text-xs text-gray-700"
                            >
                                {file.name}
                                <button
                                    type="button"
                                    onClick={() => handleRemoveNewFile(index)}
                                    disabled={isSaving}
                                    title="Убрать файл"
                                    className="text-red-500 hover:text-red-700 disabled:opacity-50"
                                >
                                    ✕
                                </button>
                            </span>
                        ))}
                    </div>
                )}
            </div>

            {/* Кнопки */}
            <div className="flex gap-2">
                <button
                    type="button"
                    onClick={handleSave}
                    disabled={isSaving || text.trim().length < 3}
                    className="px-4 py-2 bg-blue-500 text-white rounded-lg text-sm hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    {isSaving ? 'Сохранение...' : 'Сохранить'}
                </button>
                <button
                    type="button"
                    onClick={onCancel}
                    disabled={isSaving}
                    className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg text-sm hover:bg-gray-300 disabled:opacity-50"
                >
                    Отмена
                </button>
            </div>
        </div>
    )
}
