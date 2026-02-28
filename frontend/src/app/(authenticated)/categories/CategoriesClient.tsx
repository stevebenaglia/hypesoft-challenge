"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { useSession } from "next-auth/react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import CategoryFormModal from "@/components/forms/CategoryFormModal";
import { useDeleteCategory } from "@/hooks/useCategoryMutations";
import type { Category } from "@/types/api";

interface CategoriesClientProps {
  initialCategories: Category[];
  error?: string;
}

type ModalState =
  | { type: "none" }
  | { type: "create" }
  | { type: "edit"; category: Category }
  | { type: "delete"; category: Category };

export default function CategoriesClient({
  initialCategories,
  error,
}: CategoriesClientProps) {
  const { data: session } = useSession();
  const router = useRouter();
  const [categories, setCategories] = useState<Category[]>(initialCategories);
  const [modal, setModal] = useState<ModalState>({ type: "none" });

  const isAdmin = session?.user.roles.includes("admin");

  const deleteMutation = useDeleteCategory({
    onSuccess: (deletedId) => {
      setCategories((prev) => prev.filter((c) => c.id !== deletedId));
      setModal({ type: "none" });
      router.refresh();
    },
  });

  function handleCreated(category: Category) {
    setCategories((prev) => [...prev, category]);
    setModal({ type: "none" });
    router.refresh();
  }

  function handleUpdated(category: Category) {
    setCategories((prev) =>
      prev.map((c) => (c.id === category.id ? category : c))
    );
    setModal({ type: "none" });
    router.refresh();
  }

  return (
    <main className="mx-auto max-w-4xl px-6 py-8">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-zinc-900 dark:text-zinc-50">
          Categorias
        </h1>
        {isAdmin && (
          <Button onClick={() => setModal({ type: "create" })}>
            + Nova Categoria
          </Button>
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
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setModal({ type: "edit", category })}
                        >
                          Editar
                        </Button>
                        <Button
                          variant="destructive"
                          size="sm"
                          onClick={() =>
                            setModal({ type: "delete", category })
                          }
                        >
                          Excluir
                        </Button>
                      </div>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {modal.type === "create" && (
        <CategoryFormModal
          onClose={() => setModal({ type: "none" })}
          onSuccess={handleCreated}
        />
      )}

      {modal.type === "edit" && (
        <CategoryFormModal
          category={modal.category}
          onClose={() => setModal({ type: "none" })}
          onSuccess={handleUpdated}
        />
      )}

      <Dialog
        open={modal.type === "delete"}
        onOpenChange={(open) => !open && setModal({ type: "none" })}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Excluir categoria</DialogTitle>
            <DialogDescription>
              Tem certeza que deseja excluir{" "}
              <span className="font-medium">
                {modal.type === "delete" ? modal.category.name : ""}
              </span>
              ? Categorias com produtos associados não podem ser excluídas.
            </DialogDescription>
          </DialogHeader>
          {deleteMutation.error && (
            <p className="text-xs text-red-500">
              {deleteMutation.error instanceof Error
                ? deleteMutation.error.message
                : "Erro ao excluir categoria."}
            </p>
          )}
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setModal({ type: "none" })}
            >
              Cancelar
            </Button>
            <Button
              variant="destructive"
              disabled={deleteMutation.isPending}
              onClick={() =>
                modal.type === "delete" &&
                deleteMutation.mutate(modal.category.id)
              }
            >
              {deleteMutation.isPending ? "Excluindo..." : "Excluir"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </main>
  );
}
