import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";
import { apiFetch } from "@/lib/apiFetch";
import type { Category } from "@/types/api";
import CategoriesClient from "./CategoriesClient";

export default async function CategoriesPage() {
  const session = await getServerSession(authOptions);

  try {
    const categories = await apiFetch<Category[]>("/api/categories", {
      accessToken: session!.accessToken,
    });

    return <CategoriesClient initialCategories={categories} />;
  } catch (err: unknown) {
    return (
      <CategoriesClient
        initialCategories={[]}
        error={err instanceof Error ? err.message : "Erro ao carregar categorias."}
      />
    );
  }
}
