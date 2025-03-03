import { test, expect } from '@playwright/test';

test('Admin Login API', async ({ request }) => {
  const response = await request.post('/api/login', {
    data: {
      email: 'admin@example.com',
      password: 'SecureP@ssword123!'
    }
  });

  expect(response.status()).toBe(200);
  const json = await response.json();
  expect(json.message).toBe('Login successful');
});
