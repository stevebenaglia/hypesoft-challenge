import { Page, expect } from "@playwright/test";

/**
 * Performs a full Keycloak login flow:
 * 1. Goes to /auth/signin
 * 2. Clicks the "Entrar com Keycloak" button
 * 3. Fills credentials in Keycloak's form
 * 4. Submits and waits for redirect back to the app
 */
export async function login(
  page: Page,
  username = "admin",
  password = "admin"
) {
  await page.goto("/auth/signin");
  await page.getByRole("button", { name: /entrar com keycloak/i }).click();

  // Wait for Keycloak login page
  await expect(page).toHaveURL(/localhost:8080|keycloak/, { timeout: 20_000 });
  await page.locator("#username").fill(username);
  await page.locator("#password").fill(password);
  await page.locator("#kc-login").click();

  // Wait until redirected back to the app (not on auth/signin or keycloak)
  await page.waitForURL(
    (url) =>
      !url.href.includes("/auth/signin") &&
      !url.href.includes("localhost:8080") &&
      !url.href.includes("keycloak"),
    { timeout: 20_000 }
  );
}
