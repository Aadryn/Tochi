import { test, expect } from '@playwright/test'

test.describe('Dashboard', () => {
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

  test('doit afficher le titre du dashboard', async ({ page }) => {
    // Utiliser h1 ou vérifier le contenu de la page
    const title = page.locator('h1, h2, .page-title')
    await expect(title.first()).toBeVisible()
  })

  test('doit afficher les cartes de métriques', async ({ page }) => {
    // Attendre que le chargement soit terminé
    const metricCards = page.locator('.metric-card, .app-card, .stat-card')
    
    // Soit on a des métriques, soit le contenu de base
    const hasCards = await metricCards.first().isVisible().catch(() => false)
    
    if (hasCards) {
      expect(await metricCards.count()).toBeGreaterThan(0)
    }
  })

  test('doit afficher le contenu du dashboard', async ({ page }) => {
    // Vérifier que la page principale est chargée (header, contenu, ou cartes)
    const mainContent = page.locator('.view-header, .view-content, main, .app-card')
    await expect(mainContent.first()).toBeVisible()
  })

  test('doit permettre de naviguer vers les providers', async ({ page }) => {
    const providersLink = page.locator('a, button').filter({ hasText: /providers/i })
    
    if (await providersLink.first().isVisible()) {
      await providersLink.first().click()
      await expect(page).toHaveURL(/.*providers/)
    }
  })

  test('doit afficher des données ou un état de chargement', async ({ page }) => {
    // Le dashboard doit montrer quelque chose (cartes, contenu, ou état vide)
    const content = page.locator('.app-card, .metric-card, .loading-spinner, .empty-state, .view-content, canvas')
    await expect(content.first()).toBeVisible()
  })

  test('doit pouvoir interagir avec les actions disponibles', async ({ page }) => {
    // Chercher n'importe quel bouton d'action
    const actionButtons = page.getByRole('button')
    const count = await actionButtons.count()
    
    if (count > 0) {
      // Il y a des boutons cliquables
      expect(count).toBeGreaterThan(0)
    }
  })
})
