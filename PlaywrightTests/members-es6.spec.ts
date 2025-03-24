import { test, expect } from '@playwright/test';

test('Members page (ES6 fetch) works with JWT authentication', async ({ page }) => {
  // First, perform API login and store JWT token
  const loginResponse = await page.request.post('/api/login', {
    data: { email: 'admin@example.com', password: 'SecureP@ssword123!' }
  });
  expect(loginResponse.ok()).toBeTruthy();

  const { token } = await loginResponse.json();
  expect(token).toBeDefined();

  // Store JWT in localStorage before visiting page
  await page.addInitScript((jwtToken) => {
    localStorage.setItem('token', jwtToken);
  }, token);

  // Navigate to MembersES page
  await page.goto('/MembersES');

  await page.fill('input[name="Input.Email"]', "admin@example.com");
  await page.fill('input[name="Input.Password"]', "SecureP@ssword123!");
  await page.check('input[name="Input.RememberMe"]');

  // Wait explicitly for navigation after submit
  await page.click("button#login-submit"),

  
  // Click button to fetch members
  await page.click('#fetch-members-btn');

  // Wait for members list to populate
  await page.waitForSelector('#members-list li');

  // Verify that members are displayed
  const memberItems = await page.$$eval('#members-list li', items => items.map(i => i.textContent));
  expect(memberItems.length).toBeGreaterThan(0);
  expect(memberItems).toContain('Simon Peter (simon.peter@example.com)');
});
