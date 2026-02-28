import type { GetServerSideProps } from "next";
import { useState } from "react";
import { getServerSession } from "next-auth";
import { useSession } from "next-auth/react";
import { authOptions } from "@/pages/api/auth/[...nextauth]";
import { apiFetch } from "@/lib/apiFetch";
import Header from "@/components/layout/Header";
import CategoryFormModal from "@/components/categories/CategoryFormModal";
import type { Category } from "@/types/api";

interface CategoriesPageProps {
  initialCategories: Category[];
  error?: string;
}

type ModalState =
  | { type: "none" }
  | { type: "create" }
  | { type: "edit"; category: Category }
  | { type: "delete"; category: Category };

export default function CategoriesPage({
  initialCategories,
  error,
}: CategoriesPageProps) {
  const { data: session } = useSession();
  const [categories, setCategories] = useState<Category[]>(initialCategories);
  const [modal, setModal] = useState<ModalState>({ type: "none" });
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const isAdmin = session?.user.roles.includes("admin");

  function handleCreated(category: Category) {
    setCategories((prev) => [...prev, category]);
    setModal({ type: "none" });
  }

  function handleUpdated(category: Category) {
    setCategories((prev) =>
      prev.map((c) => (c.id === category.id ? category : c))
    );
    setModal({ type: "none" });
  }

  async function handleDelete() {
    if (modal.type !== "delete") return;
    setDeleteLoading(true);
    setDeleteError(null);
    try {
      await apiFetch(`/api/categories/${modal.category.id}`, {
        method: "DELETE",
        accessToken: session?.accessToken,
      });
      setCategories((prev) => prev.filter((c) => c.id !== modal.category.id));
      setModal({ type: "none" });
    } catch (err: unknown) {
      setDeleteError(
        err instanceof Error ? err.message : "Erro ao excluir categoria."
      );
    } finally {
      setDeleteLoading(false);
    }
  }

  return (
    <div className="min-h-screen bg-zinc-50 dark:bg-zinc-950">
      <Header />

      <main className="mx-auto max-w-4xl px-6 py-8">
        <div className="mb-6 flex items-center justify-between">
          <h1 className="text-2xl font-semibold text-zinc-900 dark:text-zinc-50">
            Categorias
          </h1>
          {isAdmin && (
            <button
              onClick={() => setModal({ type: "create" })}
              className="rounded-lg bg-zinc-900 px-4 py-2 text-sm font-medium text-white hover:bg-zinc-700 dark:bg-zinc-50 dark:text-zinc-900 dark:hover:bg-zinc-200"
            >
              + Nova Categoria
            </button>
          )}
        </div>

        {error ? (
          <p className="text-sm text-red-500">{error}</p>
        ) : categories.length === 0 ? (
          <p className="text-sm text-zinc-400">Nenhuma categoria cadastrada.</p>
        ) : (
          <div className="overflow-hidden rounded-xl border border-zinc-200 bg-white shadow-sm dark:border-zinc-700 dark:bg-zinc-800">
            <table className="min-w-full divide-y divide-zinc-100 dark:divide-zinc-700">
              <thead>
                <tr className="bg-zinc-50 dark:bg-zinc-900">
                  <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wide text-zinc-500">
                    Nome
                  </th>
                  <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wide text-zinc-500">
                    Descrição
                  </th>
                  {isAdmin && (
                    <th className="px-6 py-3 text-right text-xs font-semibold uppercase tracking-wide text-zinc-500">
                      Ações
                    </th>
                  )}
                </tr>
              </thead>
              <tbody className="divide-y divide-zinc-100 dark:divide-zinc-700">
                {categories.map((category) => (
                  <tr
                    key={category.id}
                    className="hover:bg-zinc-50 dark:hover:bg-zinc-900/50"
                  >
                    <td className="px-6 py-4 text-sm font-medium text-zinc-900 dark:text-zinc-50">
                      {category.name}
                    </td>
                    <td className="px-6 py-4 text-sm text-zinc-500 dark:text-zinc-400">
                      {category.description || "—"}
                    </td>
                    {isAdmin && (
                      <td className="px-6 py-4 text-right">
                        <div className="flex justify-end gap-2">
                          <button
                            onClick={() =>
                              setModal({ type: "edit", category })
                            }
                            className="rounded-md bg-blue-50 px-3 py-1.5 text-xs font-medium text-blue-700 transition-colors hover:bg-blue-100 dark:bg-blue-900/30 dark:text-blue-400 dark:hover:bg-blue-900/50"
                          >
                            Editar
                          </button>
                          <button
                            onClick={() =>
                              setModal({ type: "delete", category })
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

      {/* Modal: Criar categoria */}
      {modal.type === "create" && (
        <CategoryFormModal
          onClose={() => setModal({ type: "none" })}
          onSuccess={handleCreated}
        />
      )}

      {/* Modal: Editar categoria */}
      {modal.type === "edit" && (
        <CategoryFormModal
          category={modal.category}
          onClose={() => setModal({ type: "none" })}
          onSuccess={handleUpdated}
        />
      )}

      {/* Modal: Confirmar exclusao */}
      {modal.type === "delete" && (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
          onClick={(e) =>
            e.target === e.currentTarget && setModal({ type: "none" })
          }
        >
          <div className="w-full max-w-sm rounded-2xl border border-zinc-200 bg-white p-6 shadow-xl dark:border-zinc-700 dark:bg-zinc-800">
            <h2 className="mb-2 text-base font-semibold text-zinc-900 dark:text-zinc-50">
              Excluir categoria
            </h2>
            <p className="mb-5 text-sm text-zinc-500">
              Tem certeza que deseja excluir{" "}
              <span className="font-medium text-zinc-700 dark:text-zinc-300">
                {modal.category.name}
              </span>
              ? Categorias com produtos associados não podem ser excluídas.
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
    const categories = await apiFetch<Category[]>("/api/categories", {
      accessToken: session.accessToken,
    });

    return { props: { initialCategories: categories } };
  } catch (err: unknown) {
    return {
      props: {
        initialCategories: [],
        error:
          err instanceof Error ? err.message : "Erro ao carregar categorias.",
      },
    };
  }
};
