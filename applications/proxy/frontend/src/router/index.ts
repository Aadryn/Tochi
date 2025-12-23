import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { title: 'Connexion', public: true },
    },
    {
      path: '/',
      name: 'dashboard',
      component: () => import('@/views/DashboardView.vue'),
      meta: { title: 'Dashboard' },
    },
    {
      path: '/providers',
      name: 'providers',
      component: () => import('@/views/ProvidersView.vue'),
      meta: { title: 'Providers' },
    },
    {
      path: '/tenants',
      name: 'tenants',
      component: () => import('@/views/TenantsView.vue'),
      meta: { title: 'Tenants' },
    },
    {
      path: '/routes',
      name: 'routes',
      component: () => import('@/views/RoutesView.vue'),
      meta: { title: 'Routes' },
    },
    {
      path: '/monitoring',
      name: 'monitoring',
      component: () => import('@/views/MonitoringView.vue'),
      meta: { title: 'Monitoring' },
    },
    {
      path: '/settings',
      name: 'settings',
      component: () => import('@/views/SettingsView.vue'),
      meta: { title: 'Paramètres' },
    },
  ],
})

// Guard d'authentification
router.beforeEach((to, _from, next) => {
  const title = to.meta.title as string | undefined
  document.title = title ? `${title} - LLMProxy Admin` : 'LLMProxy Admin'
  
  // Vérifier si la route est publique
  const isPublicRoute = to.meta.public === true
  
  // Vérifier si l'utilisateur est authentifié
  const token = localStorage.getItem('auth_token')
  const isAuthenticated = !!token
  
  if (!isPublicRoute && !isAuthenticated) {
    // Rediriger vers login si non authentifié
    next({ name: 'login', query: { redirect: to.fullPath } })
  } else if (to.name === 'login' && isAuthenticated) {
    // Rediriger vers dashboard si déjà authentifié
    next({ name: 'dashboard' })
  } else {
    next()
  }
})

export default router
