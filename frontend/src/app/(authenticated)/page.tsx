import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";
import { apiFetch } from "@/lib/apiFetch";
import StatCard from "@/components/ui/StatCard";
import { Badge } from "@/components/ui/badge";
import { formatCurrency, stockBadgeVariant } from "@/utils/formatters";
import type { DashboardSummary, Product } from "@/types/api";
import DashboardChart from "@/components/charts/DashboardChart";

export default async function DashboardPage() {
  const session = await getServerSession(authOptions);

  let summary: DashboardSummary = {
    totalProducts: 0,
    totalStockValue: 0,
    lowStockProducts: [],
    productsByCategory: [],
  };
  let error: string | null = null;

  try {
    summary = await apiFetch<DashboardSummary>("/api/dashboard", {
      accessToken: session!.accessToken,
    });
  } catch (err: unknown) {
    error = err instanceof Error ? err.message : "Erro ao carregar dashboard.";
  }

  return (
    <main className="mx-auto max-w-6xl px-6 py-8">
      <h1 className="mb-6 text-2xl font-semibold text-zinc-900 dark:text-zinc-50">
        Dashboard
      </h1>

      {error ? (
        <p className="text-sm text-red-500">{error}</p>
      ) : (
        <>
          <div className="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-3">
            <StatCard title="Total de Produtos" value={summary.totalProducts} />
            <StatCard
              title="Valor Total do Estoque"
              value={formatCurrency(summary.totalStockValue)}
            />
            <StatCard
              title="Estoque Baixo"
              value={summary.lowStockProducts.length}
              subtitle="Produtos com menos de 10 unidades"
            />
          </div>

          <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
            <div className="rounded-xl border border-zinc-200 bg-white p-6 shadow-sm dark:border-zinc-700 dark:bg-zinc-800">
              <h2 className="mb-4 text-sm font-semibold text-zinc-900 dark:text-zinc-50">
                Produtos com Estoque Baixo
              </h2>
              {summary.lowStockProducts.length === 0 ? (
                <p className="text-sm text-zinc-400">
                  Nenhum produto com estoque baixo.
                </p>
              ) : (
                <ul className="space-y-2">
                  {summary.lowStockProducts.map((p: Product) => (
                    <li
                      key={p.id}
                      className="flex items-center justify-between rounded-lg bg-zinc-50 px-4 py-2.5 dark:bg-zinc-900"
                    >
                      <div>
                        <p className="text-sm font-medium text-zinc-800 dark:text-zinc-200">
                          {p.name}
                        </p>
                        <p className="text-xs text-zinc-500">{p.categoryName}</p>
                      </div>
                      <Badge variant={stockBadgeVariant(p.stockQuantity)}>
                        {p.stockQuantity} un.
                      </Badge>
                    </li>
                  ))}
                </ul>
              )}
            </div>

            <div className="rounded-xl border border-zinc-200 bg-white p-6 shadow-sm dark:border-zinc-700 dark:bg-zinc-800">
              <h2 className="mb-4 text-sm font-semibold text-zinc-900 dark:text-zinc-50">
                Produtos por Categoria
              </h2>
              <DashboardChart data={summary.productsByCategory} />
            </div>
          </div>
        </>
      )}
    </main>
  );
}
