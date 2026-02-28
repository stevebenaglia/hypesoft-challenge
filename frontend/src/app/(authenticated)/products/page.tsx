import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";
import { productService } from "@/services/productService";
import { categoryService } from "@/services/categoryService";
import ProductsClient from "./ProductsClient";

export default async function ProductsPage() {
  const session = await getServerSession(authOptions);

  try {
    const [productsData, categories] = await Promise.all([
      productService.getAll(100, session!.accessToken),
      categoryService.getAll(session!.accessToken),
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
