import type { GetServerSideProps } from "next";
import { useState } from "react";
import { getServerSession } from "next-auth";
import { useSession } from "next-auth/react";
import { authOptions } from "@/pages/api/auth/[...nextauth]";
import { apiFetch } from "@/lib/apiFetch";
import Header from "@/components/layout/Header";
import UpdateStockModal from "@/components/stock/UpdateStockModal";
import type { Product, PagedResult } from "@/types/api";

interface ProductsPageProps {
  initialData: PagedResult<Product>;
  error?: string;
}

function stockBadgeClass(qty: number) {
  if (qty === 0) return "bg-red-100 text-red-700";
  if (qty < 10) return "bg-amber-100 text-amber-700";
  return "bg-green-100 text-green-700";
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(value);
}

export default function ProductsPage({ initialData, error }: ProductsPageProps) {
  const { data: session } = useSession();
  const [products, setProducts] = useState<Product[]>(initialData.data);
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null);

  const isAdmin = session?.user.roles.includes("admin");

  function handleStockUpdated(updated: Product) {
    setProducts((prev) => prev.map((p) => (p.id === updated.id ? updated : p)));
    setSelectedProduct(null);
  }

  return (
    <div className="min-h-screen bg-zinc-50 dark:bg-zinc-950">
      <Header />

      <main className="mx-auto max-w-6xl px-6 py-8">
        <div className="mb-6 flex items-center justify-between">
          <h1 className="text-2xl font-semibold text-zinc-900 dark:text-zinc-50">
            Produtos
          </h1>
          <p className="text-sm text-zinc-500">
            {initialData.totalRecords} produto(s) encontrado(s)
          </p>
        </div>

        {error ? (
          <p className="text-sm text-red-500">{error}</p>
        ) : products.length === 0 ? (
          <p className="text-sm text-zinc-400">Nenhum produto cadastrado.</p>
        ) : (
          <div className="overflow-hidden rounded-xl border border-zinc-200 bg-white shadow-sm dark:border-zinc-700 dark:bg-zinc-800">
            <table className="min-w-full divide-y divide-zinc-100 dark:divide-zinc-700">
              <thead>
                <tr className="bg-zinc-50 dark:bg-zinc-900">
                  <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wide text-zinc-500">
                    Produto
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wide text-zinc-500">
                    Categoria
                  </th>
                  <th className="px-6 py-3 text-right text-xs font-semibold uppercase tracking-wide text-zinc-500">
                    Preço
                  </th>
                  <th className="px-6 py-3 text-center text-xs font-semibold uppercase tracking-wide text-zinc-500">
                    Estoque
                  </th>
                  {isAdmin && (
                    <th className="px-6 py-3 text-right text-xs font-semibold uppercase tracking-wide text-zinc-500">
                      Ações
                    </th>
                  )}
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-100 dark:divide-zinc-700">
                {products.map((product) => (
                  <tr
                    key={product.id}
                    className="hover:bg-zinc-50 dark:hover:bg-zinc-900/50"
                  >
                    <td className="px-6 py-4">
                      <p className="text-sm font-medium text-zinc-900 dark:text-zinc-50">
                        {product.name}
                      </p>
                      {product.description && (
                        <p className="mt-0.5 max-w-xs truncate text-xs text-zinc-400">
                          {product.description}
                        </p>
                      )}
                    </td>
                    <td className="px-6 py-4 text-sm text-zinc-600 dark:text-zinc-400">
                      {product.categoryName ?? "—"}
                    </td>
                    <td className="px-6 py-4 text-right text-sm text-zinc-700 dark:text-zinc-300">
                      {formatCurrency(product.price)}
                    </td>
                    <td className="px-6 py-4 text-center">
                      <span
                        className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold ${stockBadgeClass(product.stockQuantity)}`}
                      >
                        {product.stockQuantity} un.
                      </span>
                    </td>
                    {isAdmin && (
                      <td className="px-6 py-4 text-right">
                        <button
                          onClick={() => setSelectedProduct(product)}
                          className="rounded-md bg-zinc-100 px-3 py-1.5 text-xs font-medium text-zinc-700 transition-colors hover:bg-zinc-200 dark:bg-zinc-700 dark:text-zinc-300 dark:hover:bg-zinc-600"
                        >
                          Atualizar Estoque
                        </button>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </main>

      {selectedProduct && (
        <UpdateStockModal
          product={selectedProduct}
          onClose={() => setSelectedProduct(null)}
          onSuccess={handleStockUpdated}
        />
      )}
    </div>
  );
}

export const getServerSideProps: GetServerSideProps = async (context) => {
  const session = await getServerSession(context.req, context.res, authOptions);

  if (!session) {
    return { redirect: { destination: "/auth/signin", permanent: false } };
  }

  try {
    const data = await apiFetch<PagedResult<Product>>(
      "/api/products?pageSize=100",
      { accessToken: session.accessToken }
    );
    return { props: { initialData: data } };
  } catch (err: unknown) {
    return {
      props: {
        initialData: {
          data: [],
          pageNumber: 1,
          pageSize: 100,
          totalPages: 0,
          totalRecords: 0,
        },
        error: err instanceof Error ? err.message : "Erro ao carregar produtos.",
      },
    };
  }
};
