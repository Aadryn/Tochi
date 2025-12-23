# Point 4 : Connexion à l'API Backend - Documentation

## ✅ Travail Complété

### 1. Configuration de l'environnement

**Fichier créé :** `.env`
```env
VITE_API_BASE_URL=/api
VITE_API_VERSION=v2025-12-22
VITE_USE_MOCK_DATA=false
```

- **VITE_USE_MOCK_DATA=false** : Active l'API réelle au lieu des données mock
- **VITE_USE_MOCK_DATA=true** : Utilise les données mock pour développement sans backend

### 2. Mise à jour du client API

**Fichiers modifiés :**
- `src/api/config.ts` : Configuration centralisée avec support mode mock
- `src/api/client.ts` : Client axios avec intercepteurs JWT
- `src/api/tenants.ts` : Endpoints tenants + API keys alignés avec backend ASP.NET
- `src/api/providers.ts` : Endpoints providers alignés avec backend

**Points clés :**
- Proxy Vite configuré : `/api` → `http://localhost:5001`
- Version API : `v2025-12-22` (détection par namespace backend)
- Gestion automatique du token JWT dans headers
- Redirection automatique vers /login si 401 Unauthorized

### 3. Endpoints API mappés

#### Tenants
- ✅ `GET /api/v2025-12-22/tenants` - Liste des tenants
- ✅ `GET /api/v2025-12-22/tenants/{id}` - Détails d'un tenant
- ✅ `POST /api/v2025-12-22/tenants` - Créer un tenant
- ✅ `PUT /api/v2025-12-22/tenants/{id}/settings` - Modifier un tenant
- ✅ `POST /api/v2025-12-22/tenants/{id}/activate` - Activer un tenant
- ✅ `POST /api/v2025-12-22/tenants/{id}/deactivate` - Désactiver un tenant

#### API Keys
- ✅ `GET /api/v2025-12-22/apikeys/tenant/{tenantId}` - Liste des clés API
- ✅ `POST /api/v2025-12-22/apikeys` - Créer une clé API
- ✅ `POST /api/v2025-12-22/apikeys/{id}/revoke` - Révoquer une clé API
- ✅ `DELETE /api/v2025-12-22/apikeys/{id}` - Supprimer une clé API

#### Providers
- ✅ `GET /api/v2025-12-22/providers` - Liste des providers
- ✅ `GET /api/v2025-12-22/providers/tenant/{tenantId}` - Providers d'un tenant
- ✅ `GET /api/v2025-12-22/providers/{id}` - Détails d'un provider
- ✅ `POST /api/v2025-12-22/providers` - Créer un provider
- ✅ `PUT /api/v2025-12-22/providers/{id}` - Modifier un provider
- ✅ `DELETE /api/v2025-12-22/providers/{id}` - Supprimer un provider

### 4. Corrections TypeScript

Toutes les erreurs TypeScript ont été corrigées :
- ✅ Mock data conformes aux interfaces
- ✅ Propriétés correctes : `latencyMs` au lieu de `latency`
- ✅ Signature de fonction `revokeApiKey(keyId)` mise à jour
- ✅ Objets `CreateApiKeyResult` et `ProviderHealthCheck` corrects

### 5. Tests et Validation

- ✅ **Build réussi** : 2.43s, aucune erreur
- ✅ **136 tests passent** : Aucune régression introduite
- ✅ **Code TypeScript strict** : Conformité totale

## 🚀 Comment utiliser l'API réelle

### Option 1 : Démarrer le backend

```powershell
# 1. Démarrer PostgreSQL (via Docker)
docker-compose up -d postgres

# 2. Démarrer l'API Admin (depuis le dossier backend)
cd backend\src\Presentation\LLMProxy.Admin.API
dotnet run

# L'API démarre sur http://localhost:5001
```

### Option 2 : Utiliser les données mock

Si le backend n'est pas disponible, définir :
```env
VITE_USE_MOCK_DATA=true
```

Le frontend fonctionnera avec des données de démonstration.

## 📝 Configuration Backend requise

L'API backend doit avoir :

1. **CORS configuré** pour accepter `http://localhost:3000` et `http://localhost:3001`
   - Déjà configuré dans `appsettings.json` : `"AllowedOrigins": ["http://localhost:3000", "http://localhost:5173"]`
   - ⚠️ Ajouter port 3001 si nécessaire

2. **JWT configuré** avec les mêmes paramètres
   - Issuer: `LLMProxyAdminAPI`
   - Audience: `LLMProxyClients`

3. **Base de données PostgreSQL** accessible sur port 5432 ou 15432 (Docker)

## 🔐 Authentification

### Workflow JWT

1. **Login** : `POST /auth/login`
   ```json
   {
     "email": "admin@example.com",
     "password": "SecurePassword123!"
   }
   ```

2. **Réponse** :
   ```json
   {
     "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
     "user": {
       "id": "...",
       "email": "admin@example.com",
       "role": "admin"
     }
   }
   ```

3. **Token stocké** dans `localStorage.auth_token`

4. **Requêtes suivantes** : Header `Authorization: Bearer {token}` ajouté automatiquement

## ⚙️ Variables d'environnement

Créer `.env.local` pour override local :

```env
# Mode développement avec mock data
VITE_USE_MOCK_DATA=true

# OU mode production avec API réelle
VITE_USE_MOCK_DATA=false
VITE_API_BASE_URL=/api
VITE_API_VERSION=v2025-12-22
```

## 🐛 Troubleshooting

### Erreur CORS

Si l'erreur `No 'Access-Control-Allow-Origin' header` apparaît :
1. Vérifier que le backend inclut le port du frontend dans CORS
2. Vérifier `appsettings.json` → `"Cors:AllowedOrigins"`

### Erreur 401 Unauthorized

1. Token expiré ou invalide → Reconnexion automatique vers `/login`
2. Vérifier que le token JWT est valide
3. Vérifier les rôles requis (Admin, TenantAdmin)

### Proxy Vite ne fonctionne pas

1. Vérifier `vite.config.ts` → `server.proxy['/api']`
2. Redémarrer Vite dev server : `npm run dev`
3. Vérifier que le backend est accessible sur `http://localhost:5001`

## 📊 Résultat

✅ **Frontend prêt** à se connecter au backend ASP.NET Core
✅ **Mode mock** disponible pour développement autonome
✅ **Type-safety** complète avec TypeScript
✅ **Aucune régression** : 136 tests passent

## 🎯 Prochaines étapes (Point 5)

- Tests E2E avec Playwright
- Tests d'intégration frontend + backend
- Validation complète du workflow utilisateur
