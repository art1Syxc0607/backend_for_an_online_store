import type { ReactNode } from 'react'

export interface Column<T> {
    key: string
    title: string
    render?: (item: T) => ReactNode
    width?: string
    align?: 'left' | 'center' | 'right'
}

interface Props<T> {
    columns: Column<T>[]
    data: T[]
    isLoading?: boolean
    emptyMessage?: string
    keyExtractor: (item: T) => string | number
}

export default function AdminTable<T>({
    columns,
    data,
    isLoading,
    emptyMessage = 'No data',
    keyExtractor,
}: Props<T>) {
    if (isLoading) {
        return (
            <div className="bg-white rounded-lg shadow p-8 text-center text-gray-500">
                Loading...
            </div>
        )
    }

    if (data.length === 0) {
        return (
            <div className="bg-white rounded-lg shadow p-8 text-center text-gray-500">
                {emptyMessage}
            </div>
        )
    }

    return (
        <div className="bg-white rounded-lg shadow overflow-hidden">
            <div className="overflow-x-auto">
                <table className="w-full">
                    <thead className="bg-gray-50 border-b">
                        <tr>
                            {columns.map((col) => (
                                <th
                                    key={col.key}
                                    className={`px-4 py-3 text-xs font-medium text-gray-500 uppercase tracking-wider ${
                                        col.align === 'right' ? 'text-right' :
                                        col.align === 'center' ? 'text-center' : 'text-left'
                                    }`}
                                    style={{ width: col.width }}
                                >
                                    {col.title}
                                </th>
                            ))}
                        </tr>
                    </thead>
                    <tbody className="divide-y divide-gray-200">
                        {data.map((item) => (
                            <tr key={keyExtractor(item)} className="hover:bg-gray-50">
                                {columns.map((col) => (
                                    <td
                                        key={col.key}
                                        className={`px-4 py-3 text-sm ${
                                            col.align === 'right' ? 'text-right' :
                                            col.align === 'center' ? 'text-center' : 'text-left'
                                        }`}
                                    >
                                        {col.render
                                            ? col.render(item)
                                            : (item as any)[col.key]}
                                    </td>
                                ))}
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    )
}