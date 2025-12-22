# Tâche 028 - Implémenter ADR-037 : API Versioning

**Contexte** : L'ADR-037 définit la stratégie de versioning d'API REST pour permettre l'évolution progressive sans breaking changes. Conformité actuelle : **0%** (aucun versioning implémenté).

## OBJECTIF

Implémenter le versioning d'API conforme à ADR-037 : passer de **0%** à **90%** de conformité.

## JUSTIFICATION

### Problème Actuel

Actuellement, **AUCUN versioning** n'est implémenté :
- ❌ Endpoints sans version (`/api/users` au lieu de `/api/v1/users`)
- ❌ Impossible d'évoluer API sans casser clients existants
- ❌ Pas de dépréciation progressive
- ❌ Pas de support multi-version en production

### Impact Business

- 🔴 **BLOQUANT** pour évolution API future
- 🔴 **RISQUE** : Tout changement d'API casse les clients
- 🟡 **MAINTENABILITÉ** : Impossible de supporter plusieurs versions en parallèle

### Criticité

**Priorité HAUTE** - Fondamental pour architecture API REST évolutive.

## CRITÈRES DE SUCCÈS

- [ ] **Packages** : `Asp.Versioning.Http` v9.0.0 ajouté
- [ ] **URL Versioning** : Tous endpoints sous `/api/v{version:apiVersion}/...`
- [ ] **ApiVersion attributes** : `[ApiVersion("1.0")]` sur tous contrôleurs
- [ ] **Multiple versions** : Support v1.0 + v2.0 en parallèle (démo)
- [ ] **Dépréciation** : `[ApiVersion("1.0", Deprecated = true)]` fonctionnel
- [ ] **Default version** : v1.0 par défaut si non spécifiée
- [ ] **Documentation** : README mis à jour avec exemples versioning
- [ ] **Tests** : 10+ tests validant routing versionné
- [ ] **Build** : ✅ 0 erreurs, 0 warnings
- [ ] **Tests** : ✅ 100% passing

## PÉRIMÈTRE

### Fichiers à Créer (3)

1. **`ApiVersioningConfiguration.cs`** (Infrastructure)
   - Extension `AddApiVersioningConfiguration()`
   - Configuration URL versioning
   - Default version 1.0
   - Deprecated version reporting

2. **`ApiVersioningTests.cs`** (Tests)
   - Tests routing versionné
   - Tests default version
   - Tests dépréciation
   - Tests multi-version

3. **`v2/UsersController.cs`** (Demo v2)
   - Contrôleur v2.0 exemple
   - Endpoint `/api/v2/users` avec nouveau format

### Fichiers à Modifier (6+)

1. **`Program.cs` (Gateway)**
   - Ajouter `AddApiVersioningConfiguration()`
   - Configurer options Swagger multi-version

2. **`Program.cs` (Admin.API)**
   - Même configuration versioning

3. **Tous contrôleurs existants (6+)** :
   - `UsersController.cs`
   - `TenantsController.cs`
   - `ProvidersController.cs`
   - `QuotasController.cs`
   - `HealthController.cs`
   - `MetricsController.cs`
   
   **Modifications** :
   - Ajouter `[ApiVersion("1.0")]`
   - Changer route de `[Route("api/[controller]")]` à `[Route("api/v{version:apiVersion}/[controller]")]`

4. **`.csproj` (Gateway + Admin.API)** :
   - Ajouter `<PackageReference Include="Asp.Versioning.Http" Version="9.0.0" />`

5. **`appsettings.json` (Gateway + Admin.API)** :
   - Configurer `ApiVersioning` section

6. **`README.md`** :
   - Section API Versioning
   - Exemples requêtes versionnées
   - Politique dépréciation

## ÉTAPES D'IMPLÉMENTATION

### Étape 1 : Packages et Configuration (2h)

