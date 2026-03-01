"use client";

import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
  Legend,
} from "chart.js";
import { Bar } from "react-chartjs-2";
import type { CategorySummary } from "@/types/api";

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

interface ProductsByCategoryChartProps {
  data: CategorySummary[];
}

export default function ProductsByCategoryChart({ data }: ProductsByCategoryChartProps) {
  if (data.length === 0) {
    return (
      <p className="text-sm text-zinc-400 text-center py-8">
        Nenhuma categoria com produtos.
      </p>
    );
  }

  const chartData = {
    labels: data.map((d) => d.categoryName),
    datasets: [
      {
        label: "Produtos",
        data: data.map((d) => d.productCount),
        backgroundColor: "rgba(124, 58, 237, 0.85)",
        hoverBackgroundColor: "rgba(109, 40, 217, 0.95)",
        borderRadius: 8,
        borderSkipped: false as const,
      },
    ],
  };

  const options = {
    responsive: true,
    plugins: {
      legend: { display: false },
      tooltip: {
        backgroundColor: "rgba(24, 24, 27, 0.9)",
        titleColor: "#f4f4f5",
        bodyColor: "#a1a1aa",
        padding: 10,
        cornerRadius: 8,
      },
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: { stepSize: 1, color: "#a1a1aa" },
        grid: { color: "rgba(161,161,170,0.1)" },
        border: { display: false },
      },
      x: {
        grid: { display: false },
        ticks: { color: "#a1a1aa" },
        border: { display: false },
      },
    },
  };

  return <Bar data={chartData} options={options} />;
}
