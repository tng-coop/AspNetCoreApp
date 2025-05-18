import { test, expect } from '@playwright/test';

test('Login exists', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByText('Login')).toHaveCount(1)
});

