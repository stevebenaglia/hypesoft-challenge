import { describe, it, expect } from "vitest";
import { formatCurrency, stockBadgeVariant } from "@/utils/formatters";

describe("formatCurrency", () => {
  it("formats a positive number as BRL currency", () => {
    const result = formatCurrency(1234.56);
    expect(result).toMatch(/1\.234,56/);
    expect(result).toMatch(/R\$/);
  });

  it("formats zero correctly", () => {
    const result = formatCurrency(0);
    expect(result).toMatch(/0,00/);
  });

  it("formats large values with thousand separator", () => {
    const result = formatCurrency(1000000);
    expect(result).toMatch(/1\.000\.000/);
  });
});

describe("stockBadgeVariant", () => {
  it("returns 'destructive' when quantity is 0", () => {
    expect(stockBadgeVariant(0)).toBe("destructive");
  });

  it("returns 'secondary' when quantity is between 1 and 9", () => {
    expect(stockBadgeVariant(1)).toBe("secondary");
    expect(stockBadgeVariant(5)).toBe("secondary");
    expect(stockBadgeVariant(9)).toBe("secondary");
  });

  it("returns 'outline' when quantity is 10 or more", () => {
    expect(stockBadgeVariant(10)).toBe("outline");
    expect(stockBadgeVariant(20)).toBe("outline");
    expect(stockBadgeVariant(1000)).toBe("outline");
  });
});
