import { apiFetch } from "@/lib/apiFetch";
import type { Product, PagedResult } from "@/types/api";

export interface CreateProductData {
  name: string;
  description?: string;
  price: number;
  stockQuantity: number;
  categoryId: string;
}

export type UpdateProductData = CreateProductData;

export const productService = {
  getAll: (pageSize = 100, accessToken?: string) =>
    apiFetch<PagedResult<Product>>(`/api/products?pageSize=${pageSize}`, {
      accessToken,
    }),

  getById: (id: string, accessToken?: string) =>
    apiFetch<Product>(`/api/products/${id}`, { accessToken }),

  create: (data: CreateProductData, accessToken?: string) =>
    apiFetch<Product>("/api/products", {
      method: "POST",
      accessToken,
      body: JSON.stringify(data),
    }),

  update: (id: string, data: UpdateProductData, accessToken?: string) =>
    apiFetch<Product>(`/api/products/${id}`, {
      method: "PUT",
      accessToken,
      body: JSON.stringify(data),
    }),

  delete: (id: string, accessToken?: string) =>
    apiFetch<void>(`/api/products/${id}`, {
      method: "DELETE",
      accessToken,
    }),

  updateStock: (id: string, quantity: number, accessToken?: string) =>
    apiFetch<Product>(`/api/products/${id}/stock`, {
      method: "PATCH",
      accessToken,
      body: JSON.stringify({ quantity }),
    }),
};
