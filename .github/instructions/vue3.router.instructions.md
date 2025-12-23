---
description: Vue 3 Router - Configuration, navigation, guards, lazy loading, meta, transitions
name: Vue3_Router
applyTo: "**/frontend/router/index.ts,**/frontend/router/**/*.ts"
---

# Vue 3 Router

Guide complet pour Vue Router 4 avec Vue 3 et TypeScript.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** `this.$router` dans Composition API - utilise `useRouter()`
- **N'importe jamais** les composants directement sans lazy loading pour les routes
- **Ne définis jamais** de routes sans `name` unique
- **N'utilise jamais** `router.push()` avec des chemins dynamiques non validés
- **Ne stocke jamais** de données sensibles dans les query params
- **N'oublie jamais** de gérer les erreurs de navigation

## ✅ À FAIRE

- **Utilise toujours** `defineAsyncComponent` ou `() => import()` pour lazy loading
- **Définis toujours** des types pour les params et query
- **Utilise toujours** des navigation guards pour la protection des routes
- **Préfère toujours** la navigation par `name` plutôt que par `path`
- **Valide toujours** les paramètres de route avec props
- **Documente toujours** les meta des routes

## 📁 Structure du Router

### Organisation des Fichiers

```
src/
├── router/
│   ├── index.ts              # Configuration principale
│   ├── routes.ts             # Définition des routes
│   ├── guards/
│   │   ├── index.ts          # Export des guards
│   │   ├── authGuard.ts      # Guard d'authentification
│   │   └── roleGuard.ts      # Guard de rôles
│   ├── middleware/
│   │   ├── index.ts          # Export des middlewares
│   │   └── trackingMiddleware.ts
│   └── types.ts              # Types pour le router
```

### Configuration Principale

```typescript
// router/index.ts
import { createRouter, createWebHistory, type Router } from 'vue-router';
import { routes } from './routes';
import { setupGuards } from './guards';

/**
 * Crée et configure le router de l'application.
 * @returns Instance du router configurée
 */
export function createAppRouter(): Router {
  const router = createRouter({
    history: createWebHistory(import.meta.env.BASE_URL),
    routes,
    scrollBehavior(to, from, savedPosition) {
      // Restaurer la position de scroll si disponible
      if (savedPosition) {
        return savedPosition;
      }
      
      // Scroll vers l'ancre si présente
      if (to.hash) {
        return {
          el: to.hash,
          behavior: 'smooth',
        };
      }
      
      // Scroll en haut pour les nouvelles pages
      return { top: 0, behavior: 'smooth' };
    },
  });

  // Configuration des guards
  setupGuards(router);

  return router;
}

export const router = createAppRouter();
```

## 📝 Définition des Routes

### Types pour les Routes

```typescript
// router/types.ts
import type { RouteRecordRaw, RouteMeta } from 'vue-router';

/**
 * Métadonnées personnalisées des routes.
 */
export interface AppRouteMeta extends RouteMeta {
  /** Titre de la page (pour document.title) */
  title?: string;
  /** Route nécessite authentification */
  requiresAuth?: boolean;
  /** Rôles autorisés à accéder */
  roles?: string[];
  /** Route publique (accessible sans auth) */
  isPublic?: boolean;
  /** Layout à utiliser */
  layout?: 'default' | 'auth' | 'admin' | 'blank';
  /** Breadcrumb label */
  breadcrumb?: string;
  /** Icône pour la navigation */
  icon?: string;
  /** Ordre dans le menu (si applicable) */
  order?: number;
  /** Cacher dans le menu */
  hideInMenu?: boolean;
  /** Permissions requises */
  permissions?: string[];
}

/**
 * Route de l'application avec meta typée.
 */
export interface AppRoute extends Omit<RouteRecordRaw, 'meta'> {
  meta?: AppRouteMeta;
  children?: AppRoute[];
}

/**
 * Paramètres typés pour les routes.
 */
export interface RouteParams {
  id?: string;
  slug?: string;
}

/**
 * Query params typés.
 */
export interface RouteQuery {
  page?: string;
  search?: string;
  sort?: string;
  order?: 'asc' | 'desc';
}
```

### Définition des Routes

