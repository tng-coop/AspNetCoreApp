import { test, expect } from '@playwright/test';

test('Cookie-based login works correctly', async ({ page }) => {
  await page.goto('/Identity/Account/Login');

  await page.fill('input[name="Input.Email"]', 'admin@example.com');
  await page.fill('input[name="Input.Password"]', 'SecureP@ssword123!');
  await page.check('input[name="Input.RememberMe"]');

  // Wait explicitly for navigation after submit
  await page.click('button#login-submit'),

  // Verify successful login (ensure elements exist)
  await expect(page.locator('button#logout')).toBeVisible();
  await expect(page.locator('a#manage')).toContainText('admin@example.com');
});

test('Cookie-based login fails with invalid credentials', async ({ page }) => {
  await page.goto('/Identity/Account/Login');

  // Ensure no error initially
  await expect(page.getByText('Invalid login attempt.')).toHaveCount(0);

  await page.fill('input[name="Input.Email"]', 'paul@example.com');
  await page.fill('input[name="Input.Password"]', 'WrongPassword!');

  await Promise.all([
    page.waitForResponse(response =>
      response.url().includes('/Identity/Account/Login') && response.status() === 200
    ),
    page.click('button#login-submit'),
  ]);

  // Verify error message is displayed
  await expect(page.getByText('Invalid login attempt.')).toBeVisible();
});
