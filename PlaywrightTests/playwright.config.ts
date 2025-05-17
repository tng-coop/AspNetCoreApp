import { defineConfig, devices } from '@playwright/test';
import * as cp from 'child_process';
import * as fs from 'fs';
import * as path from 'path';

const projectFolder = path.resolve(__dirname, '../BlazorWebApp');
const csprojFile    = path.join(projectFolder, 'BlazorWebApp.csproj');

function loadUserSecretsViaCli(): Record<string, string> {
  if (!fs.existsSync(csprojFile)) {
    console.warn(`.csproj not found at ${csprojFile}; skipping user-secrets CLI.`); 
    return {};
  }

  let output: string;
  try {
    // run the CLI and capture stdout
    output = cp.execSync(
      `dotnet user-secrets list --project "${csprojFile}"`,
      { encoding: 'utf-8' }
    );
  } catch (e) {
    console.warn('Failed to run `dotnet user-secrets list`:', e);
    return {};
  }

  return output
    .split(/\r?\n/)
    .filter(Boolean)
    .reduce((acc, line) => {
      const [keyPart, ...rest] = line.split('=');
      if (!rest.length) return acc;
      const key = keyPart.trim();
      const val = rest.join('=').trim();
      acc[key] = val;
      return acc;
    }, {} as Record<string, string>);
}

const secrets = loadUserSecretsViaCli();

const baseURL =
  process.env.Kestrel__Endpoints__Https__Url              // 1) env-override
  || secrets['Kestrel:Endpoints:Https:Url']               // 2) from `dotnet user-secrets list`
  || 'http://aspn5et.lan:5000';                            // 3) fallback

export default defineConfig({
  testDir: './',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [['list'], ['html']],
  use: {
    baseURL,
    ignoreHTTPSErrors: true,
    trace: 'on',
  },
  projects: [
    {
      name: 'chrome',
      use: {
        ...devices['Desktop Chrome'],
        channel: 'chrome',
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
