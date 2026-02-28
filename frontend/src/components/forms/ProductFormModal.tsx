"use client";

import { useEffect } from "react";
import { useSession } from "next-auth/react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useMutation } from "@tanstack/react-query";
import { productService } from "@/services/productService";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import type { Product, Category } from "@/types/api";

const productSchema = z.object({
  name: z.string().min(1, "Nome é obrigatório").max(200),
  description: z.string().max(500).optional(),
  price: z
    .number({ error: "Preço é obrigatório" })
    .positive("Preço deve ser maior que zero"),
  stockQuantity: z
    .number({ error: "Quantidade é obrigatória" })
    .int()
    .min(0, "Quantidade deve ser maior ou igual a zero"),
  categoryId: z.string().min(1, "Selecione uma categoria"),
});

type ProductFormData = z.infer<typeof productSchema>;

interface ProductFormModalProps {
  product?: Product | null;
  categories: Category[];
  onClose: () => void;
  onSuccess: (product: Product) => void;
}

export default function ProductFormModal({
  product,
  categories,
  onClose,
  onSuccess,
}: ProductFormModalProps) {
  const { data: session } = useSession();
  const isEditing = !!product;

  const {
    register,
    handleSubmit,
    control,
    reset,
    formState: { errors },
  } = useForm<ProductFormData>({
    resolver: zodResolver(productSchema),
    defaultValues: {
      name: product?.name ?? "",
      description: product?.description ?? "",
      price: product?.price ?? undefined,
      stockQuantity: product?.stockQuantity ?? 0,
      categoryId: product?.categoryId ?? (categories[0]?.id ?? ""),
    },
  });

  useEffect(() => {
    reset({
      name: product?.name ?? "",
      description: product?.description ?? "",
      price: product?.price ?? undefined,
      stockQuantity: product?.stockQuantity ?? 0,
      categoryId: product?.categoryId ?? (categories[0]?.id ?? ""),
    });
  }, [product, categories, reset]);

  const mutation = useMutation({
    mutationFn: (data: ProductFormData) =>
      isEditing
        ? productService.update(product!.id, data, session?.accessToken)
        : productService.create(data, session?.accessToken),
    onSuccess,
  });

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEditing ? "Editar Produto" : "Novo Produto"}</DialogTitle>
        </DialogHeader>

        <form
          onSubmit={handleSubmit((data) => mutation.mutate(data))}
          className="flex flex-col gap-4"
        >
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="name">
              Nome <span className="text-red-500">*</span>
            </Label>
            <Input id="name" {...register("name")} />
            {errors.name && (
              <p className="text-xs text-red-500">{errors.name.message}</p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="description">Descrição</Label>
            <Textarea id="description" rows={2} {...register("description")} />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="price">
                Preço (R$) <span className="text-red-500">*</span>
              </Label>
              <Input
                id="price"
                type="number"
                step="0.01"
                min="0.01"
                {...register("price", { valueAsNumber: true })}
              />
              {errors.price && (
                <p className="text-xs text-red-500">{errors.price.message}</p>
              )}
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="stockQuantity">
                Qtd. Estoque <span className="text-red-500">*</span>
              </Label>
              <Input
                id="stockQuantity"
                type="number"
                min="0"
                {...register("stockQuantity", { valueAsNumber: true })}
              />
              {errors.stockQuantity && (
                <p className="text-xs text-red-500">
                  {errors.stockQuantity.message}
                </p>
              )}
            </div>
          </div>

          <div className="flex flex-col gap-1.5">
            <Label>
              Categoria <span className="text-red-500">*</span>
            </Label>
            <Controller
              name="categoryId"
              control={control}
              render={({ field }) => (
                <Select value={field.value} onValueChange={field.onChange}>
                  <SelectTrigger>
                    <SelectValue placeholder="Selecione uma categoria" />
                  </SelectTrigger>
                  <SelectContent>
                    {categories.map((cat) => (
                      <SelectItem key={cat.id} value={cat.id}>
                        {cat.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              )}
            />
            {errors.categoryId && (
              <p className="text-xs text-red-500">{errors.categoryId.message}</p>
            )}
          </div>

          {mutation.error && (
            <p className="text-xs text-red-500">
              {mutation.error instanceof Error
                ? mutation.error.message
                : "Erro ao salvar produto."}
            </p>
          )}

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              Cancelar
            </Button>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending
                ? "Salvando..."
                : isEditing
                  ? "Salvar"
                  : "Criar"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
