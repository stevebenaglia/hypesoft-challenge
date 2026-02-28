"use client";

import { useSession } from "next-auth/react";
import { useMutation } from "@tanstack/react-query";
import { productService } from "@/services/productService";

export function useDeleteProduct(options?: {
  onSuccess?: (id: string) => void;
  onError?: (error: Error) => void;
}) {
  const { data: session } = useSession();

  return useMutation({
    mutationFn: (id: string) => productService.delete(id, session?.accessToken),
    onSuccess: (_data, id) => {
      options?.onSuccess?.(id);
    },
    onError: (error) => {
      options?.onError?.(error instanceof Error ? error : new Error("Erro ao excluir produto."));
    },
  });
}
