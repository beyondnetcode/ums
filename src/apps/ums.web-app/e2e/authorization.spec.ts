import { test, expect } from '@playwright/test';

test.describe('Authorization Flow', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login');
  });

  test('should display login form', async ({ page }) => {
    await expect(page.getByRole('heading', { name: /sign in/i })).toBeVisible();
    await expect(page.getByLabel(/email/i)).toBeVisible();
    await expect(page.getByLabel(/password/i)).toBeVisible();
  });

  test('should navigate to tenants after login', async () => {
    // TODO: Implement with real auth flow
    test.skip();
  });

  test('should display authorization nav items', async () => {
    // TODO: Verify Tenants, Users, Delegations appear in sidebar
    test.skip();
  });

  test('should require authentication for protected routes', async ({ page }) => {
    await page.goto('/tenants');
    // TODO: Verify redirect to login or 401 response
    test.skip();
  });
});
