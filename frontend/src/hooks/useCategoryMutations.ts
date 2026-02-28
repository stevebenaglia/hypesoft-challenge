"use client";

import { useSession } from "next-auth/react";
import { useMutation } from "@tanstack/react-query";
import { categoryService } from "@/services/categoryService";

export function useDeleteCategory(options?: { onSuccess?: (id: string) => void }) {
  const { data: session } = useSession();

  return useMutation({
    mutationFn: (id: string) => categoryService.delete(id, session?.accessToken),
    onSuccess: (_data, id) => {
      options?.onSuccess?.(id);
    },
  });
}
