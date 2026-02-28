"use client";

import { useSession } from "next-auth/react";
import { useMutation } from "@tanstack/react-query";
import { categoryService } from "@/services/categoryService";

export function useDeleteCategory(options?: {
  onSuccess?: (id: string) => void;
  onError?: (error: Error) => void;
}) {
  const { data: session } = useSession();

  return useMutation({
    mutationFn: (id: string) => categoryService.delete(id, session?.accessToken),
    onSuccess: (_data, id) => {
      options?.onSuccess?.(id);
    },
    onError: (error) => {
      options?.onError?.(error instanceof Error ? error : new Error("Erro ao excluir categoria."));
    },
  });
}