**Actions** :
1. Ajouter package `Asp.Versioning.Http` v9.0.0 (Gateway + Admin.API)
2. Créer `ApiVersioningConfiguration.cs` :
   ```csharp
   public static class ApiVersioningConfiguration
   {
       public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
       {
           services.AddApiVersioning(options =>
           {
               // Versioning par URL : /api/v{version}/...
               options.ApiVersionReader = new UrlSegmentApiVersionReader();
               
               // Version par défaut si non spécifiée
               options.DefaultApiVersion = new ApiVersion(1, 0);
               options.AssumeDefaultVersionWhenUnspecified = true;
               
               // Reporter versions supportées dans headers
               options.ReportApiVersions = true;
           })
           .AddApiExplorer(options =>
           {
               // Format version dans URL : 'v'major[.minor]
               options.GroupNameFormat = "'v'VVV";
               options.SubstituteApiVersionInUrl = true;
           });
           
           return services;
       }
   }
   ```
3. Ajouter dans `Program.cs` (Gateway + Admin.API) :
   ```csharp
   builder.Services.AddApiVersioningConfiguration();
   ```
4. Build + Test

**Commit** : `feat(api-versioning): Add Asp.Versioning.Http package and configuration`

---

### Étape 2 : Migrer Contrôleurs vers v1.0 (3h)

**Actions** :
1. **Pour chaque contrôleur existant** :
   - Ajouter `using Asp.Versioning;`
   - Ajouter `[ApiVersion("1.0")]` sur la classe
   - Changer route : `[Route("api/v{version:apiVersion}/[controller]")]`
   
   **Exemple** :
   ```csharp
   [ApiController]
   [ApiVersion("1.0")]
   [Route("api/v{version:apiVersion}/[controller]")]
   public class UsersController : ControllerBase
   {
       // Endpoints inchangés, maintenant sous /api/v1/users
   }
   ```

2. Lister contrôleurs à migrer :
   - `UsersController` (Gateway)
   - `TenantsController` (Gateway)
   - `ProvidersController` (Admin.API)
   - `QuotasController` (Admin.API)
   - `HealthController` (Gateway + Admin.API)
   - `MetricsController` (Admin.API)

3. Build + Test après chaque contrôleur

**Commit** : `feat(api-versioning): Migrate controllers to v1.0 URL versioning`

---

### Étape 3 : Support Multi-Version (Démo v2) (2h)

**Actions** :
1. Créer **contrôleur v2.0** exemple :
   ```csharp
   // Fichier: UsersControllerV2.cs
   [ApiController]
   [ApiVersion("2.0")]
   [Route("api/v{version:apiVersion}/users")]
   public class UsersControllerV2 : ControllerBase
   {
       [HttpGet]
       public async Task<ActionResult<UsersResponseV2>> GetUsers()
       {
           // Format v2 : Pagination + Metadata
           return Ok(new UsersResponseV2
           {
               Data = users,
               Pagination = new { Page = 1, PageSize = 20, Total = 100 },
               Links = new { Self = "/api/v2/users", Next = "/api/v2/users?page=2" }
           });
       }
   }
   
   // v1 retourne List<User>, v2 retourne { data, pagination, links }
   ```

2. Tester appels simultanés v1 + v2 :
   - `GET /api/v1/users` → Format v1 (simple liste)
   - `GET /api/v2/users` → Format v2 (avec pagination)

3. Build + Test

**Commit** : `feat(api-versioning): Add v2.0 example with paginated response`

---

### Étape 4 : Dépréciation de Version (1h)

**Actions** :
1. Marquer v1.0 comme dépréciée :
   ```csharp
   [ApiController]
   [ApiVersion("1.0", Deprecated = true)]
   [Route("api/v{version:apiVersion}/[controller]")]
   public class UsersController : ControllerBase
   {
       // v1.0 fonctionnel mais déprécié
   }
   ```

2. Vérifier header de réponse :
   ```http
   GET /api/v1/users HTTP/1.1
   
   HTTP/1.1 200 OK
   api-supported-versions: 2.0
   api-deprecated-versions: 1.0
   ```

3. Documenter politique dépréciation :
   - v1.0 supportée 6 mois après dépréciation
   - Warning dans logs pour clients v1
   - Migration guide dans README

**Commit** : `feat(api-versioning): Add version deprecation support`

---

### Étape 5 : Tests (3h)

