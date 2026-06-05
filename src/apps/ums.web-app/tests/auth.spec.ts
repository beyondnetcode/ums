import { test, expect } from '@playwright/test';

test.describe('Authentication Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
  });

  test('should display login page with all elements', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByText(/user management system/i)).toBeVisible();
    await expect(page.getByText(/iniciar sesión/i)).toBeVisible();
    await expect(page.getByText(/Tenant/i)).toBeVisible();
    await expect(page.getByLabel(/correo electrónico/i)).toBeVisible();
    await expect(page.getByLabel(/contraseña/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /ingresar/i })).toBeVisible();
  });

  test('should show validation errors for empty fields', async ({ page }) => {
    await page.getByRole('button', { name: /ingresar/i }).click();
    await expect(page.getByText(/ingrese su correo electrónico/i)).toBeVisible();
  });

  test('should show error for invalid credentials', async ({ page }) => {
    await page.getByLabel(/correo electrónico/i).fill('wrong@example.com');
    await page.getByLabel(/contraseña/i).fill('wrongpass');
    await page.getByRole('button', { name: /ingresar/i }).click();
    await expect(page.getByText(/no pudimos iniciar sesión/i)).toBeVisible();
  });

  test('should login successfully with valid credentials', async ({ page }) => {
    await page.getByLabel(/usuario/i).fill('operador_callao');
    await page.getByLabel(/contraseña/i).fill('Admin@123');
    await page.getByRole('button', { name: /ingresar/i }).click();
    await expect(page).toHaveURL(/\/tenants/);
  });

  test('should redirect to login when accessing protected route', async ({ page }) => {
    await page.goto('/tenants');
    await expect(page).toHaveURL(/.*\/login\?redirect=%2Ftenants/);
  });

  test('should show session expired message when redirected from protected route', async ({
    page,
  }) => {
    await page.goto('/tenants');
    await expect(page.getByText(/su sesión ha expirado/i)).toBeVisible();
  });

  test('should redirect to originally requested page after login', async ({ page }) => {
    await page.goto('/users');
    await expect(page).toHaveURL(/\/login/);
    await page.getByLabel(/usuario/i).fill('operador_callao');
    await page.getByLabel(/contraseña/i).fill('Admin@123');
    await page.getByRole('button', { name: /ingresar/i }).click();
    await expect(page).toHaveURL(/\/users/);
  });

  test('should lock account after 5 failed attempts', async ({ page }) => {
    for (let i = 0; i < 5; i++) {
      await page.getByLabel(/usuario/i).fill('invalid_user');
      await page.getByLabel(/contraseña/i).fill('wrong_password');
      await page.getByRole('button', { name: /ingresar/i }).click();
      await page.waitForTimeout(100);
    }
    await expect(page.getByText(/demasiados intentos/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /bloqueado/i })).toBeVisible();
  });
});

test.describe('Logout Flow', () => {
  test('should logout and redirect to login', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/usuario/i).fill('operador_callao');
    await page.getByLabel(/contraseña/i).fill('Admin@123');
    await page.getByRole('button', { name: /ingresar/i }).click();
    await expect(page).toHaveURL(/\/tenants/);
    await page.locator('button[aria-label="Log out"]').click();
    await expect(page).toHaveURL(/\/login/);
  });

  test('should clear session after logout', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/usuario/i).fill('operador_callao');
    await page.getByLabel(/contraseña/i).fill('Admin@123');
    await page.getByRole('button', { name: /ingresar/i }).click();
    await expect(page).toHaveURL(/\/tenants/);
    await page.locator('button[aria-label="Log out"]').click();
    await page.goto('/tenants');
    await expect(page).toHaveURL(/\/login/);
  });
});