```typescript
// router/routes.ts
import type { AppRoute } from './types';

/**
 * Routes publiques de l'application.
 */
const publicRoutes: AppRoute[] = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/auth/LoginView.vue'),
    meta: {
      title: 'Connexion',
      isPublic: true,
      layout: 'auth',
    },
  },
  {
    path: '/register',
    name: 'Register',
    component: () => import('@/views/auth/RegisterView.vue'),
    meta: {
      title: 'Inscription',
      isPublic: true,
      layout: 'auth',
    },
  },
  {
    path: '/forgot-password',
    name: 'ForgotPassword',
    component: () => import('@/views/auth/ForgotPasswordView.vue'),
    meta: {
      title: 'Mot de passe oublié',
      isPublic: true,
      layout: 'auth',
    },
  },
];

/**
 * Routes protégées (nécessitent authentification).
 */
const protectedRoutes: AppRoute[] = [
  {
    path: '/',
    name: 'Home',
    component: () => import('@/views/HomeView.vue'),
    meta: {
      title: 'Accueil',
      requiresAuth: true,
      breadcrumb: 'Accueil',
      icon: 'home',
      order: 1,
    },
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('@/views/DashboardView.vue'),
    meta: {
      title: 'Tableau de bord',
      requiresAuth: true,
      breadcrumb: 'Dashboard',
      icon: 'dashboard',
      order: 2,
    },
  },
  {
    path: '/users',
    name: 'Users',
    component: () => import('@/views/users/UsersLayout.vue'),
    meta: {
      title: 'Utilisateurs',
      requiresAuth: true,
      roles: ['admin', 'manager'],
      breadcrumb: 'Utilisateurs',
      icon: 'users',
      order: 3,
    },
    children: [
      {
        path: '',
        name: 'UsersList',
        component: () => import('@/views/users/UsersListView.vue'),
        meta: {
          title: 'Liste des utilisateurs',
          breadcrumb: 'Liste',
        },
      },
      {
        path: 'create',
        name: 'UserCreate',
        component: () => import('@/views/users/UserCreateView.vue'),
        meta: {
          title: 'Nouvel utilisateur',
          breadcrumb: 'Création',
          permissions: ['user:create'],
        },
      },
      {
        path: ':id',
        name: 'UserDetail',
        component: () => import('@/views/users/UserDetailView.vue'),
        props: true, // Passe les params comme props
        meta: {
          title: 'Détail utilisateur',
          breadcrumb: 'Détail',
        },
      },
      {
        path: ':id/edit',
        name: 'UserEdit',
        component: () => import('@/views/users/UserEditView.vue'),
        props: true,
        meta: {
          title: 'Modifier utilisateur',
          breadcrumb: 'Modification',
          permissions: ['user:update'],
        },
      },
    ],
  },
];

/**
 * Routes d'administration.
 */
const adminRoutes: AppRoute[] = [
  {
    path: '/admin',
    name: 'Admin',
    component: () => import('@/views/admin/AdminLayout.vue'),
    meta: {
      title: 'Administration',
      requiresAuth: true,
      roles: ['admin'],
      layout: 'admin',
      breadcrumb: 'Admin',
      icon: 'settings',
    },
    children: [
      {
        path: '',
        name: 'AdminDashboard',
        component: () => import('@/views/admin/AdminDashboardView.vue'),
        meta: {
          title: 'Dashboard Admin',
          breadcrumb: 'Dashboard',
        },
      },
      {
        path: 'settings',
        name: 'AdminSettings',
        component: () => import('@/views/admin/AdminSettingsView.vue'),
        meta: {
          title: 'Paramètres',
          breadcrumb: 'Paramètres',
        },
      },
    ],
  },
];

/**
 * Routes d'erreur.
 */
const errorRoutes: AppRoute[] = [
  {
    path: '/forbidden',
    name: 'Forbidden',
    component: () => import('@/views/errors/ForbiddenView.vue'),
    meta: {
      title: 'Accès refusé',
      isPublic: true,
      layout: 'blank',
    },
  },
  {
    path: '/:pathMatch(.*)*',
    name: 'NotFound',
    component: () => import('@/views/errors/NotFoundView.vue'),
    meta: {
      title: 'Page non trouvée',
      isPublic: true,
      layout: 'blank',
    },
  },
];

/**
 * Toutes les routes de l'application.
 */
export const routes: AppRoute[] = [
  ...publicRoutes,
  ...protectedRoutes,
  ...adminRoutes,
  ...errorRoutes,
];
```