**Actions** :
1. Créer `ApiVersioningTests.cs` :
   ```csharp
   public class ApiVersioningTests : IClassFixture<WebApplicationFactory<Program>>
   {
       [Fact]
       public async Task GetUsers_WithV1_ShouldReturnListFormat()
       {
           var response = await _client.GetAsync("/api/v1/users");
           response.StatusCode.Should().Be(HttpStatusCode.OK);
           
           var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
           users.Should().NotBeNull();
       }
       
       [Fact]
       public async Task GetUsers_WithV2_ShouldReturnPaginatedFormat()
       {
           var response = await _client.GetAsync("/api/v2/users");
           response.StatusCode.Should().Be(HttpStatusCode.OK);
           
           var result = await response.Content.ReadFromJsonAsync<UsersResponseV2>();
           result.Should().NotBeNull();
           result.Data.Should().NotBeNull();
           result.Pagination.Should().NotBeNull();
       }
       
       [Fact]
       public async Task GetUsers_WithoutVersion_ShouldUseDefault()
       {
           var response = await _client.GetAsync("/api/users");
           // Devrait rediriger vers /api/v1/users
           response.RequestMessage.RequestUri.Should().Contain("v1");
       }
       
       [Fact]
       public async Task GetUsers_WithInvalidVersion_ShouldReturn404()
       {
           var response = await _client.GetAsync("/api/v999/users");
           response.StatusCode.Should().Be(HttpStatusCode.NotFound);
       }
       
       [Fact]
       public async Task GetUsers_V1Deprecated_ShouldReturnDeprecationHeader()
       {
           var response = await _client.GetAsync("/api/v1/users");
           response.Headers.Should().ContainKey("api-deprecated-versions");
           response.Headers.GetValues("api-deprecated-versions").Should().Contain("1.0");
       }
   }
   ```

2. Tests requis :
   - [x] Routing v1 fonctionnel
   - [x] Routing v2 fonctionnel
   - [x] Default version (sans version explicite)
   - [x] Version invalide (404)
   - [x] Headers dépréciation
   - [x] Support multi-version simultané
   - [x] Backward compatibility

3. Build + Test complet

**Commit** : `test(api-versioning): Add comprehensive API versioning tests`

---

### Étape 6 : Configuration Production (2h)

**Actions** :
1. Ajouter section `appsettings.json` :
   ```json
   {
     "ApiVersioning": {
       "DefaultVersion": "1.0",
       "ReportApiVersions": true,
       "AssumeDefaultVersionWhenUnspecified": true,
       "DeprecationPolicy": {
         "SunsetPeriodMonths": 6,
         "WarnBeforeSunsetMonths": 3
       }
     }
   }
   ```

2. Swagger multi-version :
   ```csharp
   builder.Services.AddSwaggerGen(options =>
   {
       options.SwaggerDoc("v1", new OpenApiInfo { Title = "LLMProxy API v1", Version = "v1" });
       options.SwaggerDoc("v2", new OpenApiInfo { Title = "LLMProxy API v2", Version = "v2" });
   });
   
   app.UseSwaggerUI(options =>
   {
       options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1 (Deprecated)");
       options.SwaggerEndpoint("/swagger/v2/swagger.json", "API v2");
   });
   ```

3. Logging dépréciation :
   ```csharp
   app.Use(async (context, next) =>
   {
       if (context.GetRequestedApiVersion()?.ToString() == "1.0")
       {
           _logger.LogWarning("Client using deprecated API v1.0 from {IP}", context.Connection.RemoteIpAddress);
       }
       await next();
   });
   ```

**Commit** : `feat(api-versioning): Add production config and Swagger multi-version`

---

### Étape 7 : Documentation (2h)

