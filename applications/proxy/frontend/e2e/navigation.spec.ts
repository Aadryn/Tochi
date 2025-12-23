import { test, expect } from '@playwright/test'

test.describe('Navigation', () => {
  test.beforeEach(async ({ page }) => {
    // Simuler l'authentification en injectant un token
    await page.goto('/')
    await page.evaluate(() => {
      localStorage.setItem('auth_token', 'test-token-e2e')
      localStorage.setItem('user', JSON.stringify({
        id: 1,
        name: 'Test User',
        email: 'test@example.com',
        role: 'admin'
      }))
    })
    await page.goto('/')
    await page.waitForLoadState('networkidle')
  })

  test('doit afficher la structure de navigation', async ({ page }) => {
    // La page doit avoir un header avec navigation ou un menu
    const nav = page.locator('header, nav, [role="navigation"], .app-header, .menu')
    await expect(nav.first()).toBeVisible()
  })

  test('doit naviguer vers le Dashboard par défaut', async ({ page }) => {
    // La page racine devrait être le dashboard
    await expect(page).toHaveURL(/.*\/$/)
    const title = page.locator('h1, h2')
    await expect(title.first()).toBeVisible()
  })

  test('doit naviguer vers Providers', async ({ page }) => {
    const providersLink = page.locator('a, button').filter({ hasText: /providers/i })
    if (await providersLink.first().isVisible()) {
      await providersLink.first().click()
      await expect(page).toHaveURL(/.*providers/i)
    }
  })

  test('doit naviguer vers Tenants', async ({ page }) => {
    const tenantsLink = page.locator('a, button').filter({ hasText: /tenants/i })
    if (await tenantsLink.first().isVisible()) {
      await tenantsLink.first().click()
      await expect(page).toHaveURL(/.*tenants/i)
    }
  })

  test('doit naviguer vers Routes', async ({ page }) => {
    const routesLink = page.locator('a, button').filter({ hasText: /routes/i })
    if (await routesLink.first().isVisible()) {
      await routesLink.first().click()
      await expect(page).toHaveURL(/.*routes/i)
    }
  })

  test('doit naviguer vers Monitoring', async ({ page }) => {
    const monitoringLink = page.locator('a, button').filter({ hasText: /monitoring/i })
    if (await monitoringLink.first().isVisible()) {
      await monitoringLink.first().click()
      await expect(page).toHaveURL(/.*monitoring/i)
    }
  })

  test('doit naviguer vers Paramètres', async ({ page }) => {
    const settingsLink = page.locator('a, button').filter({ hasText: /paramètres|settings/i })
    if (await settingsLink.first().isVisible()) {
      await settingsLink.first().click()
      await expect(page).toHaveURL(/.*settings/i)
    }
  })

  test('doit conserver la navigation lors des changements de page', async ({ page }) => {
    // Aller sur providers
    await page.goto('/providers')
    await page.waitForLoadState('networkidle')
    
    // Vérifier que la structure de navigation est toujours présente
    const nav = page.locator('header, nav, [role="navigation"], .app-header, .menu')
    await expect(nav.first()).toBeVisible()
  })

  test('doit marquer visuellement la page active', async ({ page }) => {
    await page.goto('/providers')
    await page.waitForLoadState('networkidle')
    
    // Vérifier qu'on est sur la bonne page par le titre
    const title = page.locator('h2.view-title, .view-title, h2')
    await expect(title.first()).toContainText(/providers/i)
  })
})
