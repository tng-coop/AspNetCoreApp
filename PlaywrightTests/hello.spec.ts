import { test, expect } from '@playwright/test';

test('Weather page', async ({ page }) => {
  await page.goto('/weather');
  await expect(page.getByText('This component demonstrates')).toHaveCount(1)
});
