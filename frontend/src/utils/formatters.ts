export function formatCurrency(value: number): string {
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
  }).format(value);
}

export function formatDate(date: string | Date): string {
  return new Intl.DateTimeFormat("pt-BR").format(
    typeof date === "string" ? new Date(date) : date
  );
}

export function stockBadgeVariant(
  qty: number
): "destructive" | "secondary" | "outline" {
  if (qty === 0) return "destructive";
  if (qty < 10) return "secondary";
  return "outline";
}
