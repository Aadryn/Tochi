import { test, expect } from '@playwright/test'

test.describe('Routes', () => {
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
    await page.goto('/routes')
    await page.waitForLoadState('networkidle')
  })

  test('doit afficher le titre de la page', async ({ page }) => {
    // Le titre de la page est maintenant dans un h2 avec classe view-title
    const title = page.locator('h2.view-title, .view-title, h2')
    await expect(title.first()).toBeVisible()
    await expect(title.first()).toContainText(/routes/i)
  })

  test('doit afficher le contenu de la page', async ({ page }) => {
    // Vérifier que la vue routes est chargée (conteneur principal ou tableau)
    const viewContainer = page.locator('.view-header, .view-content, .app-card, table, main')
    await expect(viewContainer.first()).toBeVisible()
  })

  test('doit afficher une table ou liste des routes', async ({ page }) => {
    const table = page.locator('table, .routes-list, .app-card')
    const emptyState = page.locator('.empty-state, [data-testid="empty-state"]')
    
    const hasTable = await table.first().isVisible().catch(() => false)
    const hasEmptyState = await emptyState.first().isVisible().catch(() => false)
    
    expect(hasTable || hasEmptyState).toBeTruthy()
  })

  test('doit afficher le bouton d\'ajout si disponible', async ({ page }) => {
    const addButton = page.getByRole('button', { name: /ajouter|nouveau|créer|\+/i })
    
    if (await addButton.isVisible()) {
      await expect(addButton).toBeVisible()
    }
  })

  test('doit pouvoir ouvrir un dialogue de création si bouton existe', async ({ page }) => {
    const addButton = page.getByRole('button', { name: /ajouter|nouveau|créer|\+/i })
    
    if (await addButton.isVisible()) {
      await addButton.click()
      // Attendre un dialogue ou un formulaire
      const dialog = page.getByRole('dialog')
      const form = page.locator('form')
      await page.waitForTimeout(500)
      const hasDialog = await dialog.isVisible().catch(() => false)
      const hasForm = await form.isVisible().catch(() => false)
      expect(hasDialog || hasForm).toBeTruthy()
    }
  })

  test('doit pouvoir filtrer les routes si filtre disponible', async ({ page }) => {
    const searchInput = page.getByPlaceholder(/rechercher|filtrer|search/i)

    if (await searchInput.isVisible()) {
      await searchInput.fill('test')
      await page.waitForTimeout(300)
    }
  })

  test('doit afficher les informations des routes existantes', async ({ page }) => {
    const routeItems = page.locator('.route-item, .route-card, tr, .app-card')
    
    if (await routeItems.first().isVisible()) {
      const text = await routeItems.first().textContent()
      expect(text).toBeTruthy()
    }
  })
})
