# Guide Technique - Connexion Frontend/Backend API

## Architecture de Communication

```
┌─────────────────┐      Proxy Vite       ┌──────────────────┐      MediatR/CQRS    ┌────────────────┐
│  Vue 3 Frontend │  ══════════════════>  │  ASP.NET Core    │  ══════════════════> │  PostgreSQL    │
│  (Port 3001)    │   /api → :5001        │  Admin API       │   EF Core Queries    │  (Port 15432)  │
│                 │                        │  (Port 5001)     │                      │                │
│  - Pinia Stores │                        │  - Controllers   │                      │  - DB Tables   │
│  - Axios Client │                        │  - JWT Auth      │                      │  - Schemas     │
│  - Components   │                        │  - CORS Policy   │                      │                │
└─────────────────┘                        └──────────────────┘                      └────────────────┘
```

## Flux de Données

### 1. Requête Frontend → Backend

```typescript
// 1. Component appelle le store
const tenantsStore = useTenantsStore()
await tenantsStore.loadTenants()

// 2. Store appelle l'API
import { fetchTenants } from '@/api/tenants'
const tenants = await fetchTenants()

// 3. API fait appel HTTP avec axios
const response = await apiClient.get<Tenant[]>(`/${API_CONFIG.API_VERSION}/tenants`)

// 4. Request interceptor ajoute JWT token
config.headers.Authorization = `Bearer ${localStorage.getItem('auth_token')}`

// 5. Proxy Vite transforme la requête
// GET /api/v2025-12-22/tenants → GET http://localhost:5001/api/v2025-12-22/tenants

// 6. Backend ASP.NET reçoit et traite
[HttpGet]
public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
{
    var query = new GetAllTenantsQuery();
    var result = await _mediator.Send(query, cancellationToken);
    return Ok(result.Value);
}

// 7. Backend retourne JSON
{ "value": [...], "isSuccess": true }

// 8. Response interceptor vérifie le statut
// Si 401 → Redirection vers /login
// Si 200 → Retour des données

// 9. Store met à jour l'état
tenants.value = await fetchTenants()

// 10. Vue réactive met à jour l'UI
<template>
  <div v-for="tenant in tenants" :key="tenant.id">...</div>
</template>
```

### 2. Gestion des Erreurs

```typescript
// Erreur 401 - Non authentifié
apiClient.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      localStorage.removeItem('auth_token')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

// Erreur 400 - Validation
{
  "isSuccess": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Le nom du tenant est requis"
  }
}

// Erreur 500 - Serveur
{
  "isSuccess": false,
  "error": {
    "code": "INTERNAL_ERROR",
    "message": "Une erreur interne est survenue"
  }
}
```

## Configuration par Environnement

### Mode Développement (Mock Data)

`.env.development`
```env
VITE_USE_MOCK_DATA=true
VITE_API_BASE_URL=/api
VITE_API_VERSION=v2025-12-22
```

**Avantages :**
- ✅ Développement frontend sans backend
- ✅ Données prévisibles et contrôlées
- ✅ Pas de dépendance PostgreSQL
- ✅ Tests rapides

**Utilisation :**
```typescript
if (API_CONFIG.USE_MOCK_DATA) {
  return getMockTenants()
}
```

### Mode Production (API Réelle)

`.env.production`
```env
VITE_USE_MOCK_DATA=false
VITE_API_BASE_URL=https://api.llmproxy.com
VITE_API_VERSION=v2025-12-22
```

**Avantages :**
- ✅ Données réelles de la base
- ✅ Validation complète backend
- ✅ Tests de bout en bout

## Authentification JWT

### Workflow Complet

