import { test, expect } from "@playwright/test";
import { login } from "./helpers/login";

/**
 * Role-based access control tests.
 * Verifies that the "user" role cannot access admin-only actions,
 * while the "admin" role can.
 */

test.describe("Permissions: regular user (role=user)", () => {
  test.beforeEach(async ({ page }) => {
    // Login with the non-admin "user" account
    await login(page, "user", "user");
  });

  test("user cannot see 'Nova Categoria' button on categories page", async ({ page }) => {
    await page.goto("/categories");
    await expect(page.getByRole("heading", { name: /categorias/i })).toBeVisible({ timeout: 10_000 });
    await expect(page.getByRole("button", { name: /nova categoria/i })).not.toBeVisible();
  });

  test("user cannot see 'Novo Produto' button on products page", async ({ page }) => {
    await page.goto("/products");
    await expect(page.getByRole("heading", { name: /produtos/i })).toBeVisible({ timeout: 10_000 });
    await expect(page.getByRole("button", { name: /novo produto/i })).not.toBeVisible();
  });

  test("user cannot see Editar / Excluir action buttons on categories page", async ({ page }) => {
    await page.goto("/categories");
    await expect(page.getByRole("heading", { name: /categorias/i })).toBeVisible({ timeout: 10_000 });
    // The "Ações" column header is only rendered for admins
    await expect(page.getByRole("columnheader", { name: /ações/i })).not.toBeVisible();
    await expect(page.getByRole("button", { name: /^editar$/i })).not.toBeVisible();
    await expect(page.getByRole("button", { name: /^excluir$/i })).not.toBeVisible();
  });

  test("user cannot see Estoque / Editar / Excluir action buttons on products page", async ({ page }) => {
    await page.goto("/products");
    await expect(page.getByRole("heading", { name: /produtos/i })).toBeVisible({ timeout: 10_000 });
    // The "Ações" column header is only rendered for admins
    await expect(page.getByRole("columnheader", { name: /ações/i })).not.toBeVisible();
    await expect(page.getByRole("button", { name: /estoque/i })).not.toBeVisible();
    await expect(page.getByRole("button", { name: /^editar$/i })).not.toBeVisible();
    await expect(page.getByRole("button", { name: /^excluir$/i })).not.toBeVisible();
  });

  test("user can still view the products list (read access)", async ({ page }) => {
    await page.goto("/products");
    await expect(page.getByRole("heading", { name: /produtos/i })).toBeVisible({ timeout: 10_000 });
    // Search and filter controls are visible to all authenticated users
    await expect(page.getByPlaceholder(/buscar/i)).toBeVisible({ timeout: 10_000 });
  });

  test("user can still view the categories list (read access)", async ({ page }) => {
    await page.goto("/categories");
    await expect(page.getByRole("heading", { name: /categorias/i })).toBeVisible({ timeout: 10_000 });
  });

  test("user can access the dashboard", async ({ page }) => {
    await page.goto("/");
    // Dashboard heading contains "Produtos" stat card
    await expect(page.getByRole("heading", { name: /produtos/i }).first()).toBeVisible({ timeout: 10_000 });
  });
});

test.describe("Permissions: admin user (role=admin)", () => {
  test.beforeEach(async ({ page }) => {
    await login(page, "admin", "admin");
  });

  test("admin can see 'Nova Categoria' button on categories page", async ({ page }) => {
    await page.goto("/categories");
    await expect(page.getByRole("button", { name: /nova categoria/i })).toBeVisible({ timeout: 10_000 });
  });

  test("admin can see 'Novo Produto' button on products page", async ({ page }) => {
    await page.goto("/products");
    await expect(page.getByRole("button", { name: /novo produto/i })).toBeVisible({ timeout: 10_000 });
  });
});
