import { dirname, resolve } from 'node:path'
import { fileURLToPath } from 'node:url'
import { defineConfig, devices } from '@playwright/test'

const rootDir = dirname(fileURLToPath(import.meta.url))

const API_PORT = 5199
const WEB_PORT = 5174
const API_URL = `http://localhost:${API_PORT}`
const WEB_URL = `http://localhost:${WEB_PORT}`

const e2eDbPath = resolve(rootDir, '.playwright/e2e.db')

export default defineConfig({
  testDir: './e2e',

  fullyParallel: false,
  workers: 1,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  reporter: process.env.CI ? [['github'], ['html', { open: 'never' }]] : [['list']],

  use: {
    baseURL: WEB_URL,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
  },

  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],

  webServer: [
    {

      command: `sh -c "rm -f '${e2eDbPath}' '${e2eDbPath}-shm' '${e2eDbPath}-wal' && dotnet run --project ../backend/src/TestR.Api --no-launch-profile --urls ${API_URL}"`,
      url: `${API_URL}/health`,
      cwd: rootDir,
      env: {
        ASPNETCORE_ENVIRONMENT: 'Development',

        ConnectionStrings__Default: `Data Source=${e2eDbPath}`,
        Auth__Enabled: 'false',
      },

      reuseExistingServer: false,
      stdout: 'pipe',
      stderr: 'pipe',
      timeout: 120_000,
    },
    {
      command: `npm run dev -- --port ${WEB_PORT} --strictPort`,
      url: WEB_URL,
      cwd: rootDir,
      env: { VITE_API_PROXY_TARGET: API_URL },
      reuseExistingServer: false,
      timeout: 120_000,
    },
  ],
})
