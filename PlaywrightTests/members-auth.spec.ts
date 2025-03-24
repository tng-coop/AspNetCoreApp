import { test, expect, request as playwrightRequest } from '@playwright/test';

test('Members API - JWT Auth', async ({ request }) => {
  const loginResponse = await request.post('/api/login', {
    data: {
      email: 'admin@example.com',
      password: 'SecureP@ssword123!'
    }
  });

  expect(loginResponse.ok()).toBeTruthy();
  const { token } = await loginResponse.json();
  expect(token).toBeDefined();

  const apiResponse = await request.get('/api/members', {
    headers: {
      Authorization: `Bearer ${token}`
    }
  });

  expect(apiResponse.ok()).toBeTruthy();
  const members = await apiResponse.json();
  expect(Array.isArray(members)).toBe(true);
  expect(members.length).toBeGreaterThan(0);
});

test('Members API - Unauthorized access', async ({ request, browser }) => {
    // New isolated context
    const context = await browser.newContext();
    const apiRequestContext = context.request;
  
    const response = await apiRequestContext.get('/api/members');
    expect(response.status()).toBe(401);
  
    await context.close();
  });
  