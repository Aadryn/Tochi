# ADR-056. Stack Admin UI : React + Fluent UI + Vite

Date: 2025-12-22
Statut: **Proposé**

## Contexte

L'interface d'administration LLMProxy nécessite :
- Gestion des providers LLM, tenants, API keys, routes
- Visualisation des métriques et logs
- Configuration des quotas et rate limits
- Expérience utilisateur moderne et réactive

Le backend est en .NET, mais le choix du frontend est ouvert.

## Décision

Adopter **React 18+** avec **Fluent UI React** (Microsoft) et **Vite** comme build tool.

### Stack Technique

| Composant | Choix | Version |
|-----------|-------|---------|
| Framework | React | 18.2+ |
| Design System | Fluent UI React | 9.x |
| Build Tool | Vite | 5.x |
| Language | TypeScript | 5.x |
| State Management | TanStack Query + Zustand | 5.x / 4.x |
| Routing | React Router | 6.x |
| HTTP Client | Axios | 1.x |

### Justification des Choix

**React vs Vue vs Blazor**
- React : Écosystème le plus riche, compétences disponibles, Fluent UI natif
- Vue : Excellente alternative, mais Fluent UI moins mature
- Blazor : Cohérence C#, mais bundle lourd et complexité SignalR

**Fluent UI vs Autres Design Systems**
- Fluent UI : Native Microsoft, cohérence avec écosystème Azure/M365
- MUI/Ant Design : Populaires mais style Google/Alibaba
- Tailwind : Flexible mais pas de composants prêts à l'emploi

**Vite vs Webpack**
- Vite : Dev server ultra-rapide (ESM natif), config minimale
- Webpack : Plus configurable mais plus lent et complexe

### Structure Projet

```
src/Presentation/LLMProxy.Admin.Web/
├── package.json
├── vite.config.ts
├── tsconfig.json
├── src/
│   ├── main.tsx
│   ├── App.tsx
│   ├── api/           # Clients API
│   ├── components/    # Composants réutilisables
│   ├── pages/         # Pages/routes
│   ├── hooks/         # Custom hooks
│   ├── stores/        # Zustand stores
│   └── types/         # Types TypeScript
```

## Conséquences

### Positives
- Expérience développeur excellente (Vite + React + TS)
- Design System cohérent Microsoft
- Performance optimale (code splitting, lazy loading)
- Écosystème React riche (charting, tables, etc.)
- SEO non requis (admin interne)

### Négatives
- Deux stacks à maintenir (C# backend + TypeScript frontend)
- Divergence de compétences (équipe doit maîtriser les deux)
- Build séparé (CI/CD plus complexe)
- Pas de SSR (acceptable pour admin interne)

## Alternatives Considérées

### Alternative 1 : Blazor Server (MudBlazor)
- Avantages : Tout en C#, MudBlazor déjà utilisé
- Inconvénients : Dépendance SignalR, latence réseau
- Raison du rejet : Expérience utilisateur moins fluide

### Alternative 2 : Blazor WebAssembly
- Avantages : SPA pure, C# partout
- Inconvénients : Bundle lourd (5-10MB), cold start lent
- Raison du rejet : UX initiale dégradée

### Alternative 3 : Vue + Vuetify
- Avantages : Simple, progressive, communauté FR
- Inconvénients : Fluent UI moins mature pour Vue
- Raison du rejet : Préférence pour écosystème Microsoft

## Références

- [Fluent UI React](https://react.fluentui.dev/)
- [Vite](https://vitejs.dev/)
- [TanStack Query](https://tanstack.com/query/latest)
- [React Router](https://reactrouter.com/)