## 🛡️ Navigation Guards

### Guard d'Authentification

```typescript
// router/guards/authGuard.ts
import type { NavigationGuardNext, RouteLocationNormalized, Router } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

/**
 * Guard vérifiant l'authentification.
 */
export function authGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
): void {
  const authStore = useAuthStore();
  const requiresAuth = to.meta.requiresAuth as boolean | undefined;
  const isPublic = to.meta.isPublic as boolean | undefined;

  // Route publique - autoriser
  if (isPublic) {
    // Si déjà authentifié et sur login, rediriger vers home
    if (authStore.isAuthenticated && to.name === 'Login') {
      next({ name: 'Home' });
      return;
    }
    next();
    return;
  }

  // Route protégée - vérifier auth
  if (requiresAuth && !authStore.isAuthenticated) {
    // Sauvegarder la destination pour redirect après login
    next({
      name: 'Login',
      query: { redirect: to.fullPath },
    });
    return;
  }

  next();
}

/**
 * Configure le guard d'authentification sur le router.
 */
export function setupAuthGuard(router: Router): void {
  router.beforeEach(authGuard);
}
```

### Guard de Rôles

```typescript
// router/guards/roleGuard.ts
import type { NavigationGuardNext, RouteLocationNormalized, Router } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

/**
 * Guard vérifiant les rôles utilisateur.
 */
export function roleGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
): void {
  const authStore = useAuthStore();
  const requiredRoles = to.meta.roles as string[] | undefined;

  // Pas de rôles requis - autoriser
  if (!requiredRoles || requiredRoles.length === 0) {
    next();
    return;
  }

  // Vérifier si l'utilisateur a au moins un des rôles requis
  const hasRole = requiredRoles.some((role) => authStore.hasRole(role));

  if (!hasRole) {
    next({ name: 'Forbidden' });
    return;
  }

  next();
}

/**
 * Configure le guard de rôles sur le router.
 */
export function setupRoleGuard(router: Router): void {
  router.beforeEach(roleGuard);
}
```

### Guard de Permissions

```typescript
// router/guards/permissionGuard.ts
import type { NavigationGuardNext, RouteLocationNormalized, Router } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

/**
 * Guard vérifiant les permissions utilisateur.
 */
export function permissionGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
): void {
  const authStore = useAuthStore();
  const requiredPermissions = to.meta.permissions as string[] | undefined;

  if (!requiredPermissions || requiredPermissions.length === 0) {
    next();
    return;
  }

  // Vérifier toutes les permissions requises
  const hasAllPermissions = requiredPermissions.every((permission) =>
    authStore.hasPermission(permission)
  );

  if (!hasAllPermissions) {
    next({ name: 'Forbidden' });
    return;
  }

  next();
}
```

### Configuration des Guards

```typescript
// router/guards/index.ts
import type { Router } from 'vue-router';
import { setupAuthGuard } from './authGuard';
import { setupRoleGuard } from './roleGuard';

/**
 * Configure tous les guards sur le router.
 */
export function setupGuards(router: Router): void {
  // Ordre important: auth d'abord, puis rôles
  setupAuthGuard(router);
  setupRoleGuard(router);
  
  // Guard pour le titre de page
  router.afterEach((to) => {
    const title = to.meta.title as string | undefined;
    document.title = title ? `${title} | Mon App` : 'Mon App';
  });

  // Gestion des erreurs de navigation
  router.onError((error) => {
    console.error('Navigation error:', error);
    // Possibilité de rediriger vers une page d'erreur
  });
}

export { authGuard } from './authGuard';
export { roleGuard } from './roleGuard';
export { permissionGuard } from './permissionGuard';
```

## 🧭 Navigation Programmatique

### Composable useRouter

