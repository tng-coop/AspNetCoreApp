import { test, expect } from '@playwright/test';

test('Admin Login', async ({ page }) => {
  await page.goto('/Identity/Account/Login');
  await page.fill('input[name="Input.Email"]', 'admin@example.com');
  await page.fill('input[name="Input.Password"]', 'SecureP@ssword123!');
  await page.click('button[type="submit"]');

  await expect(page).toHaveURL('/');
  await expect(page.getByText('admin@example.com')).toBeVisible();
});
