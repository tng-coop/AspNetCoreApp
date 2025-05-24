import { test, expect } from '@playwright/test';

test('Login exists', async ({ page }) => {
  await page.goto('/_login');
  await expect(page).toHaveTitle('Log in')
});

