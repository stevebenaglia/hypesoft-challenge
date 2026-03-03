"use client";

import { useSession } from "next-auth/react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { useTranslations } from "next-intl";
import { categoryService } from "@/services/categoryService";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import type { Category } from "@/types/api";

const categorySchema = z.object({
  name: z.string().min(1, "Nome é obrigatório").max(100),
  description: z.string().max(500).optional(),
});

type CategoryFormData = z.infer<typeof categorySchema>;

interface CategoryFormModalProps {
  category?: Category | null;
  onClose: () => void;
  onSuccess: (category: Category) => void;
}

export default function CategoryFormModal({
  category,
  onClose,
  onSuccess,
}: CategoryFormModalProps) {
  const { data: session } = useSession();
  const isEditing = !!category;
  const t = useTranslations("forms.category");
  const tCommon = useTranslations("common");

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CategoryFormData>({
    resolver: zodResolver(categorySchema),
    mode: "onChange",
    defaultValues: {
      name: category?.name ?? "",
      description: category?.description ?? "",
    },
  });

  const mutation = useMutation({
    mutationFn: (data: CategoryFormData) =>
      isEditing
        ? categoryService.update(category!.id, data, session?.accessToken)
        : categoryService.create(data, session?.accessToken),
    onSuccess: (savedCategory) => {
      toast.success(isEditing ? t("toast.updated") : t("toast.created"));
      onSuccess(savedCategory);
    },
    onError: (error) => {
      toast.error(error instanceof Error ? error.message : t("toast.error"));
    },
  });

  return (
    <Dialog open onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? t("editTitle") : t("createTitle")}
          </DialogTitle>
        </DialogHeader>

        <form
          onSubmit={handleSubmit((data) => mutation.mutate(data))}
          className="flex flex-col gap-4"
        >
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="name">
              {t("name")} <span className="text-red-500">{tCommon("required")}</span>
            </Label>
            <Input id="name" autoFocus {...register("name")} />
            {errors.name && (
              <p className="text-xs text-red-500">{errors.name.message}</p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <Label htmlFor="description">{t("description")}</Label>
            <Textarea id="description" rows={3} {...register("description")} />
            {errors.description && (
              <p className="text-xs text-red-500">{errors.description.message}</p>
            )}
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={onClose}>
              {tCommon("cancel")}
            </Button>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending
                ? tCommon("saving")
                : isEditing
                  ? tCommon("save")
                  : tCommon("create")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
