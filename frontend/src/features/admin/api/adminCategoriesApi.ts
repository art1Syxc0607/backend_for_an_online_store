import { apiClient } from '@/shared/api/client'

export interface AddCategoryRequest {
    name: string
    description?: string
}

export interface UpdateCategoryRequest {
    id: number
    name?: string
    description?: string
}

export async function addCategory(request: AddCategoryRequest): Promise<number> {
    const { data } = await apiClient.post<number>('/admin/category', {
        name: request.name,
        description: request.description,
        createdAt: new Date().toISOString(),
    })
    return data
}

export async function updateCategory(request: UpdateCategoryRequest): Promise<void> {
    await apiClient.put('/admin/category', request)
}

export async function deleteCategory(categoryId: number): Promise<void> {
    await apiClient.delete(`/admin/category/${categoryId}`)
}