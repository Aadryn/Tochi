import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useSettingsStore } from '@/stores/settings'

describe('useSettingsStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    // Réinitialiser le localStorage mock
    vi.mocked(localStorage.getItem).mockReturnValue(null)
    vi.mocked(localStorage.setItem).mockClear()
  })

  describe('état initial', () => {
    it('doit avoir des valeurs par défaut correctes', () => {
      const store = useSettingsStore()

      expect(store.settings.theme).toBe('light')
      expect(store.settings.refreshInterval).toBe(30)
      expect(store.settings.showNotifications).toBe(true)
      expect(store.settings.compactMode).toBe(false)
      expect(store.settings.language).toBe('fr')
    })

    it('doit charger les paramètres depuis localStorage', () => {
      const savedSettings = {
        theme: 'dark',
        refreshInterval: 60,
        showNotifications: false,
        compactMode: true,
        language: 'en',
      }
      vi.mocked(localStorage.getItem).mockReturnValue(JSON.stringify(savedSettings))

      // Recréer le store pour qu'il lise le localStorage
      setActivePinia(createPinia())
      const store = useSettingsStore()

      expect(store.settings.theme).toBe('dark')
      expect(store.settings.refreshInterval).toBe(60)
      expect(store.settings.showNotifications).toBe(false)
      expect(store.settings.compactMode).toBe(true)
      expect(store.settings.language).toBe('en')
    })
  })

  describe('updateSetting', () => {
    it('doit mettre à jour un paramètre spécifique', () => {
      const store = useSettingsStore()

      store.updateSetting('theme', 'dark')

      expect(store.settings.theme).toBe('dark')
    })

    it('doit sauvegarder dans localStorage après modification', async () => {
      const store = useSettingsStore()

      store.updateSetting('theme', 'dark')

      // Attendre le watch
      await new Promise((resolve) => setTimeout(resolve, 0))

      expect(localStorage.setItem).toHaveBeenCalled()
    })
  })

  describe('toggleTheme', () => {
    it('doit basculer de light à dark', () => {
      const store = useSettingsStore()
      expect(store.settings.theme).toBe('light')

      store.toggleTheme()

      expect(store.settings.theme).toBe('dark')
    })

    it('doit basculer de dark à light', () => {
      const store = useSettingsStore()
      store.settings.theme = 'dark'

      store.toggleTheme()

      expect(store.settings.theme).toBe('light')
    })
  })

  describe('toggleCompactMode', () => {
    it('doit basculer le mode compact', () => {
      const store = useSettingsStore()
      expect(store.settings.compactMode).toBe(false)

      store.toggleCompactMode()
      expect(store.settings.compactMode).toBe(true)

      store.toggleCompactMode()
      expect(store.settings.compactMode).toBe(false)
    })
  })

  describe('toggleNotifications', () => {
    it('doit basculer les notifications', () => {
      const store = useSettingsStore()
      expect(store.settings.showNotifications).toBe(true)

      store.toggleNotifications()
      expect(store.settings.showNotifications).toBe(false)

      store.toggleNotifications()
      expect(store.settings.showNotifications).toBe(true)
    })
  })

  describe('setRefreshInterval', () => {
    it('doit définir l\'intervalle de rafraîchissement', () => {
      const store = useSettingsStore()

      store.setRefreshInterval(60)

      expect(store.settings.refreshInterval).toBe(60)
    })

    it('doit respecter la limite minimale de 10 secondes', () => {
      const store = useSettingsStore()

      store.setRefreshInterval(5)

      expect(store.settings.refreshInterval).toBe(10)
    })

    it('doit respecter la limite maximale de 300 secondes', () => {
      const store = useSettingsStore()

      store.setRefreshInterval(600)

      expect(store.settings.refreshInterval).toBe(300)
    })
  })

  describe('setLanguage', () => {
    it('doit définir la langue', () => {
      const store = useSettingsStore()

      store.setLanguage('en')

      expect(store.settings.language).toBe('en')
    })
  })

  describe('resetToDefaults', () => {
    it('doit réinitialiser tous les paramètres aux valeurs par défaut', () => {
      const store = useSettingsStore()

      // Modifier tous les paramètres
      store.settings.theme = 'dark'
      store.settings.refreshInterval = 120
      store.settings.showNotifications = false
      store.settings.compactMode = true
      store.settings.language = 'en'

      // Réinitialiser
      store.resetToDefaults()

      // Vérifier les valeurs par défaut
      expect(store.settings.theme).toBe('light')
      expect(store.settings.refreshInterval).toBe(30)
      expect(store.settings.showNotifications).toBe(true)
      expect(store.settings.compactMode).toBe(false)
      expect(store.settings.language).toBe('fr')
    })
  })
})
