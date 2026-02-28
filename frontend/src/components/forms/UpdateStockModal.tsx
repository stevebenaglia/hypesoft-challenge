"use client";

import { useSession } from "next-auth/react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { productService } from "@/services/productService";
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
    mode: "onChange",
    defaultValues: { quantity: product.stockQuantity },
  });

  const mutation = useMutation({
    mutationFn: (data: StockFormData) =>
      productService.updateStock(product.id, data.quantity, session?.accessToken),
    onSuccess: (updated) => {
      toast.success("Estoque atualizado com sucesso!");
      onSuccess(updated);
    },
    onError: (error) => {
      toast.error(error instanceof Error ? error.message : "Erro ao atualizar estoque.");
    },
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
