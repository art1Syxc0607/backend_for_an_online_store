import { apiClient } from '@/shared/api/client'
import type { PagedResult } from '@/shared/api/types'

export interface AddProductRequest {
    name: string
    price: number
    purchasePrice: number
    stockQuantity: number
    categoryId?: number
    description: string
    files?: File[]
}

export interface UpdateProductRequest {
    id: number
    name?: string
    price?: number
    purchasePrice?: number
    stockQuantity?: number
    reservedQuantity?: number
    categoryId?: number
    description?: string
    sku?: string
}

// ✅ Добавить товар
export async function addProduct(request: AddProductRequest): Promise<number> {
    const formData = new FormData()
    formData.append('Name', request.name)
    formData.append('Price', request.price.toString())
    formData.append('PurchasePrice', request.purchasePrice.toString())
    formData.append('StockQuantity', request.stockQuantity.toString())
    formData.append('Description', request.description)
    if (request.categoryId) formData.append('CategoryId', request.categoryId.toString())
    if (request.files) {
        request.files.forEach((f) => formData.append('Files', f))
    }

    const { data } = await apiClient.post<number>('/admin/product', formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
    })
    return data
}

// ✅ Обновить товар
export async function updateProduct(request: UpdateProductRequest): Promise<void> {
    await apiClient.put('/admin/product', request)
}

// ✅ Удалить товар
export async function deleteProduct(productId: number): Promise<void> {
    await apiClient.delete(`/admin/product/${productId}`)
}

// ✅ Загрузить файлы
export async function uploadProductFiles(
    productId: number,
    files: File[]
): Promise<void> {
    const formData = new FormData()
    files.forEach((f) => formData.append('files', f))

    await apiClient.post(`/admin/product/${productId}/files`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' },
    })
}

// ✅ Удалить файлы
export async function deleteProductFiles(
    productId: number,
    fileUrls: string[]
): Promise<void> {
    await apiClient.delete(`/admin/product/${productId}/files`, {
        data: { fileUrls },
    })
}

// ✅ Мало на складе
export interface LowStockProductDto {
    id: number
    name: string
    description?: string
    price: number
    purchasePrice: number
    stockQuantity: number
    reservedQuantity: number
    availableQuantity: number
    amountOfRecieved: number
    amountOfPaid: number
    amountOfCanceled: number
    countOfOrdersContainThisProduct: number
    createdAt: string
    categoryName?: string
    imageUrls: string[]
    ordersCount: number
}

export async function getLowStockProducts(
    limit: number,
    includeReserved = true,
    categoryId?: number,
    search?: string
): Promise<LowStockProductDto[]> {
    const { data } = await apiClient.get<LowStockProductDto[]>(
        `/admin/product/low-stock/${limit}`,
        { params: { includeReserved, categoryId, search } }
    )
    return data
}