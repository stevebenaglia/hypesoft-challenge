import type { GetServerSideProps } from "next";
import { useState, useMemo } from "react";
import { getServerSession } from "next-auth";
import { useSession } from "next-auth/react";
import { authOptions } from "@/pages/api/auth/[...nextauth]";
import { apiFetch } from "@/lib/apiFetch";
import Header from "@/components/layout/Header";
import UpdateStockModal from "@/components/stock/UpdateStockModal";
import ProductFormModal from "@/components/products/ProductFormModal";
import type { Product, Category, PagedResult } from "@/types/api";

interface ProductsPageProps {
  initialProducts: Product[];
  categories: Category[];
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

type ModalState =
  | { type: "none" }
  | { type: "create" }
  | { type: "edit"; product: Product }
  | { type: "stock"; product: Product }
  | { type: "delete"; product: Product };

export default function ProductsPage({
  initialProducts,
  categories,
  error,
}: ProductsPageProps) {
  const { data: session } = useSession();
  const [products, setProducts] = useState<Product[]>(initialProducts);
  const [modal, setModal] = useState<ModalState>({ type: "none" });
  const [search, setSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("");
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const isAdmin = session?.user.roles.includes("admin");

  const filtered = useMemo(() => {
    return products.filter((p) => {
      const matchesSearch = p.name
        .toLowerCase()
        .includes(search.toLowerCase());
      const matchesCategory =
        !categoryFilter || p.categoryId === categoryFilter;
      return matchesSearch && matchesCategory;
    });
  }, [products, search, categoryFilter]);

  function handleCreated(product: Product) {
    setProducts((prev) => [product, ...prev]);
    setModal({ type: "none" });
  }

  function handleUpdated(product: Product) {
    setProducts((prev) => prev.map((p) => (p.id === product.id ? product : p)));
    setModal({ type: "none" });
  }

  function handleStockUpdated(product: Product) {
    setProducts((prev) => prev.map((p) => (p.id === product.id ? product : p)));
    setModal({ type: "none" });
  }

  async function handleDelete() {
    if (modal.type !== "delete") return;
    setDeleteLoading(true);
    setDeleteError(null);
    try {
      await apiFetch(`/api/products/${modal.product.id}`, {
        method: "DELETE",
        accessToken: session?.accessToken,
      });
      setProducts((prev) => prev.filter((p) => p.id !== modal.product.id));
      setModal({ type: "none" });
    } catch (err: unknown) {
      setDeleteError(
        err instanceof Error ? err.message : "Erro ao excluir produto."
      );
    } finally {
      setDeleteLoading(false);
    }
  }

  return (
    <div className="min-h-screen bg-zinc-50 dark:bg-zinc-950">
      <Header />

      <main className="mx-auto max-w-6xl px-6 py-8">
        {/* Header da pagina */}
        <div className="mb-6 flex items-center justify-between">
          <h1 className="text-2xl font-semibold text-zinc-900 dark:text-zinc-50">
            Produtos
          </h1>
          {isAdmin && (
            <button
              onClick={() => setModal({ type: "create" })}
              className="rounded-lg bg-zinc-900 px-4 py-2 text-sm font-medium text-white hover:bg-zinc-700 dark:bg-zinc-50 dark:text-zinc-900 dark:hover:bg-zinc-200"
            >
              + Novo Produto
            </button>
          )}
        </div>

        {/* Filtros */}
        <div className="mb-5 flex flex-col gap-3 sm:flex-row">
          <input
            type="text"
            placeholder="Buscar por nome..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm text-zinc-900 focus:outline-none focus:ring-2 focus:ring-zinc-900 dark:border-zinc-600 dark:bg-zinc-800 dark:text-zinc-50 sm:max-w-xs"
          />
          <select
            value={categoryFilter}
            onChange={(e) => setCategoryFilter(e.target.value)}
            className="w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm text-zinc-900 focus:outline-none focus:ring-2 focus:ring-zinc-900 dark:border-zinc-600 dark:bg-zinc-800 dark:text-zinc-50 sm:max-w-xs"
          >
            <option value="">Todas as categorias</option>
            {categories.map((cat) => (
              <option key={cat.id} value={cat.id}>
                {cat.name}
              </option>
            ))}
          </select>
          <p className="self-center text-sm text-zinc-400">
            {filtered.length} produto(s)
          </p>
        </div>

        {error ? (
          <p className="text-sm text-red-500">{error}</p>
        ) : filtered.length === 0 ? (
          <p className="text-sm text-zinc-400">Nenhum produto encontrado.</p>
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
                {filtered.map((product) => (
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
                        <div className="flex justify-end gap-2">
                          <button
                            onClick={() =>
                              setModal({ type: "stock", product })
                            }
                            className="rounded-md bg-zinc-100 px-3 py-1.5 text-xs font-medium text-zinc-700 transition-colors hover:bg-zinc-200 dark:bg-zinc-700 dark:text-zinc-300 dark:hover:bg-zinc-600"
                          >
                            Estoque
                          </button>
                          <button
                            onClick={() =>
                              setModal({ type: "edit", product })
                            }
                            className="rounded-md bg-blue-50 px-3 py-1.5 text-xs font-medium text-blue-700 transition-colors hover:bg-blue-100 dark:bg-blue-900/30 dark:text-blue-400 dark:hover:bg-blue-900/50"
                          >
                            Editar
                          </button>
                          <button
                            onClick={() =>
                              setModal({ type: "delete", product })
                            }
                            className="rounded-md bg-red-50 px-3 py-1.5 text-xs font-medium text-red-700 transition-colors hover:bg-red-100 dark:bg-red-900/30 dark:text-red-400 dark:hover:bg-red-900/50"
                          >
                            Excluir
                          </button>
                        </div>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </main>

      {/* Modal: Criar produto */}
      {modal.type === "create" && (
        <ProductFormModal
          categories={categories}
          onClose={() => setModal({ type: "none" })}
          onSuccess={handleCreated}
        />
      )}

      {/* Modal: Editar produto */}
      {modal.type === "edit" && (
        <ProductFormModal
          product={modal.product}
          categories={categories}
          onClose={() => setModal({ type: "none" })}
          onSuccess={handleUpdated}
        />
      )}

      {/* Modal: Atualizar estoque */}
      {modal.type === "stock" && (
        <UpdateStockModal
          product={modal.product}
          onClose={() => setModal({ type: "none" })}
          onSuccess={handleStockUpdated}
        />
      )}

      {/* Modal: Confirmar exclusao */}
      {modal.type === "delete" && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
          onClick={(e) => e.target === e.currentTarget && setModal({ type: "none" })}
        >
          <div className="w-full max-w-sm rounded-2xl border border-zinc-200 bg-white p-6 shadow-xl dark:border-zinc-700 dark:bg-zinc-800">
            <h2 className="mb-2 text-base font-semibold text-zinc-900 dark:text-zinc-50">
              Excluir produto
            </h2>
            <p className="mb-5 text-sm text-zinc-500">
              Tem certeza que deseja excluir{" "}
              <span className="font-medium text-zinc-700 dark:text-zinc-300">
                {modal.product.name}
              </span>
              ? Esta ação não pode ser desfeita.
            </p>
            {deleteError && (
              <p className="mb-3 text-xs text-red-500">{deleteError}</p>
            )}
            <div className="flex justify-end gap-2">
              <button
                onClick={() => setModal({ type: "none" })}
                className="rounded-lg px-4 py-2 text-sm font-medium text-zinc-600 hover:bg-zinc-100 dark:hover:bg-zinc-700"
              >
                Cancelar
              </button>
              <button
                onClick={handleDelete}
                disabled={deleteLoading}
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-50"
              >
                {deleteLoading ? "Excluindo..." : "Excluir"}
              </button>
            </div>
          </div>
        </div>
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
    const [productsData, categories] = await Promise.all([
      apiFetch<PagedResult<Product>>("/api/products?pageSize=100", {
        accessToken: session.accessToken,
      }),
      apiFetch<Category[]>("/api/categories", {
        accessToken: session.accessToken,
      }),
    ]);

    return {
      props: {
        initialProducts: productsData.data,
        categories,
      },
    };
  } catch (err: unknown) {
    return {
      props: {
        initialProducts: [],
        categories: [],
        error: err instanceof Error ? err.message : "Erro ao carregar dados.",
      },
    };
  }
};
