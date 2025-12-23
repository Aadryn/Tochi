import { render } from '@testing-library/vue'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createWebHistory } from 'vue-router'
import PrimeVue from 'primevue/config'
import type { Component } from 'vue'

// Routes mock pour les tests
const routes = [
  { path: '/', name: 'dashboard', component: { template: '<div>Dashboard</div>' } },
  { path: '/providers', name: 'providers', component: { template: '<div>Providers</div>' } },
  { path: '/tenants', name: 'tenants', component: { template: '<div>Tenants</div>' } },
  { path: '/routes', name: 'routes', component: { template: '<div>Routes</div>' } },
  { path: '/monitoring', name: 'monitoring', component: { template: '<div>Monitoring</div>' } },
  { path: '/settings', name: 'settings', component: { template: '<div>Settings</div>' } },
]

/**
 * Options personnalisées pour le rendu des composants de test.
 */
export interface CustomRenderOptions {
  /**
   * Route initiale pour le router.
   */
  initialRoute?: string
  /**
   * État initial pour un store Pinia.
   */
  initialState?: Record<string, unknown>
  /**
   * Props à passer au composant.
   */
  props?: Record<string, unknown>
}

/**
 * Fonction de rendu personnalisée qui inclut Pinia, Router et PrimeVue.
 */
export function renderWithPlugins(
  component: Component,
  options: CustomRenderOptions = {},
): ReturnType<typeof render> {
  const { initialRoute = '/', initialState, props, ...renderOptions } = options

  // Créer une nouvelle instance de Pinia
  const pinia = createPinia()
  setActivePinia(pinia)

  // Initialiser l'état si fourni
  if (initialState) {
    Object.entries(initialState).forEach(([storeName, state]) => {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const store = (pinia.state.value as any)[storeName]
      if (store) {
        Object.assign(store, state)
      }
    })
  }

  // Créer le router
  const router = createRouter({
    history: createWebHistory(),
    routes,
  })
  router.push(initialRoute)

  return render(component, {
    props,
    global: {
      plugins: [pinia, router, PrimeVue],
      stubs: {
        // Stub les transitions pour éviter les problèmes dans les tests
        transition: false,
        'transition-group': false,
      },
    },
  })
}

/**
 * Attendre que toutes les promesses soient résolues.
 */
export function flushPromises(): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, 0))
}

/**
 * Attendre un délai spécifique (en ms).
 */
export function waitFor(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms))
}

/**
 * Mock d'un appel API réussi.
 */
export function mockApiSuccess<T>(data: T): Promise<T> {
  return Promise.resolve(data)
}

/**
 * Mock d'un appel API en erreur.
 */
export function mockApiError(message: string, status = 500): Promise<never> {
  const error = new Error(message) as Error & { response?: { status: number } }
  error.response = { status }
  return Promise.reject(error)
}

// Réexporter les utilitaires de testing-library
export * from '@testing-library/vue'
export { default as userEvent } from '@testing-library/user-event'
