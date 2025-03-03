import { test, expect } from '@playwright/test';

test('Cookie-based login works correctly', async ({ page }) => {
  await page.goto('/Identity/Account/Login');

  await page.fill('input[name="Input.Email"]', 'admin@example.com');
  await page.fill('input[name="Input.Password"]', 'SecureP@ssword123!');
  await page.check('input[name="Input.RememberMe"]');

  await page.click('button#login-submit');

  // Verify successful login
  await expect(page.locator('#logout')).toBeVisible();
  await expect(page.locator('a#manage')).toContainText('admin@example.com');
});

test('Cookie-based login fails with invalid credentials', async ({ page }) => {
  await page.goto('/Identity/Account/Login');

  await expect(page.getByText('Invalid login attempt.')).toHaveCount(0)
  await page.fill('input[name="Input.Email"]', 'admin@example.com');
  await page.fill('input[name="Input.Password"]', 'WrongPassword!');
  await page.click('button#login-submit');

  // Verify error message
  await expect(page.getByText('Invalid login attempt.')).toHaveCount(1)
});
