"use client";

import { useState, useMemo } from "react";
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
import { toast } from "sonner";
import { ChevronsUpDown, ChevronUp, ChevronDown } from "lucide-react";
import type { Category } from "@/types/api";

type SortField = "name" | "productCount";
type SortDir = "asc" | "desc";

function SortIcon({ field, sortField, sortDir }: { field: SortField; sortField: SortField | null; sortDir: SortDir }) {
  if (sortField !== field) return <ChevronsUpDown className="ml-1 inline h-3.5 w-3.5 text-zinc-400" />;
  return sortDir === "asc"
    ? <ChevronUp className="ml-1 inline h-3.5 w-3.5" />
    : <ChevronDown className="ml-1 inline h-3.5 w-3.5" />;
}

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
  const [sortField, setSortField] = useState<SortField | null>(null);
  const [sortDir, setSortDir] = useState<SortDir>("asc");

  const isAdmin = session?.user.roles.includes("admin");

  function toggleSort(field: SortField) {
    if (sortField === field) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortField(field);
      setSortDir("asc");
    }
  }

  const sorted = useMemo(() => {
    if (!sortField) return categories;
    return [...categories].sort((a, b) => {
      const valA = a[sortField];
      const valB = b[sortField];
      const cmp = typeof valA === "string" ? valA.localeCompare(valB as string) : (valA as number) - (valB as number);
      return sortDir === "asc" ? cmp : -cmp;
    });
  }, [categories, sortField, sortDir]);

  const deleteMutation = useDeleteCategory({
    onSuccess: (deletedId) => {
      toast.success("Categoria excluída com sucesso!");
      setCategories((prev) => prev.filter((c) => c.id !== deletedId));
      setModal({ type: "none" });
      router.refresh();
    },
    onError: (error: Error) => {
      toast.error(error.message ?? "Erro ao excluir categoria.");
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
        <div className="overflow-x-auto rounded-xl border border-zinc-200 bg-white shadow-sm dark:border-zinc-700 dark:bg-zinc-800">
          <table className="min-w-full divide-y divide-zinc-100 dark:divide-zinc-700">
            <thead>
              <tr className="bg-zinc-50 dark:bg-zinc-900">
                <th
                  className="cursor-pointer select-none px-6 py-3 text-left text-xs font-semibold uppercase tracking-wide text-zinc-500 hover:text-zinc-900 dark:hover:text-zinc-100"
                  onClick={() => toggleSort("name")}
                >
                  Nome <SortIcon field="name" sortField={sortField} sortDir={sortDir} />
                </th>
                <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wide text-zinc-500">
                  Descrição
                </th>
                <th
                  className="cursor-pointer select-none px-6 py-3 text-center text-xs font-semibold uppercase tracking-wide text-zinc-500 hover:text-zinc-900 dark:hover:text-zinc-100"
                  onClick={() => toggleSort("productCount")}
                >
                  Produtos <SortIcon field="productCount" sortField={sortField} sortDir={sortDir} />
                </th>
                {isAdmin && (
                  <th className="px-6 py-3 text-right text-xs font-semibold uppercase tracking-wide text-zinc-500">
                    Ações
                  </th>
                )}
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-100 dark:divide-zinc-700">
              {sorted.map((category) => (
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
                  <td className="px-6 py-4 text-center text-sm text-zinc-600 dark:text-zinc-400">
                    {category.productCount}
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
                          onClick={() => setModal({ type: "delete", category })}
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
