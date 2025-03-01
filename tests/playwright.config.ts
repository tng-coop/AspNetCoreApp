import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './',  // ✅ updated to point directly to current dir
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'list',
  use: {
    baseURL: 'https://localhost:5001', // recommended best practice
    ignoreHTTPSErrors: true, // enables self-signed HTTPS
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
});
