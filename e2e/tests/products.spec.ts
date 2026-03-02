import { test, expect } from "@playwright/test";
import { login } from "./helpers/login";

test.describe("Products", () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test("products list page loads", async ({ page }) => {
    await page.goto("/products");
    await expect(page.getByRole("heading", { name: /produtos/i })).toBeVisible({ timeout: 10_000 });
  });

  test("admin can see New Product button", async ({ page }) => {
    await page.goto("/products");
    await expect(page.getByRole("button", { name: /novo produto/i })).toBeVisible({ timeout: 10_000 });
  });

  test("stock filter dropdown is visible", async ({ page }) => {
    await page.goto("/products");
    // Use role=combobox with hasText because SelectValue renders display:contents
    await expect(
      page.locator('[role="combobox"]', { hasText: /todos os estoques/i })
    ).toBeVisible({ timeout: 10_000 });
  });

  test("search input is visible", async ({ page }) => {
    await page.goto("/products");
    await expect(page.getByPlaceholder(/buscar/i)).toBeVisible({ timeout: 10_000 });
  });
});
