import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './',  // ✅ points directly to current directory
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [['list'], ['html']],  // ✅ includes both list and html reporters
  use: {
    baseURL: process.env.Kestrel__Endpoints__Https__Url || 'https://aspnet.test:5001', // recommended best practice
    ignoreHTTPSErrors: true, // enables self-signed HTTPS
    trace: 'on',
  },
  projects: [
    {
      name: 'chrome',
      use: { 
        ...devices['Desktop Chrome'],
        channel: 'chrome', // ✅ explicitly uses branded Chrome browser
        launchOptions: {
          args: [
            '--ozone-platform=wayland',
            '--enable-features=UseOzonePlatform',
          ],
        },
      },        
    },
  ],
});
