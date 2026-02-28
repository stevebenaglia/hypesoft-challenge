import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";
import { apiFetch } from "@/lib/apiFetch";
import type { Product, Category, PagedResult } from "@/types/api";
import ProductsClient from "./ProductsClient";

export default async function ProductsPage() {
  const session = await getServerSession(authOptions);

  try {
    const [productsData, categories] = await Promise.all([
      apiFetch<PagedResult<Product>>("/api/products?pageSize=100", {
        accessToken: session!.accessToken,
      }),
      apiFetch<Category[]>("/api/categories", {
        accessToken: session!.accessToken,
      }),
    ]);

    return (
      <ProductsClient
        initialProducts={productsData.data}
        categories={categories}
      />
    );
  } catch (err: unknown) {
    return (
      <ProductsClient
        initialProducts={[]}
        categories={[]}
        error={err instanceof Error ? err.message : "Erro ao carregar dados."}
      />
    );
  }
}
