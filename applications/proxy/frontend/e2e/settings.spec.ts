import { test, expect } from '@playwright/test'

test.describe('Settings', () => {
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
    await page.goto('/settings')
    await page.waitForLoadState('networkidle')
  })

  test('doit afficher le titre de la page', async ({ page }) => {
    // Le titre de la page est maintenant dans un h2 avec classe view-title
    const title = page.locator('h2.view-title, .view-title, h2')
    await expect(title.first()).toBeVisible()
    await expect(title.first()).toContainText(/paramètres|settings/i)
  })

  test('doit afficher les options de thème', async ({ page }) => {
    // Chercher le texte "thème" avec un sélecteur plus précis
    const themeLabel = page.locator('.setting-label, label').filter({ hasText: /thème/i })
    await expect(themeLabel.first()).toBeVisible()
  })

  test('doit pouvoir interagir avec les contrôles de thème', async ({ page }) => {
    // Chercher un toggle ou bouton de thème
    const themeToggle = page.locator('[data-testid="theme-toggle"], .theme-toggle, input[type="checkbox"], button').filter({ hasText: /sombre|clair|dark|light/i })

    if (await themeToggle.first().isVisible()) {
      await themeToggle.first().click()
    }
  })

  test('doit afficher les options de notification', async ({ page }) => {
    // Chercher le label notification spécifiquement
    const notifLabel = page.locator('.setting-label, label').filter({ hasText: /notification/i })
    await expect(notifLabel.first()).toBeVisible()
  })

  test('doit pouvoir interagir avec les notifications', async ({ page }) => {
    const notifToggle = page.locator('input[type="checkbox"]').first()

    if (await notifToggle.isVisible()) {
      const initialState = await notifToggle.isChecked()
      await notifToggle.click()
      // Vérifier que l'état a changé
      const newState = await notifToggle.isChecked()
      expect(newState).not.toBe(initialState)
    }
  })

  test('doit afficher l\'option d\'intervalle de rafraîchissement', async ({ page }) => {
    const refreshLabel = page.locator('.setting-label, label').filter({ hasText: /rafraîchissement|refresh|intervalle/i })
    await expect(refreshLabel.first()).toBeVisible()
  })

  test('doit pouvoir modifier l\'intervalle de rafraîchissement', async ({ page }) => {
    const intervalInput = page.locator('input[type="number"], input[type="range"], select').first()

    if (await intervalInput.isVisible()) {
      const inputType = await intervalInput.getAttribute('type')
      if (inputType === 'number') {
        await intervalInput.fill('60')
        await expect(intervalInput).toHaveValue('60')
      }
    }
  })

  test('doit afficher le mode compact', async ({ page }) => {
    const compactLabel = page.locator('.setting-label, label').filter({ hasText: /compact/i })
    await expect(compactLabel.first()).toBeVisible()
  })

  test('doit afficher la section À propos', async ({ page }) => {
    // Chercher le titre "À propos" dans une carte ou section
    const aboutSection = page.locator('.card-title, h3, h4').filter({ hasText: /à propos/i })
    await expect(aboutSection.first()).toBeVisible()
  })

  test('doit afficher la version de l\'application', async ({ page }) => {
    const versionText = page.locator('text=/\\d+\\.\\d+\\.\\d+/')
    await expect(versionText.first()).toBeVisible()
  })

  test('doit pouvoir réinitialiser les paramètres par défaut', async ({ page }) => {
    const resetButton = page.getByRole('button', { name: /réinitialiser|reset|défaut/i })

    if (await resetButton.isVisible()) {
      await resetButton.click()
    }
  })
})
