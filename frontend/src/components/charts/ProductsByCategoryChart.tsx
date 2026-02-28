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
        backgroundColor: "rgba(24, 24, 27, 0.8)",
        borderRadius: 6,
      },
    ],
  };

  const options = {
    responsive: true,
    plugins: {
      legend: { display: false },
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: { stepSize: 1 },
        grid: { color: "rgba(0,0,0,0.05)" },
      },
      x: {
        grid: { display: false },
      },
    },
  };

  return <Bar data={chartData} options={options} />;
}