**Actions** :
1. Ajouter section README :
   ```markdown
   ## API Versioning (ADR-037)
   
   **Stratégie** : URL-based versioning (`/api/v{version}/...`)
   
   **Versions Supportées** :
   - **v2.0** (Recommandée) - Pagination, HATEOAS, HTTP/2
   - **v1.0** (Dépréciée) - Support jusqu'au 2026-06-30
   
   **Exemples Requêtes** :
   
   ```bash
   # Version explicite (recommandé)
   curl https://api.llmproxy.com/api/v2/users
   
   # Sans version (utilise v1 par défaut)
   curl https://api.llmproxy.com/api/users
   
   # Version dépréciée (warning dans response headers)
   curl -I https://api.llmproxy.com/api/v1/users
   # api-supported-versions: 2.0
   # api-deprecated-versions: 1.0
   ```
   
   **Migration v1 → v2** :
   
   | Feature | v1.0 | v2.0 |
   |---------|------|------|
   | Format réponse | `List<T>` | `{ data, pagination, links }` |
   | Pagination | Manuelle | Automatique (query params) |
   | HATEOAS | Non | Oui (`_links`) |
   | HTTP/2 | Non | Oui |
   
   **Politique Dépréciation** :
   - Support 6 mois après dépréciation
   - Warning 3 mois avant sunset
   - Breaking changes uniquement en major version
   ```

2. Créer `docs/API_VERSIONING_GUIDE.md` :
   - Stratégie de versioning détaillée
   - Exemples de migration
   - Changelog par version

**Commit** : `docs(api-versioning): Add API versioning documentation`

---

### Étape 8 : Validation Finale (1h)

**Actions** :
1. Build complet : `dotnet build --no-restore`
   - ✅ 0 erreurs, 0 warnings
2. Tests complets : `dotnet test --no-build`
   - ✅ 100% passing (10+ nouveaux tests)
3. Vérifier routing :
   - `/api/v1/users` → v1 (deprecated)
   - `/api/v2/users` → v2 (recommended)
   - `/api/users` → v1 (default)
   - `/api/v999/users` → 404
4. Vérifier headers dépréciation :
   ```http
   api-supported-versions: 2.0
   api-deprecated-versions: 1.0
   ```
5. Swagger multi-version fonctionnel

**Commit** : `chore(api-versioning): Final validation ADR-037`

---

## DÉPENDANCES

- **Aucune tâche bloquante**
- Package `Asp.Versioning.Http` disponible (NuGet officiel)
- Contrôleurs existants stables

## ESTIMATIONS

### Temps de Développement

| Étape | Durée Estimée | Complexité |
|-------|---------------|------------|
| 1. Packages et Config | 2h | 🟢 SIMPLE |
| 2. Migration Contrôleurs v1 | 3h | 🟡 MOYEN |
| 3. Support Multi-Version | 2h | 🟡 MOYEN |
| 4. Dépréciation | 1h | 🟢 SIMPLE |
| 5. Tests | 3h | 🟡 MOYEN |
| 6. Config Production | 2h | 🟢 SIMPLE |
| 7. Documentation | 2h | 🟢 SIMPLE |
| 8. Validation Finale | 1h | 🟢 SIMPLE |
| **TOTAL** | **16h** | 🟡 MOYEN |

### Risques

| Risque | Impact | Mitigation |
|--------|--------|------------|
| Breaking changes clients | 🔴 CRITIQUE | Conserver v1 en parallèle 6 mois |
| Tests complexes multi-version | 🟡 MOYEN | Utiliser WebApplicationFactory avec config par version |
| Swagger confusion multi-version | 🟢 FAIBLE | Dropdown version dans Swagger UI |

## COMMITS PRÉVUS

1. `feat(api-versioning): Add Asp.Versioning.Http package and configuration`
2. `feat(api-versioning): Migrate controllers to v1.0 URL versioning`
3. `feat(api-versioning): Add v2.0 example with paginated response`
4. `feat(api-versioning): Add version deprecation support`
5. `test(api-versioning): Add comprehensive API versioning tests`
6. `feat(api-versioning): Add production config and Swagger multi-version`
7. `docs(api-versioning): Add API versioning documentation`
8. `chore(api-versioning): Final validation ADR-037`

## NOTES

- **Versioning par URL** choisi (vs Header/Query) : Plus visible, cache-friendly, SEO-friendly
- **Default version 1.0** : Backward compatibility pour clients existants
- **Dépréciation progressive** : 6 mois de support après dépréciation
- **Swagger multi-version** : Dropdown pour switcher entre versions
- **HATEOAS en v2** : Liens hypermedia pour découvrabilité API

