import { defineConfig, devices } from '@playwright/test'

/**
 * Configuration Playwright pour les tests E2E
 * @see https://playwright.dev/docs/test-configuration
 */
export default defineConfig({
  testDir: './e2e',
  /* Temps maximum par test */
  timeout: 30 * 1000,
  /* Temps maximum pour expect() */
  expect: {
    timeout: 5000,
  },
  /* Exécution en parallèle */
  fullyParallel: true,
  /* Fail si test.only reste dans le code */
  forbidOnly: !!process.env.CI,
  /* Nombre de retries */
  retries: process.env.CI ? 2 : 0,
  /* Limiter les workers en CI */
  workers: process.env.CI ? 1 : undefined,
  /* Reporter */
  reporter: [['html', { open: 'never' }]],
  /* Configuration partagée */
  use: {
    /* URL de base pour les actions comme `await page.goto('/')` */
    baseURL: process.env.PLAYWRIGHT_BASE_URL || 'http://localhost:3000',
    /* Collecter une trace lors d'un échec */
    trace: 'on-first-retry',
    /* Screenshots lors d'un échec */
    screenshot: 'only-on-failure',
  },
  /* Configuration des projets (navigateurs) */
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
  ],
  /* Démarrer le serveur de dev avant les tests */
  webServer: {
    command: 'npm run dev',
    url: process.env.PLAYWRIGHT_BASE_URL || 'http://localhost:3000',
    reuseExistingServer: !process.env.CI,
    timeout: 120 * 1000,
  },
})
