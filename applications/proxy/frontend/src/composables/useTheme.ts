import { watch, onMounted } from 'vue'
import { useSettingsStore } from '@/stores'

/**
 * Composable pour gérer le thème de l'application.
 * Applique les classes CSS et les variables de thème dynamiquement.
 */
export function useTheme() {
  const settingsStore = useSettingsStore()

  /**
   * Applique le thème au document.
   */
  function applyTheme(theme: 'light' | 'dark'): void {
    const root = document.documentElement
    const body = document.body

    if (theme === 'dark') {
      root.classList.add('dark-theme')
      body.classList.add('dark-theme')
    } else {
      root.classList.remove('dark-theme')
      body.classList.remove('dark-theme')
    }

    // Mettre à jour la meta theme-color pour les navigateurs mobiles
    const metaThemeColor = document.querySelector('meta[name="theme-color"]')
    if (metaThemeColor) {
      metaThemeColor.setAttribute(
        'content',
        theme === 'dark' ? '#1e1e2e' : '#ffffff'
      )
    }
  }

  /**
   * Initialise le thème au démarrage.
   */
  function initTheme(): void {
    applyTheme(settingsStore.settings.theme)
  }

  // Observer les changements de thème
  watch(
    () => settingsStore.settings.theme,
    (newTheme) => {
      applyTheme(newTheme)
    }
  )

  // Appliquer le thème au montage
  onMounted(() => {
    initTheme()
  })

  return {
    applyTheme,
    initTheme,
  }
}
