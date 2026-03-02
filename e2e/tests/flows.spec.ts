import { test, expect } from "@playwright/test";
import { login } from "./helpers/login";

/**
 * Full end-to-end flow tests.
 * Each test covers a complete user journey that spans multiple pages and API calls.
 * These tests require the full stack (Next.js + API + MongoDB + Keycloak) to be running.
 */

test.describe("Full flow: category and product lifecycle", () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test("admin can create a category and see it in the list", async ({ page }) => {
    await page.goto("/categories");

    // Open the create modal
    await page.getByRole("button", { name: /nova categoria/i }).click();
    await expect(page.getByRole("dialog")).toBeVisible({ timeout: 5_000 });

    // Fill the form
    const categoryName = `E2E Cat ${Date.now()}`;
    await page.locator("#name").fill(categoryName);
    await page.locator("#description").fill("Categoria criada por E2E test");

    // Submit
    await page.getByRole("button", { name: /^criar$/i }).click();

    // Dialog should close and new row should appear in the table
    await expect(page.getByRole("dialog")).not.toBeVisible({ timeout: 10_000 });
    await expect(page.getByText(categoryName)).toBeVisible({ timeout: 10_000 });
  });

  test("admin can create a product in an existing category", async ({ page }) => {
    // First, ensure a category exists by creating one
    await page.goto("/categories");
    await page.getByRole("button", { name: /nova categoria/i }).click();
    await expect(page.getByRole("dialog")).toBeVisible({ timeout: 5_000 });
    const categoryName = `E2E CatForProduct ${Date.now()}`;
    await page.locator("#name").fill(categoryName);
    await page.getByRole("button", { name: /^criar$/i }).click();
    await expect(page.getByRole("dialog")).not.toBeVisible({ timeout: 10_000 });

    // Now create a product
    await page.goto("/products");
    await page.getByRole("button", { name: /novo produto/i }).click();
    await expect(page.getByRole("dialog")).toBeVisible({ timeout: 5_000 });

    const productName = `E2E Product ${Date.now()}`;
    await page.locator("#name").fill(productName);
    await page.locator("#price").fill("99.99");
    await page.locator("#stockQuantity").fill("25");

    // Select the category we just created
    await page.getByRole("combobox").click();
    await page.getByRole("option", { name: categoryName }).click();

    await page.getByRole("button", { name: /^criar$/i }).click();

    // Dialog should close and product should appear in the table
    await expect(page.getByRole("dialog")).not.toBeVisible({ timeout: 10_000 });
    await expect(page.getByText(productName)).toBeVisible({ timeout: 10_000 });
  });

  test("admin can update product stock", async ({ page }) => {
    // Create a category and a product first
    await page.goto("/categories");
    await page.getByRole("button", { name: /nova categoria/i }).click();
    await expect(page.getByRole("dialog")).toBeVisible({ timeout: 5_000 });
    const categoryName = `E2E CatStock ${Date.now()}`;
    await page.locator("#name").fill(categoryName);
    await page.getByRole("button", { name: /^criar$/i }).click();
    await expect(page.getByRole("dialog")).not.toBeVisible({ timeout: 10_000 });

    await page.goto("/products");
    await page.getByRole("button", { name: /novo produto/i }).click();
    await expect(page.getByRole("dialog")).toBeVisible({ timeout: 5_000 });
    const productName = `E2E ProductStock ${Date.now()}`;
    await page.locator("#name").fill(productName);
    await page.locator("#price").fill("49.99");
    await page.locator("#stockQuantity").fill("10");
    await page.getByRole("combobox").click();
    await page.getByRole("option", { name: categoryName }).click();
    await page.getByRole("button", { name: /^criar$/i }).click();
    await expect(page.getByRole("dialog")).not.toBeVisible({ timeout: 10_000 });
    await expect(page.getByText(productName)).toBeVisible({ timeout: 10_000 });

    // Click the "Estoque" button for our product row
    const row = page.locator("tr", { hasText: productName });
    await row.getByRole("button", { name: /estoque/i }).click();

    // Stock update modal should open
    await expect(page.getByRole("dialog")).toBeVisible({ timeout: 5_000 });
    await expect(page.getByRole("heading", { name: /atualizar estoque/i })).toBeVisible();

    // Change quantity and save
    await page.locator("#quantity").fill("99");
    await page.getByRole("button", { name: /^salvar$/i }).click();

    // Modal should close; updated quantity should appear in table
    await expect(page.getByRole("dialog")).not.toBeVisible({ timeout: 10_000 });
    await expect(page.getByText("99 un.")).toBeVisible({ timeout: 10_000 });
  });

  test("admin can delete a product", async ({ page }) => {
    // Create a category and a product to delete
    await page.goto("/categories");
    await page.getByRole("button", { name: /nova categoria/i }).click();
    await expect(page.getByRole("dialog")).toBeVisible({ timeout: 5_000 });
    const categoryName = `E2E CatDelete ${Date.now()}`;
    await page.locator("#name").fill(categoryName);
    await page.getByRole("button", { name: /^criar$/i }).click();
    await expect(page.getByRole("dialog")).not.toBeVisible({ timeout: 10_000 });

    await page.goto("/products");
    await page.getByRole("button", { name: /novo produto/i }).click();
    await expect(page.getByRole("dialog")).toBeVisible({ timeout: 5_000 });
    const productName = `E2E ProductDelete ${Date.now()}`;
    await page.locator("#name").fill(productName);
    await page.locator("#price").fill("19.99");
    await page.locator("#stockQuantity").fill("5");
    await page.getByRole("combobox").click();
    await page.getByRole("option", { name: categoryName }).click();
    await page.getByRole("button", { name: /^criar$/i }).click();
    await expect(page.getByRole("dialog")).not.toBeVisible({ timeout: 10_000 });
    await expect(page.getByText(productName)).toBeVisible({ timeout: 10_000 });

    // Click "Excluir" on the product row
    const row = page.locator("tr", { hasText: productName });
    await row.getByRole("button", { name: /excluir/i }).click();

    // Confirm deletion dialog
    await expect(
      page.getByRole("dialog", { name: /excluir produto/i })
    ).toBeVisible({ timeout: 5_000 });
    await page.getByRole("button", { name: /^excluir$/i }).last().click();

    // Product should no longer be visible
    await expect(page.getByText(productName)).not.toBeVisible({ timeout: 10_000 });
  });

  test("admin can delete a category without products", async ({ page }) => {
    await page.goto("/categories");
    await page.getByRole("button", { name: /nova categoria/i }).click();
    await expect(page.getByRole("dialog")).toBeVisible({ timeout: 5_000 });
    const categoryName = `E2E CatToDelete ${Date.now()}`;
    await page.locator("#name").fill(categoryName);
    await page.getByRole("button", { name: /^criar$/i }).click();
    await expect(page.getByRole("dialog")).not.toBeVisible({ timeout: 10_000 });
    await expect(page.getByText(categoryName)).toBeVisible({ timeout: 10_000 });

    // Click "Excluir" on the category row
    const row = page.locator("tr", { hasText: categoryName });
    await row.getByRole("button", { name: /excluir/i }).click();

    // Confirm deletion
    await expect(
      page.getByRole("dialog", { name: /excluir categoria/i })
    ).toBeVisible({ timeout: 5_000 });
    await page.getByRole("button", { name: /^excluir$/i }).last().click();

    // Category should be removed from the list
    await expect(page.getByText(categoryName)).not.toBeVisible({ timeout: 10_000 });
  });
});
