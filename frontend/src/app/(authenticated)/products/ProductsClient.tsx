"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useSession } from "next-auth/react";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useTranslations } from "next-intl";
import { exportProductsPdf } from "@/utils/pdfExport";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { AlertTriangle, ChevronLeft, ChevronRight, ChevronsUpDown, ChevronUp, ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";
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
import { productService } from "@/services/productService";
import { formatCurrency, stockBadgeVariant } from "@/utils/formatters";
import { toast } from "sonner";
import type { Product, Category } from "@/types/api";

const PAGE_SIZE = 10;

type SortField = "name" | "price" | "stockQuantity";
type SortDir = "asc" | "desc";

function SortIcon({ field, sortField, sortDir }: { field: SortField; sortField: SortField | null; sortDir: SortDir }) {
  if (sortField !== field) return <ChevronsUpDown className="ml-1 inline h-3.5 w-3.5 text-zinc-400" />;
  return sortDir === "asc"
    ? <ChevronUp className="ml-1 inline h-3.5 w-3.5" />
    : <ChevronDown className="ml-1 inline h-3.5 w-3.5" />;
}

interface ProductsClientProps {
  initialCategories: Category[];
  error?: string;
}

type ModalState =
  | { type: "none" }
  | { type: "create" }
  | { type: "edit"; product: Product }
  | { type: "stock"; product: Product }
  | { type: "delete"; product: Product };

export default function ProductsClient({
  initialCategories,
  error,
}: ProductsClientProps) {
  const { data: session } = useSession();
  const router = useRouter();
  const queryClient = useQueryClient();
  const t = useTranslations("products");

  const [modal, setModal] = useState<ModalState>({ type: "none" });
  const [search, setSearch] = useState("");
  const [debouncedSearch, setDebouncedSearch] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("all");
  const [stockFilter, setStockFilter] = useState("all");
  const [pageNumber, setPageNumber] = useState(1);
  const [sortField, setSortField] = useState<SortField | null>(null);
  const [sortDir, setSortDir] = useState<SortDir>("asc");

  // Debounce search input by 400 ms
  useEffect(() => {
    const timer = setTimeout(() => setDebouncedSearch(search), 400);
    return () => clearTimeout(timer);
  }, [search]);

  // Reset to page 1 whenever filters change
  useEffect(() => {
    setPageNumber(1);
  }, [debouncedSearch, categoryFilter, stockFilter]);

  const queryKey = ["products", pageNumber, PAGE_SIZE, debouncedSearch, categoryFilter, stockFilter] as const;

  const { data, isLoading, isError } = useQuery({
    queryKey,
    queryFn: () =>
      productService.getAll(
        {
          pageNumber,
          pageSize: PAGE_SIZE,
          searchTerm: debouncedSearch || undefined,
          categoryId: categoryFilter !== "all" ? categoryFilter : undefined,
          lowStockOnly: stockFilter === "low",
        },
        session?.accessToken,
      ),
    enabled: !!session?.accessToken,
    placeholderData: (prev) => prev,
  });

  function toggleSort(field: SortField) {
    if (sortField === field) {
      setSortDir((d) => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortField(field);
      setSortDir("asc");
    }
  }

  const rawProducts = data?.data ?? [];
  const products = sortField
    ? [...rawProducts].sort((a, b) => {
        const valA = a[sortField];
        const valB = b[sortField];
        const cmp = typeof valA === "string" ? valA.localeCompare(valB as string) : (valA as number) - (valB as number);
        return sortDir === "asc" ? cmp : -cmp;
      })
    : rawProducts;
  const totalPages = data?.totalPages ?? 1;
  const totalRecords = data?.totalRecords ?? 0;

  const isAdmin = session?.user.roles.includes("admin");

  function invalidateProducts() {
    queryClient.invalidateQueries({ queryKey: ["products"] });
    router.refresh();
  }

  const deleteMutation = useDeleteProduct({
    onSuccess: () => {
      toast.success(t("toast.deleted"));
      setModal({ type: "none" });
      invalidateProducts();
    },
    onError: (err: Error) => {
      toast.error(err.message ?? t("toast.deleteError"));
    },
  });

  function handleCreated() {
    setModal({ type: "none" });
    invalidateProducts();
  }

  function handleUpdated() {
    setModal({ type: "none" });
    invalidateProducts();
  }

  function handleStockUpdated() {
    setModal({ type: "none" });
    invalidateProducts();
  }

  return (
    <main className="mx-auto max-w-6xl px-6 py-8">
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-semibold text-zinc-900 dark:text-zinc-50">
          {t("title")}
        </h1>
        <div className="flex items-center gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() =>
              exportProductsPdf(products, {
                search: debouncedSearch || undefined,
                category: categoryFilter !== "all" ? categoryFilter : undefined,
                stockFilter: stockFilter !== "all" ? stockFilter : undefined,
              })
            }
            disabled={products.length === 0}
          >
            Exportar PDF
          </Button>
          {isAdmin && (
            <Button onClick={() => setModal({ type: "create" })}>
              {t("newProduct")}
            </Button>
          )}
        </div>
      </div>

      {/* Filtros */}
      <div className="mb-5 flex flex-col gap-3 sm:flex-row sm:flex-wrap">
        <Input
          placeholder={t("searchPlaceholder")}
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          className="sm:max-w-xs"
        />
        <Select value={categoryFilter} onValueChange={setCategoryFilter}>
          <SelectTrigger className="sm:max-w-xs">
            <SelectValue placeholder={t("allCategories")} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t("allCategories")}</SelectItem>
            {initialCategories.map((cat) => (
              <SelectItem key={cat.id} value={cat.id}>
                {cat.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
        <Select value={stockFilter} onValueChange={setStockFilter}>
          <SelectTrigger className="sm:max-w-xs">
            <SelectValue placeholder={t("allStock")} />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">{t("allStock")}</SelectItem>
            <SelectItem value="low">
              <span className="flex items-center gap-1.5">
                <AlertTriangle className="h-3.5 w-3.5 text-amber-500" />
                {t("lowStockFilter")}
              </span>
            </SelectItem>
          </SelectContent>
        </Select>
        {!isLoading && (
          <p className="self-center text-sm text-zinc-400">
            {t("productCount", { count: totalRecords })}
          </p>
        )}
      </div>

      {error ? (
        <p className="text-sm text-red-500">{error}</p>
      ) : isError ? (
        <p className="text-sm text-red-500">Erro ao carregar produtos.</p>
      ) : isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: PAGE_SIZE }).map((_, i) => (
            <div
              key={i}
              className="h-14 animate-pulse rounded-lg bg-zinc-100 dark:bg-zinc-800"
            />
          ))}
        </div>
      ) : products.length === 0 ? (
        <p className="text-sm text-zinc-400">{t("noProducts")}</p>
      ) : (
        <div className="overflow-x-auto rounded-xl border border-zinc-200 bg-white shadow-sm dark:border-zinc-700 dark:bg-zinc-800">
          <table className="min-w-full divide-y divide-zinc-100 dark:divide-zinc-700">
            <thead>
              <tr className="bg-zinc-50 dark:bg-zinc-900">
                <th
                  className="cursor-pointer select-none px-6 py-3 text-left text-xs font-semibold uppercase tracking-wide text-zinc-500 hover:text-zinc-900 dark:hover:text-zinc-100"
                  onClick={() => toggleSort("name")}
                >
                  {t("columns.product")} <SortIcon field="name" sortField={sortField} sortDir={sortDir} />
                </th>
                <th className="px-6 py-3 text-left text-xs font-semibold uppercase tracking-wide text-zinc-500">
                  {t("columns.category")}
                </th>
                <th
                  className="cursor-pointer select-none px-6 py-3 text-right text-xs font-semibold uppercase tracking-wide text-zinc-500 hover:text-zinc-900 dark:hover:text-zinc-100"
                  onClick={() => toggleSort("price")}
                >
                  {t("columns.price")} <SortIcon field="price" sortField={sortField} sortDir={sortDir} />
                </th>
                <th
                  className="cursor-pointer select-none px-6 py-3 text-center text-xs font-semibold uppercase tracking-wide text-zinc-500 hover:text-zinc-900 dark:hover:text-zinc-100"
                  onClick={() => toggleSort("stockQuantity")}
                >
                  {t("columns.stock")} <SortIcon field="stockQuantity" sortField={sortField} sortDir={sortDir} />
                </th>
                {isAdmin && (
                  <th className="px-6 py-3 text-right text-xs font-semibold uppercase tracking-wide text-zinc-500">
                    {t("columns.actions")}
                  </th>
                )}
              </tr>
            </thead>
            <tbody className="divide-y divide-zinc-100 dark:divide-zinc-700">
              {products.map((product) => (
                <tr
                  key={product.id}
                  className={cn(
                    "hover:bg-zinc-50 dark:hover:bg-zinc-900/50",
                    product.stockQuantity < 10 &&
                      "bg-amber-50/60 dark:bg-amber-900/10",
                  )}
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
                    <div className="flex items-center justify-center gap-1.5">
                      {product.stockQuantity < 10 && (
                        <AlertTriangle className="h-3.5 w-3.5 shrink-0 text-amber-500" />
                      )}
                      <Badge variant={stockBadgeVariant(product.stockQuantity)}>
                        <span
                          className={
                            product.stockQuantity < 10 ? "font-bold" : ""
                          }
                        >
                          {product.stockQuantity} un.
                        </span>
                      </Badge>
                    </div>
                  </td>
                  {isAdmin && (
                    <td className="px-6 py-4 text-right">
                      <div className="flex justify-end gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setModal({ type: "stock", product })}
                        >
                          {t("actions.stock")}
                        </Button>
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => setModal({ type: "edit", product })}
                        >
                          {t("actions.edit")}
                        </Button>
                        <Button
                          variant="destructive"
                          size="sm"
                          onClick={() => setModal({ type: "delete", product })}
                        >
                          {t("actions.delete")}
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

      {/* Paginação */}
      {!isLoading && totalPages > 1 && (
        <div className="mt-4 flex items-center justify-between">
          <p className="text-sm text-zinc-500">
            {t("pagination.page")} {pageNumber} {t("pagination.of")} {totalPages}
          </p>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={pageNumber <= 1}
              onClick={() => setPageNumber((p) => p - 1)}
            >
              <ChevronLeft className="h-4 w-4" />
              {t("pagination.previous")}
            </Button>
            <Button
              variant="outline"
              size="sm"
              disabled={pageNumber >= totalPages}
              onClick={() => setPageNumber((p) => p + 1)}
            >
              {t("pagination.next")}
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
        </div>
      )}

      {modal.type === "create" && (
        <ProductFormModal
          categories={initialCategories}
          onClose={() => setModal({ type: "none" })}
          onSuccess={handleCreated}
        />
      )}

      {modal.type === "edit" && (
        <ProductFormModal
          product={modal.product}
          categories={initialCategories}
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
            <DialogTitle>{t("deleteDialog.title")}</DialogTitle>
            <DialogDescription>
              {t("deleteDialog.confirmPrefix")}{" "}
              <span className="font-medium">
                {modal.type === "delete" ? modal.product.name : ""}
              </span>
              {t("deleteDialog.confirmSuffix")}
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
                deleteMutation.mutate(modal.product.id)
              }
            >
              {deleteMutation.isPending ? "Excluindo..." : t("actions.delete")}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </main>
  );
}
