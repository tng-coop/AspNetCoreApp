import { test, expect } from '@playwright/test';

test('Hello John', async ({ page }) => {
  await page.goto('https://localhost:5001/Hello/John');
  await expect(page.getByRole('heading', { name: 'Hello, John!' })).toHaveCount(1)
});

test('WeatherForecast JSON API', async ({ request }) => {
  const response = await request.get('/weatherforecast');
  expect(response.ok()).toBeTruthy();
  expect(response.headers()['content-type']).toContain('application/json');

  const forecasts = await response.json();
  
  expect(Array.isArray(forecasts)).toBe(true);
  expect(forecasts.length).toBeGreaterThan(0);

  for (const forecast of forecasts) {
    expect(forecast).toHaveProperty('date');
    expect(forecast).toHaveProperty('temperatureC');
    expect(forecast).toHaveProperty('temperatureF');
    expect(forecast).toHaveProperty('summary');
    expect(typeof forecast.temperatureC).toBe('number');
    expect(typeof forecast.temperatureF).toBe('number');
    expect(typeof forecast.summary).toBe('string');
  }
});