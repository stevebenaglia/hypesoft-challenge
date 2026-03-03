import { getServerSession } from "next-auth";
import { authOptions } from "@/lib/auth";
import { apiFetch } from "@/lib/apiFetch";
import type { DashboardSummary } from "@/types/api";
import DashboardView from "@/app/(authenticated)/DashboardView";

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

  return <DashboardView summary={summary} error={error} />;
}
