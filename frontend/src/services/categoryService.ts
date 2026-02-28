import { apiFetch } from "@/lib/apiFetch";
import type { Category } from "@/types/api";

export interface CreateCategoryData {
  name: string;
  description?: string;
}

export type UpdateCategoryData = CreateCategoryData;

export const categoryService = {
  getAll: (accessToken?: string) =>
    apiFetch<Category[]>("/api/categories", { accessToken }),

  getById: (id: string, accessToken?: string) =>
    apiFetch<Category>(`/api/categories/${id}`, { accessToken }),

  create: (data: CreateCategoryData, accessToken?: string) =>
    apiFetch<Category>("/api/categories", {
      method: "POST",
      accessToken,
      body: JSON.stringify(data),
    }),

  update: (id: string, data: UpdateCategoryData, accessToken?: string) =>
    apiFetch<Category>(`/api/categories/${id}`, {
      method: "PUT",
      accessToken,
      body: JSON.stringify(data),
    }),

  delete: (id: string, accessToken?: string) =>
    apiFetch<void>(`/api/categories/${id}`, {
      method: "DELETE",
      accessToken,
    }),
};
