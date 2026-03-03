import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";
import { categoryService } from "@/services/categoryService";
import ProductsClient from "./ProductsClient";

export default async function ProductsPage() {
  const session = await getServerSession(authOptions);

  try {
    const categories = await categoryService.getAll(session!.accessToken);
    return <ProductsClient initialCategories={categories} />;
  } catch (err: unknown) {
    return (
      <ProductsClient
        initialCategories={[]}
        error={err instanceof Error ? err.message : "Erro ao carregar categorias."}
      />
    );
  }
}
