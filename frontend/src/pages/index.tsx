import type { GetServerSideProps } from "next";
import dynamic from "next/dynamic";
import { getServerSession } from "next-auth";
import { authOptions } from "@/pages/api/auth/[...nextauth]";
import { apiFetch } from "@/lib/apiFetch";
import Header from "@/components/layout/Header";
import StatCard from "@/components/ui/StatCard";
import type { DashboardSummary, Product } from "@/types/api";

const ProductsByCategoryChart = dynamic(
  () => import("@/components/charts/ProductsByCategoryChart"),
  { ssr: false }
);

interface DashboardProps {
  summary: DashboardSummary;
  error?: string;
}

function stockBadgeClass(qty: number) {
  if (qty === 0) return "bg-red-100 text-red-700";
  return "bg-amber-100 text-amber-700";
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(value);
}

export default function Dashboard({ summary, error }: DashboardProps) {
  return (
    <div className="min-h-screen bg-zinc-50 dark:bg-zinc-950">
      <Header />

      <main className="mx-auto max-w-6xl px-6 py-8">
        <h1 className="mb-6 text-2xl font-semibold text-zinc-900 dark:text-zinc-50">
          Dashboard
        </h1>

        {error ? (
          <p className="text-sm text-red-500">{error}</p>
        ) : (
          <>
            {/* KPI cards */}
            <div className="mb-8 grid grid-cols-1 gap-4 sm:grid-cols-3">
              <StatCard
                title="Total de Produtos"
                value={summary.totalProducts}
              />
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
              {/* Low stock list */}
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
                        <span
                          className={`rounded-full px-2.5 py-0.5 text-xs font-semibold ${stockBadgeClass(p.stockQuantity)}`}
                        >
                          {p.stockQuantity} un.
                        </span>
                      </li>
                    ))}
                  </ul>
                )}
              </div>

              {/* Chart */}
              <div className="rounded-xl border border-zinc-200 bg-white p-6 shadow-sm dark:border-zinc-700 dark:bg-zinc-800">
                <h2 className="mb-4 text-sm font-semibold text-zinc-900 dark:text-zinc-50">
                  Produtos por Categoria
                </h2>
                <ProductsByCategoryChart data={summary.productsByCategory} />
              </div>
            </div>
          </>
        )}
      </main>
    </div>
  );
}

export const getServerSideProps: GetServerSideProps = async (context) => {
  const session = await getServerSession(context.req, context.res, authOptions);

  if (!session) {
    return { redirect: { destination: "/auth/signin", permanent: false } };
  }

  try {
    const summary = await apiFetch<DashboardSummary>("/api/dashboard", {
      accessToken: session.accessToken,
    });
    return { props: { summary } };
  } catch (err: unknown) {
    return {
      props: {
        summary: {
          totalProducts: 0,
          totalStockValue: 0,
          lowStockProducts: [],
          productsByCategory: [],
        },
        error: err instanceof Error ? err.message : "Erro ao carregar dashboard.",
      },
    };
  }
};
