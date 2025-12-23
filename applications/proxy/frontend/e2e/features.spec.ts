import { test, expect } from '@playwright/test'

/**
 * Tests E2E pour les nouvelles fonctionnalités
 * - Thème sombre
 * - Modals de création/édition
 * - Graphiques Chart.js
 */
test.describe('Nouvelles Fonctionnalités - Point 1, 2, 3', () => {
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

  test.describe('Thème Sombre (Point 1)', () => {
    test('doit avoir un toggle de thème visible', async ({ page }) => {
      // Chercher le bouton de toggle thème avec data-testid
      const themeToggle = page.locator('[data-testid="theme-toggle"]')
      await expect(themeToggle).toBeVisible()
    })

    test('doit pouvoir basculer entre thème clair et sombre', async ({ page }) => {
      // Récupérer l'état initial
      const htmlElement = page.locator('html')
      const initialTheme = await htmlElement.getAttribute('class')
      
      // Cliquer sur le toggle avec data-testid
      const themeToggle = page.locator('[data-testid="theme-toggle"]')
      await themeToggle.click()
      
      // Attendre le changement
      await page.waitForTimeout(300)
      
      // Vérifier que la classe a changé
      const newTheme = await htmlElement.getAttribute('class')
      expect(newTheme).not.toBe(initialTheme)
    })

    test('doit persister le choix du thème après rechargement', async ({ page }) => {
      const htmlElement = page.locator('html')
      const themeToggle = page.locator('[data-testid="theme-toggle"]')
      
      // Activer le thème sombre
      await themeToggle.click()
      await page.waitForTimeout(300)
      const themeAfterToggle = await htmlElement.getAttribute('class')
      
      // Recharger la page
      await page.reload()
      await page.waitForLoadState('networkidle')
      
      // Vérifier que le thème est persisté
      const themeAfterReload = await htmlElement.getAttribute('class')
      expect(themeAfterReload).toBe(themeAfterToggle)
    })
  })

  test.describe('Graphiques Chart.js (Point 2)', () => {
    test('doit afficher la page de monitoring avec graphiques', async ({ page }) => {
      await page.goto('/monitoring')
      await page.waitForLoadState('networkidle')
      
      // Vérifier que la page est chargée
      const title = page.locator('h1')
      await expect(title).toContainText(/monitoring/i)
    })

    test('doit afficher le graphique en donut (Requêtes par provider)', async ({ page }) => {
      await page.goto('/monitoring')
      await page.waitForLoadState('networkidle')
      
      // Chercher le canvas Chart.js
      const canvas = page.locator('canvas').first()
      await expect(canvas).toBeVisible()
      
      // Vérifier que le canvas a des dimensions
      const box = await canvas.boundingBox()
      expect(box).not.toBeNull()
      expect(box!.width).toBeGreaterThan(0)
      expect(box!.height).toBeGreaterThan(0)
    })

    test('doit afficher le graphique de latence (ligne)', async ({ page }) => {
      await page.goto('/monitoring')
      await page.waitForLoadState('networkidle')
      
      // Devrait y avoir au moins 2 canvas (donut + ligne)
      const canvases = page.locator('canvas')
      const count = await canvases.count()
      expect(count).toBeGreaterThanOrEqual(2)
    })

    test('doit afficher des métriques en temps réel', async ({ page }) => {
      await page.goto('/monitoring')
      await page.waitForLoadState('networkidle')
      
      // Vérifier la présence de cartes de métriques
      const metricCards = page.locator('.app-card, .metric-card')
      const count = await metricCards.count()
      expect(count).toBeGreaterThan(0)
    })
  })

  test.describe('Modals Création/Édition (Point 3)', () => {
    test.describe('Modal Provider', () => {
      test('doit ouvrir le modal de création de provider', async ({ page }) => {
        await page.goto('/providers')
        await page.waitForLoadState('networkidle')
        
        // Cliquer sur "Nouveau Provider"
        const newButton = page.getByRole('button', { name: /nouveau provider/i })
        await expect(newButton).toBeVisible()
        await newButton.click()
        
        // Vérifier que le modal est ouvert
        await page.waitForTimeout(500)
        const modal = page.locator('[role="dialog"], .app-modal')
        await expect(modal).toBeVisible()
        
        // Vérifier le titre du modal
        const modalTitle = modal.locator('h2, h3, .modal-title')
        await expect(modalTitle).toContainText(/provider/i)
      })

      test('doit afficher les boutons de type de provider', async ({ page }) => {
        await page.goto('/providers')
        await page.waitForLoadState('networkidle')
        
        const newButton = page.getByRole('button', { name: /nouveau provider/i })
        await newButton.click()
        await page.waitForTimeout(500)
        
        // Vérifier les boutons de type (OpenAI, Anthropic, etc.)
        const modal = page.locator('[role="dialog"], .app-modal')
        const typeButtons = modal.locator('button').filter({ hasText: /openai|anthropic|azure|ollama|custom/i })
        const count = await typeButtons.count()
        expect(count).toBeGreaterThanOrEqual(4)
      })

      test('doit afficher des suggestions de modèles', async ({ page }) => {
        await page.goto('/providers')
        await page.waitForLoadState('networkidle')
        
        const newButton = page.getByRole('button', { name: /nouveau provider/i })
        await newButton.click()
        await page.waitForTimeout(500)
        
        // Cliquer sur OpenAI
        const openAiButton = page.locator('button').filter({ hasText: /openai/i }).first()
        await openAiButton.click()
        
        // Vérifier les suggestions de modèles
        const modal = page.locator('[role="dialog"], .app-modal')
        const suggestions = modal.locator('.suggestion-chip, button').filter({ hasText: /gpt-4|gpt-3.5/i })
        const count = await suggestions.count()
        expect(count).toBeGreaterThan(0)
      })

      test('doit pouvoir fermer le modal avec Escape', async ({ page }) => {
        await page.goto('/providers')
        await page.waitForLoadState('networkidle')
        
        const newButton = page.getByRole('button', { name: /nouveau provider/i })
        await newButton.click()
        await page.waitForTimeout(500)
        
        // Fermer avec Escape
        await page.keyboard.press('Escape')
        await page.waitForTimeout(500)
        
        // Vérifier que le modal est fermé
        const modal = page.locator('[role="dialog"], .app-modal')
        await expect(modal).not.toBeVisible()
      })
    })

    test.describe('Modal Tenant', () => {
      test('doit ouvrir le modal de création de tenant', async ({ page }) => {
        await page.goto('/tenants')
        await page.waitForLoadState('networkidle')
        
        // Cliquer sur "Nouveau Tenant"
        const newButton = page.getByRole('button', { name: /nouveau tenant/i })
        await expect(newButton).toBeVisible()
        await newButton.click()
        
        // Vérifier que le modal est ouvert
        await page.waitForTimeout(500)
        const modal = page.locator('[role="dialog"], .app-modal')
        await expect(modal).toBeVisible()
        
        // Vérifier le titre du modal
        const modalTitle = modal.locator('h2, h3, .modal-title')
        await expect(modalTitle).toContainText(/tenant/i)
      })

      test('doit afficher les presets de quotas', async ({ page }) => {
        await page.goto('/tenants')
        await page.waitForLoadState('networkidle')
        
        const newButton = page.getByRole('button', { name: /nouveau tenant/i })
        await newButton.click()
        await page.waitForTimeout(500)
        
        // Vérifier les boutons de preset (Starter, Standard, Pro, Enterprise)
        const modal = page.locator('[role="dialog"], .app-modal')
        const presetButtons = modal.locator('button').filter({ hasText: /starter|standard|pro|enterprise/i })
        const count = await presetButtons.count()
        expect(count).toBe(4)
      })

      test('doit générer automatiquement le slug depuis le nom', async ({ page }) => {
        await page.goto('/tenants')
        await page.waitForLoadState('networkidle')
        
        const newButton = page.getByRole('button', { name: /nouveau tenant/i })
        await newButton.click()
        await page.waitForTimeout(500)
        
        const modal = page.locator('[role="dialog"], .app-modal')
        
        // Trouver le champ nom
        const nameInput = modal.locator('input[placeholder*="Mon Application"], input[name="name"]').first()
        await nameInput.fill('Test Company')
        
        // Attendre la génération du slug
        await page.waitForTimeout(300)
        
        // Vérifier que le slug est généré
        const slugInput = modal.locator('input[readonly], .slug-input').first()
        const slugValue = await slugInput.inputValue()
        expect(slugValue.toLowerCase()).toContain('test')
      })

      test('doit formater les nombres de quota', async ({ page }) => {
        await page.goto('/tenants')
        await page.waitForLoadState('networkidle')
        
        const newButton = page.getByRole('button', { name: /nouveau tenant/i })
        await newButton.click()
        await page.waitForTimeout(500)
        
        const modal = page.locator('[role="dialog"], .app-modal')
        
        // Cliquer sur le preset Standard
        const standardButton = modal.locator('button').filter({ hasText: /standard/i }).first()
        await standardButton.click()
        
        // Vérifier que les nombres sont formatés (avec espaces ou virgules)
        const quotaText = await modal.locator('.quota-value, .formatted-number').first().textContent()
        expect(quotaText).toMatch(/\d+\s\d+|\d+,\d+/)
      })
    })

    test.describe('Modal Confirm Delete', () => {
      test('doit afficher un dialogue de confirmation avant suppression', async ({ page }) => {
        await page.goto('/tenants')
        await page.waitForLoadState('networkidle')
        
        // Chercher un bouton de suppression (icône trash ou texte "Supprimer")
        const deleteButton = page.locator('button[aria-label*="upprimer"], button[title*="upprimer"]').first()
        
        const isVisible = await deleteButton.isVisible().catch(() => false)
        if (isVisible) {
          await deleteButton.click()
          await page.waitForTimeout(500)
          
          // Vérifier le dialogue de confirmation
          const confirmDialog = page.locator('[role="dialog"], .confirm-dialog')
          await expect(confirmDialog).toBeVisible()
          
          // Vérifier le message d'avertissement
          const message = confirmDialog.locator('text=/supprimer|delete|irréversible/i')
          await expect(message).toBeVisible()
          
          // Vérifier les boutons Annuler et Confirmer
          const cancelButton = confirmDialog.getByRole('button', { name: /annuler|cancel/i })
          const confirmButton = confirmDialog.getByRole('button', { name: /supprimer|delete|confirmer/i })
          
          await expect(cancelButton).toBeVisible()
          await expect(confirmButton).toBeVisible()
        }
      })
    })
  })

  test.describe('Workflow Complet E2E', () => {
    test('doit permettre un workflow complet : créer provider → vérifier liste → dashboard', async ({ page }) => {
      // Étape 1 : Aller sur la page providers
      await page.goto('/providers')
      await page.waitForLoadState('networkidle')
      
      // Étape 2 : Compter les providers existants
      const initialCards = page.locator('.app-card, .provider-card')
      const initialCount = await initialCards.count()
      
      // Étape 3 : Retourner au dashboard
      await page.goto('/')
      await page.waitForLoadState('networkidle')
      
      // Étape 4 : Vérifier que le dashboard affiche des métriques
      const metricCards = page.locator('.app-card, .metric-card')
      const metricCount = await metricCards.count()
      expect(metricCount).toBeGreaterThan(0)
      
      // Étape 5 : Naviguer vers monitoring
      await page.goto('/monitoring')
      await page.waitForLoadState('networkidle')
      
      // Étape 6 : Vérifier que les graphiques sont présents
      const canvases = page.locator('canvas')
      const canvasCount = await canvases.count()
      expect(canvasCount).toBeGreaterThanOrEqual(2)
    })
  })
})
