const { test, expect } = require("@playwright/test");

test.describe("MembersJQ Page", () => {
  test.beforeEach(async ({ page }) => {
    // Log in via API first and store JWT token in localStorage
    const response = await page.request.post("/api/login", {
      data: { email: "admin@example.com", password: "SecureP@ssword123!" },
    });

    const data = await response.json();
    await page.addInitScript((token) => {
      localStorage.setItem("token", token);
    }, data.token);
  });

  test("should fetch and display members using jQuery", async ({ page }) => {
    await page.goto("/MembersJQ");

    await page.fill('input[name="Input.Email"]', "admin@example.com");
    await page.fill('input[name="Input.Password"]', "SecureP@ssword123!");
    await page.check('input[name="Input.RememberMe"]');

    // Wait explicitly for navigation after submit
    await page.click("button#login-submit"),
      // Click the button to fetch members
      await page.waitForFunction(() => {
        // Ensure jQuery has loaded on the page
        const jqueryLoaded = !!window.jQuery;
    
        // Check if the button element exists in DOM
        const btn = document.getElementById('fetch-members-btn');
        const btnExists = !!btn;
    
        // Check if jQuery has attached a click handler to the button
        const clickHandlerAttached = btnExists && jqueryLoaded && !!jQuery._data(btn, 'events')?.click;
    
        // Proceed only if all conditions are met
        return jqueryLoaded && btnExists && clickHandlerAttached;
    });
    
      await page.click('#fetch-members-btn');
      
    await expect(page.getByText("Paul Last (paul@example.com)")).toHaveCount(1);
  });
});
