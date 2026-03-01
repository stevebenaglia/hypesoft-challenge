"use client";

import dynamic from "next/dynamic";
import type { CategorySummary } from "@/types/api";

const ProductsByCategoryChart = dynamic(
  () => import("@/components/charts/ProductsByCategoryChart"),
  { ssr: false }
);

interface Props {
  data: CategorySummary[];
}

export default function DashboardChart({ data }: Props) {
  return <ProductsByCategoryChart data={data} />;
}
