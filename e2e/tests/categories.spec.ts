import { test, expect } from "@playwright/test";
import { login } from "./helpers/login";

test.describe("Categories", () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test("categories list page loads", async ({ page }) => {
    await page.goto("/categories");
    await expect(page.getByRole("heading", { name: /categorias/i })).toBeVisible({ timeout: 10_000 });
  });

  test("admin can see New Category button", async ({ page }) => {
    await page.goto("/categories");
    await expect(page.getByRole("button", { name: /nova categoria/i })).toBeVisible({ timeout: 10_000 });
  });
});
