# Tâche 060 - Créer Admin UI React + Fluent UI

## PRIORITÉ
🟠 **P2 - HAUTE** (Priorité 5/8 de la refonte)

## OBJECTIF

Développer une interface d'administration React avec Fluent UI React et Vite pour configurer le proxy LLM : gestion des providers, tenants, quotas, routes, et monitoring.

## CONTEXTE

### Stack Technique Choisie
- **Framework** : React 18+
- **Design System** : Fluent UI React (Microsoft)
- **Build Tool** : Vite
- **Language** : TypeScript
- **State Management** : TanStack Query (React Query) + Zustand
- **Routing** : React Router v6

### Fonctionnalités Admin UI
1. Dashboard (métriques temps réel)
2. Gestion Providers LLM (CRUD, health status)
3. Gestion Tenants (CRUD, quotas, API keys)
4. Gestion Routes YARP (configuration dynamique)
5. Monitoring (logs, traces, métriques)
6. Configuration (rate limits, feature flags)

## IMPLÉMENTATION

### Phase 1 : Project Setup
```
src/Presentation/LLMProxy.Admin.Web/
├── package.json
├── vite.config.ts
├── tsconfig.json
├── index.html
├── src/
│   ├── main.tsx
│   ├── App.tsx
│   ├── api/
│   │   ├── client.ts           # Axios/fetch wrapper
│   │   ├── providers.api.ts
│   │   ├── tenants.api.ts
│   │   └── routes.api.ts
│   ├── components/
│   │   ├── layout/
│   │   │   ├── AppShell.tsx
│   │   │   ├── NavRail.tsx
│   │   │   └── Header.tsx
│   │   ├── providers/
│   │   │   ├── ProviderCard.tsx
│   │   │   ├── ProviderForm.tsx
│   │   │   └── ProviderHealthBadge.tsx
│   │   ├── tenants/
│   │   │   ├── TenantList.tsx
│   │   │   ├── TenantDetail.tsx
│   │   │   └── QuotaEditor.tsx
│   │   └── common/
│   │       ├── DataGrid.tsx
│   │       ├── ConfirmDialog.tsx
│   │       └── LoadingSpinner.tsx
│   ├── pages/
│   │   ├── Dashboard.tsx
│   │   ├── Providers.tsx
│   │   ├── Tenants.tsx
│   │   ├── Routes.tsx
│   │   ├── Monitoring.tsx
│   │   └── Settings.tsx
│   ├── hooks/
│   │   ├── useProviders.ts
│   │   ├── useTenants.ts
│   │   └── useMetrics.ts
│   ├── stores/
│   │   └── authStore.ts
│   └── types/
│       ├── provider.ts
│       ├── tenant.ts
│       └── route.ts
```

### Phase 2 : Dépendances
```json
{
  "dependencies": {
    "@fluentui/react-components": "^9.46.0",
    "@fluentui/react-icons": "^2.0.230",
    "@tanstack/react-query": "^5.28.0",
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "react-router-dom": "^6.22.0",
    "zustand": "^4.5.0",
    "axios": "^1.6.0"
  },
  "devDependencies": {
    "@types/react": "^18.2.0",
    "@types/react-dom": "^18.2.0",
    "@vitejs/plugin-react": "^4.2.0",
    "typescript": "^5.3.0",
    "vite": "^5.1.0"
  }
}
```

### Phase 3 : Design System Setup
```tsx
// App.tsx
import { FluentProvider, webLightTheme, webDarkTheme } from '@fluentui/react-components';

export const App = () => {
  const [isDark, setIsDark] = useState(false);
  
  return (
    <FluentProvider theme={isDark ? webDarkTheme : webLightTheme}>
      <QueryClientProvider client={queryClient}>
        <RouterProvider router={router} />
      </QueryClientProvider>
    </FluentProvider>
  );
};
```

### Phase 4 : Integration Backend
- Proxy Vite vers Admin.API (dev)
- CORS configuré sur Admin.API
- Authentication JWT Bearer

## CRITÈRES DE SUCCÈS

- [ ] Projet Vite + React + TypeScript initialisé
- [ ] Fluent UI React configuré avec thème clair/sombre
- [ ] Layout avec navigation (NavRail + Header)
- [ ] Page Dashboard avec widgets métriques
- [ ] Page Providers avec DataGrid et CRUD
- [ ] Page Tenants avec gestion quotas
- [ ] Page Routes pour configuration YARP
- [ ] React Query pour data fetching
- [ ] Responsive design
- [ ] Build production optimisé
- [ ] Documentation README

## DÉPENDANCES

- Admin.API fonctionnel avec endpoints REST
- Tâche 059 (Vertical Slices) pour endpoints

## ESTIMATION

**Effort** : 16h
**Complexité** : Moyenne-Haute

## RÉFÉRENCES

- [Fluent UI React](https://react.fluentui.dev/)
- [Vite](https://vitejs.dev/)
- [TanStack Query](https://tanstack.com/query/latest)
