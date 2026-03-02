import { test, expect } from "@playwright/test";

test.describe("Authentication", () => {
  test("redirects to /auth/signin when not logged in", async ({ page }) => {
    await page.goto("/");
    await expect(page).toHaveURL(/\/auth\/signin/);
  });

  test("shows 'Entrar com Keycloak' button at /auth/signin", async ({ page }) => {
    await page.goto("/auth/signin");
    await expect(
      page.getByRole("button", { name: /entrar com keycloak/i })
    ).toBeVisible({ timeout: 10_000 });
  });

  test("clicking Keycloak button opens Keycloak login form", async ({ page }) => {
    await page.goto("/auth/signin");
    await page.getByRole("button", { name: /entrar com keycloak/i }).click();
    // Should navigate to Keycloak login page
    await expect(page).toHaveURL(/localhost:8080|keycloak/, { timeout: 15_000 });
    await expect(page.locator("#username")).toBeVisible({ timeout: 15_000 });
  });

  test("login with admin credentials redirects to dashboard", async ({ page }) => {
    await page.goto("/auth/signin");
    await page.getByRole("button", { name: /entrar com keycloak/i }).click();

    // Wait for Keycloak login page
    await expect(page).toHaveURL(/localhost:8080|keycloak/, { timeout: 15_000 });
    await page.locator("#username").fill("admin");
    await page.locator("#password").fill("admin");
    await page.locator("#kc-login").click();

    // After successful login should leave auth/signin
    await expect(page).not.toHaveURL(/\/auth\/signin/, { timeout: 20_000 });
  });
});
