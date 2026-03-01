import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";
import { apiFetch } from "@/lib/apiFetch";
import StatCard from "@/components/ui/StatCard";
import { Badge } from "@/components/ui/badge";
import { formatCurrency, stockBadgeVariant } from "@/utils/formatters";
import type { DashboardSummary, Product } from "@/types/api";
import DashboardChart from "@/components/charts/DashboardChart";
import { Package, Wallet, AlertTriangle } from "lucide-react";

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
    <div className="px-6 py-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-zinc-900 dark:text-zinc-50">
          Dashboard
        </h1>
        <p className="mt-1 text-sm text-zinc-500 dark:text-zinc-400">
          Visão geral do seu estoque
        </p>
      </div>

      {error ? (
        <p className="text-sm text-red-500">{error}</p>
      ) : (
        <>
          {/* Stat cards */}
          <div className="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-3">
            <StatCard
              title="Total de Produtos"
              value={summary.totalProducts}
              icon={Package}
              iconClass="bg-blue-100 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400"
            />
            <StatCard
              title="Valor Total do Estoque"
              value={formatCurrency(summary.totalStockValue)}
              icon={Wallet}
              iconClass="bg-emerald-100 text-emerald-600 dark:bg-emerald-900/30 dark:text-emerald-400"
            />
            <StatCard
              title="Estoque Baixo"
              value={summary.lowStockProducts.length}
              subtitle="Produtos com menos de 10 unidades"
              icon={AlertTriangle}
              iconClass="bg-amber-100 text-amber-600 dark:bg-amber-900/30 dark:text-amber-400"
            />
          </div>

          {/* Content grid */}
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-2">
            {/* Low stock table */}
            <div className="rounded-xl border border-zinc-200 bg-white shadow-sm dark:border-zinc-700 dark:bg-zinc-900">
              <div className="border-b border-zinc-100 px-6 py-4 dark:border-zinc-800">
                <h2 className="text-sm font-semibold text-zinc-900 dark:text-zinc-50">
                  Produtos com Estoque Baixo
                </h2>
                <p className="mt-0.5 text-xs text-zinc-500">
                  Atenção: abaixo de 10 unidades
                </p>
              </div>
              <div className="p-6">
                {summary.lowStockProducts.length === 0 ? (
                  <p className="py-4 text-center text-sm text-zinc-400">
                    Nenhum produto com estoque baixo.
                  </p>
                ) : (
                  <ul className="space-y-2">
                    {summary.lowStockProducts.map((p: Product) => (
                      <li
                        key={p.id}
                        className="flex items-center justify-between rounded-lg bg-zinc-50 px-4 py-3 dark:bg-zinc-800/60"
                      >
                        <div>
                          <p className="text-sm font-medium text-zinc-800 dark:text-zinc-200">
                            {p.name}
                          </p>
                          <p className="text-xs text-zinc-500">
                            {p.categoryName ?? "—"}
                          </p>
                        </div>
                        <Badge variant={stockBadgeVariant(p.stockQuantity)}>
                          {p.stockQuantity} un.
                        </Badge>
                      </li>
                    ))}
                  </ul>
                )}
              </div>
            </div>

            {/* Chart */}
            <div className="rounded-xl border border-zinc-200 bg-white shadow-sm dark:border-zinc-700 dark:bg-zinc-900">
              <div className="border-b border-zinc-100 px-6 py-4 dark:border-zinc-800">
                <h2 className="text-sm font-semibold text-zinc-900 dark:text-zinc-50">
                  Produtos por Categoria
                </h2>
                <p className="mt-0.5 text-xs text-zinc-500">
                  Distribuição do catálogo
                </p>
              </div>
              <div className="p-6">
                <DashboardChart data={summary.productsByCategory} />
              </div>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