```typescript
// composables/useAppRouter.ts
import { useRouter, useRoute, type RouteLocationRaw } from 'vue-router';
import type { RouteParams, RouteQuery } from '@/router/types';

/**
 * Composable pour la navigation typée.
 */
export function useAppRouter() {
  const router = useRouter();
  const route = useRoute();

  /**
   * Navigue vers une route par son nom.
   */
  async function navigateTo(
    name: string,
    params?: RouteParams,
    query?: RouteQuery
  ): Promise<void> {
    try {
      await router.push({ name, params, query });
    } catch (error) {
      // Navigation dupliquée ignorée
      if ((error as Error).name !== 'NavigationDuplicated') {
        console.error('Navigation failed:', error);
        throw error;
      }
    }
  }

  /**
   * Remplace la route actuelle.
   */
  async function replaceTo(
    name: string,
    params?: RouteParams,
    query?: RouteQuery
  ): Promise<void> {
    try {
      await router.replace({ name, params, query });
    } catch (error) {
      if ((error as Error).name !== 'NavigationDuplicated') {
        throw error;
      }
    }
  }

  /**
   * Retourne en arrière dans l'historique.
   */
  function goBack(fallback: RouteLocationRaw = { name: 'Home' }): void {
    if (window.history.length > 1) {
      router.back();
    } else {
      router.push(fallback);
    }
  }

  /**
   * Met à jour les query params sans changer de route.
   */
  async function updateQuery(query: Partial<RouteQuery>): Promise<void> {
    await router.replace({
      query: {
        ...route.query,
        ...query,
      },
    });
  }

  /**
   * Supprime un query param.
   */
  async function removeQueryParam(key: keyof RouteQuery): Promise<void> {
    const newQuery = { ...route.query };
    delete newQuery[key];
    await router.replace({ query: newQuery });
  }

  /**
   * Obtient un param typé.
   */
  function getParam<K extends keyof RouteParams>(key: K): RouteParams[K] {
    return route.params[key] as RouteParams[K];
  }

  /**
   * Obtient un query param typé.
   */
  function getQuery<K extends keyof RouteQuery>(key: K): RouteQuery[K] {
    return route.query[key] as RouteQuery[K];
  }

  return {
    router,
    route,
    navigateTo,
    replaceTo,
    goBack,
    updateQuery,
    removeQueryParam,
    getParam,
    getQuery,
    currentRouteName: computed(() => route.name as string),
    currentPath: computed(() => route.path),
    isActive: (name: string) => route.name === name,
  };
}
```

### Usage dans les Composants

```vue
<script setup lang="ts">
import { useAppRouter } from '@/composables/useAppRouter';

const { navigateTo, goBack, getParam, updateQuery, isActive } = useAppRouter();

// Navigation simple
async function goToUser(id: string): Promise<void> {
  await navigateTo('UserDetail', { id });
}

// Navigation avec query
async function search(term: string): Promise<void> {
  await updateQuery({ search: term, page: '1' });
}

// Récupérer un param
const userId = getParam('id');

// Vérifier la route active
const isHomeActive = isActive('Home');
</script>

<template>
  <nav>
    <button @click="goBack()">Retour</button>
    <RouterLink 
      :to="{ name: 'Home' }" 
      :class="{ active: isHomeActive }"
    >
      Accueil
    </RouterLink>
  </nav>
</template>
```

## 🔄 Transitions de Route

### Configuration des Transitions

```vue
<!-- App.vue -->
<script setup lang="ts">
import { useRoute } from 'vue-router';
import { computed } from 'vue';

const route = useRoute();

// Transition différente selon la meta
const transitionName = computed(() => {
  return (route.meta.transition as string) || 'fade';
});
</script>

<template>
  <RouterView v-slot="{ Component, route: currentRoute }">
    <Transition :name="transitionName" mode="out-in">
      <component 
        :is="Component" 
        :key="currentRoute.path"
      />
    </Transition>
  </RouterView>
</template>

<style>
/* Fade */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Slide */
.slide-enter-active,
.slide-leave-active {
  transition: all 0.3s ease;
}

.slide-enter-from {
  opacity: 0;
  transform: translateX(30px);
}

.slide-leave-to {
  opacity: 0;
  transform: translateX(-30px);
}

/* Scale */
.scale-enter-active,
.scale-leave-active {
  transition: all 0.2s ease;
}

.scale-enter-from,
.scale-leave-to {
  opacity: 0;
  transform: scale(0.95);
}
</style>
```

## 📊 Route Props