```typescript
// 1. Login
const response = await login({
  email: 'admin@example.com',
  password: 'SecurePassword123!'
})

// 2. Stockage du token
localStorage.setItem('auth_token', response.token)
apiClient.setAuthToken(response.token)

// 3. Stockage des infos utilisateur
localStorage.setItem('user_info', JSON.stringify(response.user))

// 4. Requêtes suivantes incluent automatiquement le token
// Header: Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

// 5. Backend valide le token JWT
[Authorize(Policy = "AdminOnly")]
public class TenantsController : ControllerBase { ... }

// 6. Si token expiré/invalide → 401 → Redirection /login
```

### Structure du Token JWT

```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-id-123",
    "email": "admin@example.com",
    "role": "Admin",
    "exp": 1703347200,
    "iss": "LLMProxyAdminAPI",
    "aud": "LLMProxyClients"
  }
}
```

## Mapping Backend ↔ Frontend

### Tenants

| Frontend Type | Backend Type | Endpoint |
|---------------|--------------|----------|
| `Tenant` | `TenantDto` | GET `/api/v{version}/tenants` |
| `CreateTenantRequest` | `CreateTenantCommand` | POST `/api/v{version}/tenants` |
| `UpdateTenantRequest` | `UpdateTenantSettingsCommand` | PUT `/api/v{version}/tenants/{id}/settings` |

### Providers

| Frontend Type | Backend Type | Endpoint |
|---------------|--------------|----------|
| `Provider` | `ProviderDto` | GET `/api/v{version}/providers` |
| `CreateProviderRequest` | `CreateProviderCommand` | POST `/api/v{version}/providers` |
| `UpdateProviderRequest` | `UpdateProviderCommand` | PUT `/api/v{version}/providers/{id}` |

### API Keys

| Frontend Type | Backend Type | Endpoint |
|---------------|--------------|----------|
| `TenantApiKey` | `ApiKeyDto` | GET `/api/v{version}/apikeys/tenant/{id}` |
| `CreateApiKeyResult` | `CreateApiKeyResult` | POST `/api/v{version}/apikeys` |

## Patterns et Bonnes Pratiques

### 1. Result Pattern (Backend C#)

```csharp
// Backend retourne toujours un Result<T>
public async Task<IActionResult> GetAll()
{
    var result = await _mediator.Send(query);
    
    if (!result.IsSuccess)
        return BadRequest(result.Error);
    
    return Ok(result.Value);
}
```

```typescript
// Frontend gère le Result pattern
const response = await apiClient.get<Tenant[]>('/tenants')
// response.data contient directement les données (axios extrait response.data)
```

### 2. Store Pattern (Frontend Pinia)

```typescript
export const useTenantsStore = defineStore('tenants', () => {
  const tenants = ref<Tenant[]>([])
  const isLoading = ref(false)
  const error = ref<string | null>(null)
  
  async function loadTenants() {
    isLoading.value = true
    error.value = null
    try {
      tenants.value = await fetchTenants()
    } catch (err) {
      error.value = err.message
    } finally {
      isLoading.value = false
    }
  }
  
  return { tenants, isLoading, error, loadTenants }
})
```

### 3. Composition API Pattern

```vue
<script setup lang="ts">
import { onMounted } from 'vue'
import { useTenantsStore } from '@/stores/tenants'

const tenantsStore = useTenantsStore()

onMounted(() => {
  tenantsStore.loadTenants()
})
</script>

<template>
  <div v-if="tenantsStore.isLoading">Chargement...</div>
  <div v-else-if="tenantsStore.error">{{ tenantsStore.error }}</div>
  <div v-else>
    <div v-for="tenant in tenantsStore.tenants" :key="tenant.id">
      {{ tenant.name }}
    </div>
  </div>
</template>
```

## Débogage

### Activer les logs réseau

```typescript
// Dans src/api/client.ts
apiClient.interceptors.request.use(config => {
  console.log('🔵 REQUEST:', config.method?.toUpperCase(), config.url)
  console.log('   Headers:', config.headers)
  return config
})

apiClient.interceptors.response.use(response => {
  console.log('🟢 RESPONSE:', response.status, response.config.url)
  console.log('   Data:', response.data)
  return response
})
```

