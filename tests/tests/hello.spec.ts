import { test, expect } from '@playwright/test';

test('Hello John', async ({ page }) => {
  await page.goto('https://localhost:5001/Hello/John');
  await expect(page.getByRole('heading', { name: 'Hello, John!' })).toHaveCount(1)
});