import { useState } from "react";
import { useSession } from "next-auth/react";
import { apiFetch } from "@/lib/apiFetch";
import type { Product } from "@/types/api";

interface UpdateStockModalProps {
  product: Product;
  onClose: () => void;
  onSuccess: (updated: Product) => void;
}

export default function UpdateStockModal({
  product,
  onClose,
  onSuccess,
}: UpdateStockModalProps) {
  const { data: session } = useSession();
  const [quantity, setQuantity] = useState(product.stockQuantity);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const updated = await apiFetch<Product>(
        `/api/products/${product.id}/stock`,
        {
          method: "PATCH",
          accessToken: session?.accessToken,
          body: JSON.stringify({ quantity }),
        }
      );
      onSuccess(updated);
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : "Erro ao atualizar estoque.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/40"
      onClick={(e) => e.target === e.currentTarget && onClose()}
    >
      <div className="w-full max-w-sm rounded-2xl border border-zinc-200 bg-white p-6 shadow-xl dark:border-zinc-700 dark:bg-zinc-800">
        <h2 className="mb-1 text-base font-semibold text-zinc-900 dark:text-zinc-50">
          Atualizar Estoque
        </h2>
        <p className="mb-5 text-sm text-zinc-500 truncate">{product.name}</p>

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div>
            <label className="mb-1 block text-sm font-medium text-zinc-700 dark:text-zinc-300">
              Nova quantidade
            </label>
            <input
              type="number"
              min={0}
              value={quantity}
              onChange={(e) => setQuantity(Number(e.target.value))}
              className="w-full rounded-lg border border-zinc-300 px-3 py-2 text-sm text-zinc-900 focus:outline-none focus:ring-2 focus:ring-zinc-900 dark:border-zinc-600 dark:bg-zinc-900 dark:text-zinc-50"
            />
          </div>

          {error && <p className="text-xs text-red-500">{error}</p>}

          <div className="flex justify-end gap-2">
            <button
              type="button"
              onClick={onClose}
              className="rounded-lg px-4 py-2 text-sm font-medium text-zinc-600 hover:bg-zinc-100 dark:hover:bg-zinc-700"
            >
              Cancelar
            </button>
            <button
              type="submit"
              disabled={loading}
              className="rounded-lg bg-zinc-900 px-4 py-2 text-sm font-medium text-white hover:bg-zinc-700 disabled:opacity-50 dark:bg-zinc-50 dark:text-zinc-900 dark:hover:bg-zinc-200"
            >
              {loading ? "Salvando..." : "Salvar"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