---

**Dernière mise à jour** : 2025-12-22  
**Priorité** : 🔴 HAUTE  
**Impact** : Architecture API REST évolutive  
**Conformité Cible** : ADR-037 de 0% → 90%


## TRACKING
Début: 2025-12-22T08:52:22.9943180Z


## TRACKING

Début: 2025-12-22T08:52:22.9943180Z
Durée actuelle: ~3h (en cours)

## PROGRESSION DÉTAILLÉE

###  Step 1: Packages & Configuration (COMPLÉTÉ - 30min)
-  Asp.Versioning.Http v8.1.0 (Gateway + Admin.API)
-  Asp.Versioning.Mvc.ApiExplorer v8.1.0 (Gateway + Admin.API)
-  ApiVersioningConfiguration.cs créé (2 projets)
-  Build réussi
-  Note: v9.0.0 n'existe pas, utilisé v8.1.0

###  Step 2: Intégration Program.cs + Migration v1.0 (COMPLÉTÉ - 1h)
-  Program.cs modifiés (Gateway + Admin.API)
-  4 contrôleurs migrés vers v1.0 (Users, Tenants, Providers, ApiKeys)
-  Routes changées: /api/[controller]  /api/v{version}/[controller]
-  Attributes [ApiVersion("1.0")] ajoutés
-  Build réussi, tests 119/120 passing

###  Step 3: Demo Multi-Version v2.0 (PARTIEL - 1h30)
-  TenantsV2Controller créé (201 lines)
-  Route: /api/v2/tenants (explicite)
-  Features v2: Pagination, métadonnées enrichies, CreatedAtAction
-  Tests créés: 13 tests API versioning
-  Tests: 6/13 passing (7 échouent sur dynamic cast)
-  Projet LLMProxy.Admin.API.Tests créé
-  Problème: Tests v2 utilisent \s dynamic\ qui échoue

###  Step 4-8: RESTE À FAIRE
- [ ] Step 4: Corriger tests API versioning (dynamic  JSON deserialization)
- [ ] Step 5: Support dépréciation ([ApiVersion("1.0", Deprecated = true)])
- [ ] Step 6: Documentation README (exemples versioning)
- [ ] Step 7: Tests production config
- [ ] Step 8: Validation finale + merge

## COMMITS

1. **feat(api-versioning): Add Asp.Versioning packages and configuration** (260a4bc)
   - Packages v8.1.0 ajoutés
   - Configuration classes créées

2. **feat(api-versioning): Integrate versioning in Program.cs and migrate controllers to v1.0** (73979e5)
   - Program.cs intégration
   - 4 contrôleurs migrés v1.0

3. **feat(api-versioning): Add v2.0 example controller and integration tests** (da8784d)
   - TenantsV2Controller avec pagination
   - 13 tests (6/13 passing)

## PROBLÈMES RÉSOLUS

1. **Package v9.0.0 introuvable**  Utilisé v8.1.0 (latest disponible)
2. **AddApiExplorer non trouvé**  Ajouté package Asp.Versioning.Mvc.ApiExplorer
3. **NSubstitute types anonymes**  Changé pour TenantDto réel
4. **Result<Guid> vs Result<TenantDto>**  CreateTenantCommand retourne TenantDto

## PROBLÈME ACTUEL

**Tests v2 échouent** (7/13):
- Cause: \esponse as dynamic\ ne fonctionne pas bien avec JSON anonymes
- Ligne: ApiVersioningTests.cs:176, 249, 280, 302, 315
- Solution nécessaire: Utiliser JsonSerializer pour deserialize réponses

## ÉTAT BUILD

-  Build: 13/13 projets (0 errors, 3 warnings KubernetesClient)
-  Tests: 125/132 passing (7 failed - tests API v2)
-  Conformité ADR-037: **~60%** (au lieu de 90% visé)

## NEXT STEPS

1. **PRIORITÉ HAUTE**: Fixer tests v2 (use JsonSerializer instead of dynamic)
2. Ajouter tests dépréciation
3. Documenter dans README
4. Validation finale