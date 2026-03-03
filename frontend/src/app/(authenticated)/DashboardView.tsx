"use client";

import { useTranslations } from "next-intl";
import { exportDashboardPdf } from "@/utils/pdfExport";
import StatCard from "@/components/ui/StatCard";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { formatCurrency, stockBadgeVariant } from "@/utils/formatters";
import type { DashboardSummary, Product } from "@/types/api";
import DashboardChart from "@/components/charts/DashboardChart";
import { Package, Wallet, AlertTriangle } from "lucide-react";

interface DashboardViewProps {
  summary: DashboardSummary;
  error: string | null;
}

export default function DashboardView({ summary, error }: DashboardViewProps) {
  const t = useTranslations("dashboard");

  return (
    <div className="px-6 py-8">
      <div className="mb-8 flex items-start justify-between">
        <div>
          <h1 className="text-2xl font-bold text-zinc-900 dark:text-zinc-50">
            {t("title")}
          </h1>
          <p className="mt-1 text-sm text-zinc-500 dark:text-zinc-400">
            {t("subtitle")}
          </p>
        </div>
        <Button
          variant="outline"
          size="sm"
          onClick={() => exportDashboardPdf(summary)}
        >
          Exportar PDF
        </Button>
      </div>

      {error ? (
        <p className="text-sm text-red-500">{error}</p>
      ) : (
        <>
          {/* Stat cards */}
          <div className="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-3">
            <StatCard
              title={t("stats.totalProducts")}
              value={summary.totalProducts}
              icon={Package}
              iconClass="bg-blue-100 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400"
            />
            <StatCard
              title={t("stats.totalStockValue")}
              value={formatCurrency(summary.totalStockValue)}
              icon={Wallet}
              iconClass="bg-emerald-100 text-emerald-600 dark:bg-emerald-900/30 dark:text-emerald-400"
            />
            <StatCard
              title={t("stats.lowStock")}
              value={summary.lowStockProducts.length}
              subtitle={t("stats.lowStockSubtitle")}
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
                  {t("lowStockTable.title")}
                </h2>
                <p className="mt-0.5 text-xs text-zinc-500">
                  {t("lowStockTable.subtitle")}
                </p>
              </div>
              <div className="p-6">
                {summary.lowStockProducts.length === 0 ? (
                  <p className="py-4 text-center text-sm text-zinc-400">
                    {t("lowStockTable.empty")}
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
                  {t("chart.title")}
                </h2>
                <p className="mt-0.5 text-xs text-zinc-500">
                  {t("chart.subtitle")}
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
