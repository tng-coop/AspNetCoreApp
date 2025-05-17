import { test, expect } from '@playwright/test';

test('Login exists', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByText('Login')).toHaveCount(1)
});

test('Cert exists', async ({ page }) => {
  await page.goto('http://aspnet.lan/cert/');
  await expect(page.getByText('aspnet.lan-ca.crt')).toHaveCount(1)
});
