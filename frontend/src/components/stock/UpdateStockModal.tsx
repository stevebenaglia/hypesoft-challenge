"use client";

import { useSession } from "next-auth/react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useMutation } from "@tanstack/react-query";
import { apiFetch } from "@/lib/apiFetch";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import type { Product } from "@/types/api";

const stockSchema = z.object({
  quantity: z
    .number({ error: "Quantidade é obrigatória" })
    .int()
    .min(0, "Quantidade deve ser maior ou igual a zero"),
});

type StockFormData = z.infer<typeof stockSchema>;

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

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<StockFormData>({
    resolver: zodResolver(stockSchema),
    defaultValues: { quantity: product.stockQuantity },
  });

  const mutation = useMutation({
    mutationFn: (data: StockFormData) =>
      apiFetch<Product>(`/api/products/${product.id}/stock`, {
        method: "PATCH",
        accessToken: session?.accessToken,
        body: JSON.stringify({ quantity: data.quantity }),
      }),
    onSuccess,
  });

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>Atualizar Estoque</DialogTitle>
          <DialogDescription className="truncate">{product.name}</DialogDescription>
        </DialogHeader>

        <form
          onSubmit={handleSubmit((data) => mutation.mutate(data))}
          className="flex flex-col gap-4"
        >
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="quantity">Nova quantidade</Label>
            <Input
              id="quantity"
              type="number"
              min={0}
              {...register("quantity", { valueAsNumber: true })}
            />
            {errors.quantity && (
              <p className="text-xs text-red-500">{errors.quantity.message}</p>
            )}
          </div>

          {mutation.error && (
            <p className="text-xs text-red-500">
              {mutation.error instanceof Error
                ? mutation.error.message
                : "Erro ao atualizar estoque."}
            </p>
          )}

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              Cancelar
            </Button>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? "Salvando..." : "Salvar"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
