import { test, expect } from '@playwright/test';

test('Admin Login API (correct credentials)', async ({ request }) => {
  const response = await request.post('/api/login', {
    data: {
      email: 'admin@example.com',
      password: 'SecureP@ssword123!'
    }
  });

  expect(response.status()).toBe(200);
  const json = await response.json();
  expect(json.message).toBe('Login successful');
  expect(json.token).toBeDefined();
});

test('Admin Login API (wrong password)', async ({ request }) => {
  const response = await request.post('/api/login', {
    data: {
      email: 'simon.peter@example.com',
      password: 'WrongPassword!'
    }
  });

  expect(response.status()).toBe(401);
});
