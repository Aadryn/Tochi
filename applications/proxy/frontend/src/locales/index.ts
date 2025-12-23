import { createI18n } from 'vue-i18n'
import fr from './fr.json'
import en from './en.json'

// Type-safe message schema
export type MessageSchema = typeof fr

/**
 * Détecte la langue initiale de l'utilisateur
 * Priorité : localStorage > navigateur > français (défaut)
 */
function getInitialLocale(): 'fr' | 'en' {
  // 1. Vérifier localStorage (préférence explicite utilisateur)
  const stored = localStorage.getItem('llmproxy-locale')
  if (stored === 'fr' || stored === 'en') {
    return stored
  }
  
  // 2. Détecter langue du navigateur
  const browserLang = navigator.language.split('-')[0]
  if (browserLang === 'fr' || browserLang === 'en') {
    return browserLang as 'fr' | 'en'
  }
  
  // 3. Défaut : français
  return 'fr'
}

/**
 * Instance i18n configurée pour Vue 3 Composition API
 * - legacy: false (utilise Composition API)
 * - globalInjection: true (accès global à $t dans templates)
 * - fallbackLocale: français (en cas de traduction manquante)
 */
export const i18n = createI18n({
  legacy: false, // Composition API mode
  locale: getInitialLocale(),
  fallbackLocale: 'fr',
  messages: {
    fr,
    en
  },
  globalInjection: true,
  missingWarn: import.meta.env.DEV, // Warn en dev seulement
  fallbackWarn: import.meta.env.DEV
})

/**
 * Change la langue et persiste la préférence
 * @param locale - 'fr' ou 'en'
 */
export function setLocale(locale: 'fr' | 'en'): void {
  // @ts-ignore - i18n.global.locale is a WritableComputedRef in Composition API mode
  i18n.global.locale.value = locale
  localStorage.setItem('llmproxy-locale', locale)
  document.documentElement.lang = locale
}

/**
 * Récupère la langue courante
 */
export function getCurrentLocale(): 'fr' | 'en' {
  // @ts-ignore - i18n.global.locale is a WritableComputedRef in Composition API mode
  return i18n.global.locale.value as 'fr' | 'en'
}
