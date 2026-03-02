import { test, expect } from "@playwright/test";
import { login } from "./helpers/login";

test.describe("Dashboard", () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test("shows stat cards on dashboard", async ({ page }) => {
    await page.goto("/");
    await expect(page.getByRole("heading", { name: /produtos/i }).first()).toBeVisible({ timeout: 10_000 });
  });

  test("shows Products navigation item in sidebar", async ({ page }) => {
    await page.goto("/");
    await expect(page.getByRole("link", { name: /produtos/i })).toBeVisible({ timeout: 10_000 });
  });

  test("shows Categories navigation item in sidebar", async ({ page }) => {
    await page.goto("/");
    await expect(page.getByRole("link", { name: /categorias/i })).toBeVisible({ timeout: 10_000 });
  });
});
