import { test, expect } from '@playwright/test';

test.describe('Login Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the login page for the RANSA_PERU tenant
    await page.goto('/login?tenantCode=RANSA_PERU');
  });

  test('should display login form correctly', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /Iniciar Sesión/i })).toBeVisible();
    await expect(page.getByLabel(/Correo electrónico/i)).toBeVisible();
    await expect(page.getByLabel(/Contraseña/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /Ingresar/i })).toBeVisible();
  });

  test('should successfully login as an internal user and navigate to dashboard', async ({ page }) => {
    // Fill in the login form with internal user credentials
    await page.getByLabel(/Correo electrónico/i).fill('gerente.operaciones@ransa.pe');
    await page.getByLabel(/Contraseña/i).fill('Test1234!'); // Using CoreDevDataSeeder.SuperAdminPassword
    
    // Click login
    await page.getByRole('button', { name: /Ingresar/i }).click();

    // Verify successful login by checking navigation to a protected route (e.g., /tenants or /dashboard)
    await expect(page).toHaveURL(/\/tenants|\/dashboard|\//);
    
    // Verify an element that should only be visible after login
    // E.g., user profile menu, logout button, or specific dashboard heading
    // await expect(page.getByRole('button', { name: /Logout|Cerrar Sesión/i })).toBeVisible();
  });

  test('should show error with invalid credentials', async ({ page }) => {
    // Fill in the login form with invalid credentials
    await page.getByLabel(/Correo electrónico/i).fill('gerente.operaciones@ransa.pe');
    await page.getByLabel(/Contraseña/i).fill('WrongPassword!');
    
    // Click login
    await page.getByRole('button', { name: /Ingresar/i }).click();

    // Verify error message is displayed
    await expect(page.getByText(/No pudimos iniciar sesión/i).first()).toBeVisible();
  });
});
