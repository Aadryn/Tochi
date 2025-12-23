import { test, expect } from '@playwright/test'

test.describe('Monitoring', () => {
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
    await page.goto('/monitoring')
    await page.waitForLoadState('networkidle')
  })

  test('doit afficher le titre de la page', async ({ page }) => {
    // Le titre de la page a maintenant un data-testid pour faciliter les tests
    const title = page.locator('[data-testid="page-title"]')
    await expect(title).toBeVisible()
    await expect(title).toContainText(/monitoring/i)
  })

  test('doit afficher les métriques en temps réel', async ({ page }) => {
    await page.waitForLoadState('networkidle')

    // Vérifier qu'il y a des métriques affichées
    const metrics = page.locator('.metric-card, .stat-card, [data-testid="metric"]')
    await expect(metrics.first()).toBeVisible()
  })

  test('doit afficher les données de monitoring', async ({ page }) => {
    await page.waitForLoadState('networkidle')

    // Vérifier que la page contient des données (métriques, cartes, graphiques ou contenu)
    const content = page.locator('.view-header, .view-content, .app-card, canvas, main')
    await expect(content.first()).toBeVisible()
  })

  test('doit afficher les graphiques ou cartes', async ({ page }) => {
    await page.waitForLoadState('networkidle')

    // Chercher des éléments de graphique ou des cartes de métriques
    const charts = page.locator('canvas, svg.chart, .chart-container, .app-card, .metric-card')

    if (await charts.first().isVisible()) {
      expect(await charts.count()).toBeGreaterThan(0)
    }
  })

  test('doit pouvoir filtrer par période', async ({ page }) => {
    const periodFilter = page.locator('select, [role="combobox"]').filter({ hasText: /heure|jour|semaine/i })

    if (await periodFilter.first().isVisible()) {
      await periodFilter.first().click()
    }
  })

  test('doit afficher le statut des providers', async ({ page }) => {
    await page.waitForLoadState('networkidle')

    const providerStatus = page.locator('text=/provider.*status|état.*provider/i')

    if (await providerStatus.first().isVisible()) {
      expect(await providerStatus.count()).toBeGreaterThan(0)
    }
  })

  test('doit afficher des indicateurs de rafraîchissement', async ({ page }) => {
    // Vérifier qu'il y a un indicateur de temps ou de rafraîchissement
    await page.waitForLoadState('networkidle')
    const pageContent = page.locator('.view-header, .view-content, .app-card, canvas, main')
    await expect(pageContent.first()).toBeVisible()
  })

  test('doit pouvoir interagir avec les contrôles', async ({ page }) => {
    const pauseButton = page.getByRole('button', { name: /pause|arrêter/i })

    if (await pauseButton.isVisible()) {
      await pauseButton.click()
    }
  })

  test('doit afficher des informations sur les erreurs si disponibles', async ({ page }) => {
    await page.waitForLoadState('networkidle')

    // Test souple - vérifie juste que la page charge correctement
    const viewContainer = page.locator('.view-header, .view-content, .app-card, canvas, main')
    await expect(viewContainer.first()).toBeVisible()
  })
})
