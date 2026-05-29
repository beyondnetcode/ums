import { test, expect } from '@playwright/test';

test.describe('Navigation', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel(/usuario/i).fill('operador_callao');
    await page.getByLabel(/contraseña/i).fill('Admin@123');
    await page.getByRole('button', { name: /ingresar/i }).click();
    await expect(page).toHaveURL(/\/tenants/);
  });

  test('should display navigation rail with all sections', async ({ page }) => {
    await expect(page.getByText(/identity context/i)).toBeVisible();
    await expect(page.getByText(/authorization context/i)).toBeVisible();
    await expect(page.getByText(/system diagnostics/i)).toBeVisible();
  });

  test('should navigate to Tenants page', async ({ page }) => {
    await page.getByRole('button', { name: /tenant/i }).click();
    await expect(page).toHaveURL(/\/tenants/);
    await expect(page.getByRole('heading', { name: /tenant/i })).toBeVisible();
  });

  test('should navigate to Users page', async ({ page }) => {
    await page.getByRole('button', { name: /user accounts/i }).click();
    await expect(page).toHaveURL(/\/users/);
    await expect(page.getByRole('heading', { name: /user account/i })).toBeVisible();
  });

  test('should navigate to System Suites page', async ({ page }) => {
    await page.getByRole('button', { name: /system suites/i }).click();
    await expect(page).toHaveURL(/\/system-suites/);
  });

  test('should navigate to Permission Templates page', async ({ page }) => {
    await page.getByRole('button', { name: /permission templates/i }).click();
    await expect(page).toHaveURL(/\/permission-templates/);
  });

  test('should navigate to Profiles page', async ({ page }) => {
    await page.getByRole('button', { name: /profiles/i }).click();
    await expect(page).toHaveURL(/\/profiles/);
  });

  test('should navigate to Feature Flags page', async ({ page }) => {
    await page.getByRole('button', { name: /feature flags/i }).click();
    await expect(page).toHaveURL(/\/feature-flags/);
  });

  test('should navigate to Profile page', async ({ page }) => {
    await page.getByRole('button', { name: /profile stats/i }).click();
    await expect(page).toHaveURL(/\/profile/);
  });

  test('should collapse and expand navigation sections', async ({ page }) => {
    const identitySection = page.getByRole('button', { name: /identity context/i });
    await identitySection.click();
    await expect(page.getByRole('button', { name: /tenant/i })).not.toBeVisible();
    await identitySection.click();
    await expect(page.getByRole('button', { name: /tenant/i })).toBeVisible();
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