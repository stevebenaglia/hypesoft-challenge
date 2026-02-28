import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";
import { categoryService } from "@/services/categoryService";
import CategoriesClient from "./CategoriesClient";

export default async function CategoriesPage() {
  const session = await getServerSession(authOptions);

  try {
    const categories = await categoryService.getAll(session!.accessToken);

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
