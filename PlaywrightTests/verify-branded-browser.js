const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({
    channel: 'chrome', // or 'msedge', etc.
    headless: false
  });

  // Correct way to check executable path
  const executablePath = browser.browserType().executablePath();
  console.log('Browser executable path:', executablePath);

  const page = await browser.newPage();
  await page.goto('https://example.com');

  await browser.close();
})();

