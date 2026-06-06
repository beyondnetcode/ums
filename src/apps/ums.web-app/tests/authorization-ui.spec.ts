import { test, expect } from '@playwright/test';

test.describe('Dynamic Authorization UI Tests', () => {
  test('Admin user should have Agregar button enabled', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/correo electrónico/i).fill('admin@ums.local');
    await page.getByLabel(/contraseña/i).fill('Admin@123');
    await page.click('button[type="submit"]');

    // Wait for tenants
    await page.waitForURL('**/tenants', { timeout: 10000 });

    // Check if the add button is visible and enabled
    const addButton = page.locator('button[title="Agregar"]');
    await expect(addButton).toBeVisible();
    await expect(addButton).not.toBeDisabled();
  });

  test('Tenant Supervisor should see Access Denied on Tenants page', async ({ page }) => {
    await page.goto('/login');
    await page.getByText(/INTERNAL_ADMIN/i).first().click();
    await page.getByText(/Ransa Comercial S.A./i).click();
    await page.getByLabel(/correo electrónico/i).fill('gerente.operaciones@ransa.pe');
    await page.getByLabel(/contraseña/i).fill('Admin@123');
    await page.click('button[type="submit"]');

    // Wait for tenants
    await page.waitForURL('**/tenants', { timeout: 10000 });

    // Should show access denied
    await expect(page.getByText(/Acceso Denegado/i)).toBeVisible();
  });
});
