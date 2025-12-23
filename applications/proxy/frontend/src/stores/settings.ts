import { ref, watch } from 'vue'
import { defineStore } from 'pinia'
import type { AppSettings } from '@/types'

const SETTINGS_STORAGE_KEY = 'llmproxy_admin_settings'

const defaultSettings: AppSettings = {
  theme: 'light',
  refreshInterval: 30,
  showNotifications: true,
  compactMode: false,
  language: 'fr',
}

/**
 * Store Pinia pour les paramètres de l'application.
 */
export const useSettingsStore = defineStore('settings', () => {
  // Charger les paramètres depuis localStorage
  function loadFromStorage(): AppSettings {
    try {
      const stored = localStorage.getItem(SETTINGS_STORAGE_KEY)
      if (stored) {
        return { ...defaultSettings, ...JSON.parse(stored) }
      }
    } catch (err) {
      console.warn('Erreur lors du chargement des paramètres:', err)
    }
    return { ...defaultSettings }
  }

  // État
  const settings = ref<AppSettings>(loadFromStorage())

  // Sauvegarder automatiquement les changements
  watch(
    settings,
    (newSettings) => {
      try {
        localStorage.setItem(SETTINGS_STORAGE_KEY, JSON.stringify(newSettings))
      } catch (err) {
        console.warn('Erreur lors de la sauvegarde des paramètres:', err)
      }
    },
    { deep: true },
  )

  // Actions
  function updateSetting<K extends keyof AppSettings>(key: K, value: AppSettings[K]): void {
    settings.value[key] = value
  }

  function toggleTheme(): void {
    settings.value.theme = settings.value.theme === 'light' ? 'dark' : 'light'
  }

  function toggleCompactMode(): void {
    settings.value.compactMode = !settings.value.compactMode
  }

  function toggleNotifications(): void {
    settings.value.showNotifications = !settings.value.showNotifications
  }

  function setRefreshInterval(seconds: number): void {
    settings.value.refreshInterval = Math.max(10, Math.min(300, seconds))
  }

  function setLanguage(language: 'fr' | 'en'): void {
    settings.value.language = language
  }

  function resetToDefaults(): void {
    settings.value = { ...defaultSettings }
  }

  return {
    // État
    settings,
    // Actions
    updateSetting,
    toggleTheme,
    toggleCompactMode,
    toggleNotifications,
    setRefreshInterval,
    setLanguage,
    resetToDefaults,
  }
})
