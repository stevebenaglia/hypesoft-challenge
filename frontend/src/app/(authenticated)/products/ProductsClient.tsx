"use client";

import { useState, useMemo } from "react";
import { useRouter } from "next/navigation";
import { useSession } from "next-auth/react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import ProductFormModal from "@/components/forms/ProductFormModal";
import UpdateStockModal from "@/components/forms/UpdateStockModal";
import { useDeleteProduct } from "@/hooks/useProductMutations";
import { formatCurrency, stockBadgeVariant } from "@/utils/formatters";
import type { Product, Category } from "@/types/api";

interface ProductsClientProps {
  initialProducts: Product[];
  categories: Category[];
  error?: string;
}

type ModalState =
  | { type: "none" }
  | { type: "create" }
  | { type: "edit"; product: Product }
  | { type: "stock"; product: Product }
  | { type: "delete"; product: Product };

export default function ProductsClient({
  initialProducts,
  categories,
  error,
}: ProductsClientProps) {
  const { data: session } = useSession();
  const router = useRouter();
  const [products, setProducts] = useState<Product[]>(initialProducts);
  const [modal, setModal] = useState<ModalState>({ type: "none" });
  const [search, setSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("all");

  const isAdmin = session?.user.roles.includes("admin");

  const filtered = useMemo(() => {
    return products.filter((p) => {
      const matchesSearch = p.name.toLowerCase().includes(search.toLowerCase());
      const matchesCategory =
        categoryFilter === "all" || p.categoryId === categoryFilter;
      return matchesSearch && matchesCategory;
    });
  }, [products, search, categoryFilter]);

  const deleteMutation = useDeleteProduct({
    onSuccess: (deletedId) => {
      setProducts((prev) => prev.filter((p) => p.id !== deletedId));
      setModal({ type: "none" });
      router.refresh();
    },
  });

  function handleCreated(product: Product) {
    setProducts((prev) => [product, ...prev]);
    setModal({ type: "none" });
    router.refresh();
  }

  function handleUpdated(product: Product) {
    setProducts((prev) => prev.map((p) => (p.id === product.id ? product : p)));
    setModal({ type: "none" });
    router.refresh();
  }

  function handleStockUpdated(product: Product) {
    setProducts((prev) => prev.map((p) => (p.id === product.id ? product : p)));
    setModal({ type: "none" });
    router.refresh();
  }

  return (
    <main className="mx-auto max-w-6xl px-6 py-8">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-zinc-900 dark:text-zinc-50">
          Produtos
        </h1>
        {isAdmin && (
          <Button onClick={() => setModal({ type: "create" })}>
            + Novo Produto
          </Button>
        )}
      </div>

      {/* Filtros */}
      <div className="mb-5 flex flex-col gap-3 sm:flex-row">
        <Input
          placeholder="Buscar por nome..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="sm:max-w-xs"
        />
        <Select value={categoryFilter} onValueChange={setCategoryFilter}>
          <SelectTrigger className="sm:max-w-xs">
            <SelectValue placeholder="Todas as categorias" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Todas as categorias</SelectItem>
            {categories.map((cat) => (
              <SelectItem key={cat.id} value={cat.id}>
                {cat.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
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
                    <Badge variant={stockBadgeVariant(product.stockQuantity)}>
                      {product.stockQuantity} un.
                    </Badge>
                  </td>
                  {isAdmin && (
                    <td className="px-6 py-4 text-right">
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setModal({ type: "stock", product })}
                        >
                          Estoque
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setModal({ type: "edit", product })}
                        >
                          Editar
                        </Button>
                        <Button
                          variant="destructive"
                          size="sm"
                          onClick={() => setModal({ type: "delete", product })}
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
        <ProductFormModal
          categories={categories}
          onClose={() => setModal({ type: "none" })}
          onSuccess={handleCreated}
        />
      )}

      {modal.type === "edit" && (
        <ProductFormModal
          product={modal.product}
          categories={categories}
          onClose={() => setModal({ type: "none" })}
          onSuccess={handleUpdated}
        />
      )}

      {modal.type === "stock" && (
        <UpdateStockModal
          product={modal.product}
          onClose={() => setModal({ type: "none" })}
          onSuccess={handleStockUpdated}
        />
      )}

      <Dialog
        open={modal.type === "delete"}
        onOpenChange={(open) => !open && setModal({ type: "none" })}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Excluir produto</DialogTitle>
            <DialogDescription>
              Tem certeza que deseja excluir{" "}
              <span className="font-medium">
                {modal.type === "delete" ? modal.product.name : ""}
              </span>
              ? Esta ação não pode ser desfeita.
            </DialogDescription>
          </DialogHeader>
          {deleteMutation.error && (
            <p className="text-xs text-red-500">
              {deleteMutation.error instanceof Error
                ? deleteMutation.error.message
                : "Erro ao excluir produto."}
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
                deleteMutation.mutate(modal.product.id)
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
