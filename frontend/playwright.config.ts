import path from 'node:path'
import { defineConfig, devices } from '@playwright/test'

const frontendDir = import.meta.dirname
const repoRoot = path.resolve(frontendDir, '..')

// Браузерный тест поднимает реальные бекенд и Vite: моков API нет.
export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  reporter: 'list',
  use: {
    baseURL: 'http://127.0.0.1:5173',
    trace: 'on-first-retry',
  },
  // Локально и на CI используется установленный Chrome: скачивание
  // браузеров Playwright недоступно в этом окружении.
  projects: [
    { name: 'chrome', use: { ...devices['Desktop Chrome'], channel: 'chrome' } },
  ],
  webServer: [
    {
      command:
        'dotnet run --project backend/CallCalendar --urls http://127.0.0.1:5150',
      cwd: repoRoot,
      url: 'http://127.0.0.1:5150/health',
      reuseExistingServer: !process.env.CI,
      timeout: 120_000,
    },
    {
      command: 'pnpm dev --host 127.0.0.1 --port 5173 --strictPort',
      cwd: frontendDir,
      url: 'http://127.0.0.1:5173',
      reuseExistingServer: !process.env.CI,
      timeout: 120_000,
    },
  ],
})