### Props depuis les Params

```typescript
// routes.ts
{
  path: '/users/:id',
  name: 'UserDetail',
  component: () => import('@/views/users/UserDetailView.vue'),
  props: true, // Passe automatiquement les params comme props
}
```

```vue
<!-- UserDetailView.vue -->
<script setup lang="ts">
interface Props {
  id: string;
}

const props = defineProps<Props>();

// props.id est directement disponible
</script>
```

### Props Fonction

```typescript
// routes.ts
{
  path: '/users/:id',
  name: 'UserDetail',
  component: () => import('@/views/users/UserDetailView.vue'),
  props: (route) => ({
    id: route.params.id,
    edit: route.query.edit === 'true',
  }),
}
```

### Props Objet Statique

```typescript
// routes.ts
{
  path: '/about',
  name: 'About',
  component: () => import('@/views/AboutView.vue'),
  props: {
    version: '1.0.0',
    showContact: true,
  },
}
```

## 🍞 Breadcrumbs

### Composable Breadcrumb

```typescript
// composables/useBreadcrumb.ts
import { computed } from 'vue';
import { useRoute, useRouter, type RouteLocationMatched } from 'vue-router';

export interface BreadcrumbItem {
  label: string;
  path: string;
  name: string;
  isActive: boolean;
}

/**
 * Composable pour générer le fil d'Ariane.
 */
export function useBreadcrumb() {
  const route = useRoute();
  const router = useRouter();

  const breadcrumbs = computed<BreadcrumbItem[]>(() => {
    const matched = route.matched.filter(
      (r) => r.meta.breadcrumb
    );

    return matched.map((record, index) => {
      const isLast = index === matched.length - 1;
      const path = router.resolve({
        name: record.name as string,
        params: route.params,
      }).href;

      return {
        label: record.meta.breadcrumb as string,
        path,
        name: record.name as string,
        isActive: isLast,
      };
    });
  });

  return { breadcrumbs };
}
```

### Composant Breadcrumb

```vue
<!-- components/AppBreadcrumb.vue -->
<script setup lang="ts">
import { useBreadcrumb } from '@/composables/useBreadcrumb';

const { breadcrumbs } = useBreadcrumb();
</script>

<template>
  <nav aria-label="Fil d'Ariane" class="breadcrumb">
    <ol>
      <li>
        <RouterLink :to="{ name: 'Home' }">
          <HomeIcon class="icon" />
          <span class="sr-only">Accueil</span>
        </RouterLink>
      </li>
      <li
        v-for="crumb in breadcrumbs"
        :key="crumb.name"
        :aria-current="crumb.isActive ? 'page' : undefined"
      >
        <ChevronRightIcon class="separator" />
        <RouterLink
          v-if="!crumb.isActive"
          :to="crumb.path"
        >
          {{ crumb.label }}
        </RouterLink>
        <span v-else>{{ crumb.label }}</span>
      </li>
    </ol>
  </nav>
</template>
```

## ⚠️ Bonnes Pratiques

### Lazy Loading

```typescript
// ✅ BON : Lazy loading des composants
{
  path: '/dashboard',
  component: () => import('@/views/DashboardView.vue'),
}

// ✅ BON : Grouper les chunks par feature
{
  path: '/admin',
  component: () => import(/* webpackChunkName: "admin" */ '@/views/admin/AdminLayout.vue'),
  children: [
    {
      path: 'users',
      component: () => import(/* webpackChunkName: "admin" */ '@/views/admin/UsersView.vue'),
    },
  ],
}

// ❌ MAUVAIS : Import direct (charge tout au démarrage)
import DashboardView from '@/views/DashboardView.vue';
{
  path: '/dashboard',
  component: DashboardView,
}
```

### Gestion des Erreurs

```typescript
// router/index.ts
router.onError((error, to, from) => {
  // Log l'erreur
  console.error('Router error:', error);
  
  // Erreur de chargement de chunk (lazy loading)
  if (error.message.includes('Failed to fetch dynamically imported module')) {
    // Recharger la page pour obtenir les nouveaux chunks
    window.location.href = to.fullPath;
    return;
  }
  
  // Autres erreurs
  router.push({ name: 'Error', query: { message: error.message } });
});
```
