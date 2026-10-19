import { useEffect, useMemo, useState } from 'react'

export interface MediaChange {
    newFiles: File[]
    removedUrls: string[]
}

interface Props {
    /** Уже сохранённые URL изображений */
    images: string[]
    /** Уже сохранённые URL видео */
    videos: string[]
    /** Файлы, выбранные для загрузки (ещё не сохранённые) */
    newFiles: File[]
    /** URL уже сохранённых файлов, отмеченных на удаление */
    removedUrls: string[]
    onChange: (next: MediaChange) => void
    maxFiles?: number
    disabled?: boolean
}

function isVideoUrl(url: string) {
    return /\.(mp4|webm|ogg|ogv|mov|m4v)(\?.*)?$/i.test(url)
}

// Превью нового файла с корректной очисткой object URL
function NewFilePreview({
    file,
    onRemove,
    disabled,
}: {
    file: File
    onRemove: () => void
    disabled?: boolean
}) {
    const [objectUrl, setObjectUrl] = useState('')

    useEffect(() => {
        const url = URL.createObjectURL(file)
        setObjectUrl(url)
        return () => URL.revokeObjectURL(url)
    }, [file])

    const isVideo = file.type.startsWith('video/')

    return (
        <div className="relative rounded-lg overflow-hidden border-2 border-blue-300">
            {objectUrl &&
                (isVideo ? (
                    <video src={objectUrl} className="w-full h-24 object-cover bg-black" muted />
                ) : (
                    <img src={objectUrl} alt={file.name} className="w-full h-24 object-cover" />
                ))}
            <div className="absolute bottom-0 inset-x-0 bg-black/60 text-white text-[10px] px-1 py-0.5 truncate">
                {file.name}
            </div>
            {!disabled && (
                <button
                    type="button"
                    onClick={onRemove}
                    title="Убрать файл"
                    className="absolute top-1 right-1 w-6 h-6 rounded-full bg-red-500 text-white text-xs leading-none hover:bg-red-600"
                >
                    ×
                </button>
            )}
        </div>
    )
}

/**
 * Редактор медиафайлов товара (фото + видео).
 * Позволяет: добавлять новые файлы и помечать существующие на удаление.
 * Итоговое сохранение (загрузка/удаление) выполняет родитель при submit.
 */
export default function ProductMediaEditor({
    images,
    videos,
    newFiles,
    removedUrls,
    onChange,
    maxFiles = 8,
    disabled,
}: Props) {
    const [error, setError] = useState<string | null>(null)

    const existing = useMemo(
        () => [
            ...images.map((url) => ({ url, kind: 'image' as const, isVideo: isVideoUrl(url) })),
            ...videos.map((url) => ({ url, kind: 'video' as const, isVideo: true })),
        ],
        [images, videos]
    )

    const keptCount = existing.filter((e) => !removedUrls.includes(e.url)).length
    const total = keptCount + newFiles.length

    const toggleRemove = (url: string) => {
        if (disabled) return
        const nextRemoved = removedUrls.includes(url)
            ? removedUrls.filter((u) => u !== url)
            : [...removedUrls, url]
        onChange({ newFiles, removedUrls: nextRemoved })
    }

    const addFiles = (fileList: FileList | null) => {
        if (!fileList) return
        const incoming = Array.from(fileList)
        const slots = Math.max(0, maxFiles - total)

        if (incoming.length > slots) {
            setError(
                slots === 0
                    ? `Достигнут лимит ${maxFiles} файлов.`
                    : `Можно добавить ещё ${slots} файл(ов). Всего не более ${maxFiles}.`
            )
        } else {
            setError(null)
        }

        const accepted = incoming.slice(0, slots)
        if (accepted.length === 0) return

        const merged = [...newFiles]
        for (const f of accepted) {
            const dup = merged.some(
                (x) => x.name === f.name && x.size === f.size && x.lastModified === f.lastModified
            )
            if (!dup) merged.push(f)
        }
        onChange({ newFiles: merged, removedUrls })
    }

    const removeNew = (index: number) => {
        if (disabled) return
        onChange({ newFiles: newFiles.filter((_, i) => i !== index), removedUrls })
    }

    return (
        <div className="space-y-3">
            <div className="flex items-center justify-between">
                <label className="block text-sm font-medium">Медиафайлы (фото/видео)</label>
                <span className={`text-xs ${total >= maxFiles ? 'text-red-500' : 'text-gray-500'}`}>
                    {total} / {maxFiles}
                </span>
            </div>

            {(existing.length > 0 || newFiles.length > 0) && (
                <div className="grid grid-cols-3 sm:grid-cols-4 gap-2">
                    {existing.map((item) => {
                        const isRemoved = removedUrls.includes(item.url)
                        return (
                            <div
                                key={item.url}
                                className={`relative rounded-lg overflow-hidden border ${
                                    isRemoved ? 'border-red-400 opacity-50' : 'border-gray-200'
                                }`}
                            >
                                {item.isVideo ? (
                                    <video
                                        src={item.url}
                                        className="w-full h-24 object-cover bg-black"
                                        muted
                                    />
                                ) : (
                                    <img src={item.url} alt="" className="w-full h-24 object-cover" />
                                )}
                                <button
                                    type="button"
                                    disabled={disabled}
                                    onClick={() => toggleRemove(item.url)}
                                    title={isRemoved ? 'Отменить удаление' : 'Пометить на удаление'}
                                    className={`absolute top-1 right-1 w-6 h-6 rounded-full text-white text-xs leading-none ${
                                        isRemoved ? 'bg-green-600 hover:bg-green-700' : 'bg-red-500 hover:bg-red-600'
                                    }`}
                                >
                                    {isRemoved ? '↺' : '×'}
                                </button>
                                {isRemoved && (
                                    <div className="absolute inset-x-0 bottom-0 bg-red-500/80 text-white text-[10px] text-center py-0.5">
                                        будет удалён
                                    </div>
                                )}
                            </div>
                        )
                    })}

                    {newFiles.map((file, i) => (
                        <NewFilePreview
                            key={`${file.name}-${file.size}-${file.lastModified}-${i}`}
                            file={file}
                            disabled={disabled}
                            onRemove={() => removeNew(i)}
                        />
                    ))}
                </div>
            )}

            {!disabled && (
                <div>
                    <input
                        type="file"
                        multiple
                        accept="image/*,video/*"
                        disabled={total >= maxFiles}
                        onChange={(e) => {
                            addFiles(e.target.files)
                            e.target.value = ''
                        }}
                        className="w-full text-sm"
                    />
                    <p className="text-xs text-gray-400 mt-1">
                        Фото и видео, всего не более {maxFiles} файлов. Файлы с пометкой «×» удалятся при
                        сохранении, «↺» — отменить удаление.
                    </p>
                </div>
            )}

            {error && <p className="text-xs text-red-500">{error}</p>}
        </div>
    )
}
