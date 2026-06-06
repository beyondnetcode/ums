import { test, expect } from '@playwright/test';

test.describe('Navigation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/correo electrónico/i).fill('admin@ums.local');
    await page.getByLabel(/contraseña/i).fill('Admin@123');
    await page.getByRole('button', { name: /ingresar/i }).click();
    await expect(page).toHaveURL(/.*\/tenants/);
  });

  test('should display navigation rail with all sections', async ({ page }) => {
    await expect(page.getByText(/(identidad|identity)/i).first()).toBeVisible();
    await expect(page.getByText(/(autorización|authorization)/i).first()).toBeVisible();
    await expect(page.getByText(/(configuración|configuration)/i).first()).toBeVisible();
  });

  test('should navigate to Tenants page', async ({ page }) => {
    await page.getByRole('button', { name: /tenant/i }).first().click();
    await expect(page).toHaveURL(/\/tenants/);
  });

  test('should navigate to Users page', async ({ page }) => {
    await page.getByRole('button', { name: /(user accounts|cuentas de usuario)/i }).first().click();
    await expect(page).toHaveURL(/\/users/);
  });

  test('should navigate to System Suites page', async ({ page }) => {
    await page.getByRole('button', { name: /(system suites|suites del sistema)/i }).first().click();
    await expect(page).toHaveURL(/\/system-suites/);
  });

  test('should navigate to Permission Templates page', async ({ page }) => {
    await page.getByRole('button', { name: /(permission templates|plantillas de permisos)/i }).first().click();
    await expect(page).toHaveURL(/\/permission-templates/);
  });

  test('should navigate to Profiles page', async ({ page }) => {
    await page.getByRole('button', { name: /(authorization profiles|perfiles de autorización)/i }).first().click();
    await expect(page).toHaveURL(/\/profiles/);
  });

  test('should navigate to Feature Flags page', async ({ page }) => {
    await page.getByRole('button', { name: /(feature flags|flags|banderas)/i }).first().click();
    await expect(page).toHaveURL(/\/feature-flags/);
  });

  test('should navigate to Profile page', async ({ page }) => {
    await page.getByRole('button', { name: /(profile stats|estadísticas de perfil)/i }).first().click();
    await expect(page).toHaveURL(/\/profile/);
  });

  test('should collapse and expand navigation sections', async ({ page }) => {
    const identitySection = page.getByRole('button', { name: /(identidad|identity)/i }).first();
    await identitySection.click();
    await expect(page.getByRole('button', { name: /tenant/i }).first()).not.toBeVisible();
    await identitySection.click();
    await expect(page.getByRole('button', { name: /tenant/i }).first()).toBeVisible();
  });

  test('should toggle navigation rail collapsed state', async ({ page }) => {
    const menuButton = page.locator('button[aria-label="Toggle navigation"]');
    await menuButton.click();
    await expect(page.locator('aside')).toHaveClass(/w-20/);
    await menuButton.click();
    await expect(page.locator('aside')).toHaveClass(/w-64/);
  });

  test('should highlight active navigation item', async ({ page }) => {
    await page.getByRole('button', { name: /tenant/i }).click();
    const tenantButton = page.getByRole('button', { name: /tenant/i });
    await expect(tenantButton).toHaveClass(/bg-m3-primary-container/);
  });
});