### Vérifier le token JWT

```javascript
// Dans la console navigateur
const token = localStorage.getItem('auth_token')
const payload = JSON.parse(atob(token.split('.')[1]))
console.log('JWT Payload:', payload)
console.log('Expires:', new Date(payload.exp * 1000))
```

### Tester directement l'API

```powershell
# Via curl
curl -X GET "http://localhost:5001/api/v2025-12-22/tenants" `
  -H "Authorization: Bearer YOUR_TOKEN_HERE" `
  -H "Content-Type: application/json"

# Via Swagger UI
# Ouvrir http://localhost:5001/swagger
# Cliquer sur "Authorize"
# Entrer le token JWT
```

## Performances

### Optimisations Axios

```typescript
// Cache des requêtes GET (si applicable)
import axios from 'axios'
import { setupCache } from 'axios-cache-interceptor'

const apiClient = setupCache(axios.create({
  baseURL: API_CONFIG.BASE_URL,
  timeout: API_CONFIG.TIMEOUT
}), {
  ttl: 5 * 60 * 1000, // 5 minutes
  methods: ['get']
})
```

### Debounce des requêtes

```typescript
import { debounce } from 'lodash-es'

const searchTenants = debounce(async (query: string) => {
  const results = await fetchTenants({ search: query })
  tenants.value = results
}, 300)
```

### Pagination

```typescript
// Backend supporte la pagination
GET /api/v2025-12-22/tenants?page=1&pageSize=20

// Frontend gère le state de pagination
const currentPage = ref(1)
const pageSize = ref(20)

async function loadPage(page: number) {
  currentPage.value = page
  await loadTenants({ page, pageSize: pageSize.value })
}
```

## Sécurité

### ✅ Bonnes Pratiques Implémentées

1. **HTTPS en production** : Toujours utiliser HTTPS pour l'API en production
2. **Token dans localStorage** : Accessible uniquement via JavaScript (pas de cookies HttpOnly pour SPA)
3. **Validation CORS** : Backend limite les origines autorisées
4. **Expiration JWT** : Tokens avec durée de vie limitée (60 min par défaut)
5. **Refresh Token** : Renouvellement automatique avant expiration
6. **Gestion 401** : Déconnexion automatique si token invalide

### ⚠️ Considérations de Sécurité

1. **XSS** : Toujours échapper les données utilisateur dans le DOM
2. **CSRF** : Pas de risque avec JWT Bearer (pas de cookies)
3. **Injection SQL** : Backend utilise EF Core avec paramètres (protection native)
4. **Rate Limiting** : Backend implémente le throttling (ADR-041)

## Checklist de Déploiement

### Frontend
- [ ] Modifier `.env.production` avec l'URL de l'API production
- [ ] Définir `VITE_USE_MOCK_DATA=false`
- [ ] Build production : `npm run build`
- [ ] Tester le build : `npm run preview`
- [ ] Vérifier que les assets sont optimisés (gzip, minification)

### Backend
- [ ] Vérifier `appsettings.Production.json` :
  - [ ] Connection string PostgreSQL production
  - [ ] JWT secret sécurisé (256 bits minimum)
  - [ ] CORS avec domaines frontend production
  - [ ] Logging configuré (Serilog, Application Insights)
- [ ] Migrations de base de données appliquées
- [ ] Health checks configurés
- [ ] Rate limiting activé

### Infrastructure
- [ ] PostgreSQL accessible et sécurisé
- [ ] Redis pour cache/session (si applicable)
- [ ] Reverse proxy (nginx/IIS) configuré
- [ ] Certificats SSL valides
- [ ] Monitoring (APM, logs centralisés)

---

**Documentation créée le :** 2025-12-22  
**Version API :** v2025-12-22  
**Compatibilité :** Vue 3.5+, .NET 9.0+
