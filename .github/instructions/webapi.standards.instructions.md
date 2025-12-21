# Instructions - Standards WebApi ASP.NET Core

**Fichier :** `webapi.standards.instructions.md`  
**Objectif :** Définir les standards et bonnes pratiques pour les contrôleurs WebApi ASP.NET Core  
**Scope :** S'applique à tous les fichiers de contrôleurs (`**/Controllers/*.cs`) dans les projets WebApi

---

## 📋 TABLE DES MATIÈRES

1. [Principes Généraux](#principes-généraux)
2. [Ordre de Priorité des Sources de Binding](#ordre-de-priorité-des-sources-de-binding)
3. [Attributs de Routing](#attributs-de-routing)
4. [Documentation XML](#documentation-xml)
5. [Gestion des Erreurs](#gestion-des-erreurs)
6. [Exemples Complets](#exemples-complets)

---

## 🎯 PRINCIPES GÉNÉRAUX

### Règles Fondamentales

1. **TOUJOURS** préférer les paramètres dans cet ordre de priorité :
   - `[FromRoute]` (paramètres dans l'URL)
   - `[FromBody]` (payload JSON)
   - `[FromHeader]` (headers HTTP)
   - `[FromForm]` (formulaires)
   - `[FromQuery]` (query string)

2. **JAMAIS** utiliser `[FromQuery]` pour des identifiants qui devraient être dans la route

3. **TOUJOURS** être explicite sur la source du binding (ne pas compter sur l'inférence)

### Justification de l'Ordre de Priorité

#### 1. `[FromRoute]` - Premier Choix

**Avantages :**
- URLs RESTful et sémantiques (`/users/123/collections/456`)
- Meilleure découvrabilité dans Swagger/OpenAPI
- Cache HTTP plus efficace (les URLs sont les clés de cache)
- Évite les conflits de routes (routes uniques garanties)
- Convention REST standard

**Cas d'usage :**
- Identifiants de ressources (`userId`, `collectionId`, `promptId`)
- Hiérarchies de ressources (`/users/{userId}/orders/{orderId}`)
- Actions sur ressources spécifiques

```csharp
// ✅ BON : Identifiants dans la route
[HttpGet("{collectionId:guid}")]
public async Task<ActionResult<CollectionResponse>> GetCollection(
    [FromRoute] Guid userId,
    [FromRoute] Guid collectionId)

// ❌ MAUVAIS : Identifiant en query string
[HttpGet]
public async Task<ActionResult<CollectionResponse>> GetCollection(
    [FromRoute] Guid userId,
    [FromQuery] Guid collectionId)  // Crée conflit de route
```

#### 2. `[FromBody]` - Deuxième Choix

**Avantages :**
- Structure de données complexe (objets, listes)
- Validation automatique via Data Annotations
- Sérialisation/désérialisation JSON robuste
- Pas de limite de taille (contrairement à query string)

**Cas d'usage :**
- Créer/modifier des ressources (POST, PUT, PATCH)
- Payload avec plusieurs propriétés
- Données sensibles (non loggées dans les URLs)

```csharp
// ✅ BON : Payload complexe
[HttpPost]
public async Task<ActionResult<CollectionResponse>> CreateCollection(
    [FromRoute] Guid userId,
    [FromBody] CreateCollectionRequest request)  // Objet complet

// ❌ MAUVAIS : Multiples paramètres query pour données complexes
[HttpPost]
public async Task<ActionResult<CollectionResponse>> CreateCollection(
    [FromRoute] Guid userId,
    [FromQuery] string name,
    [FromQuery] string? description,
    [FromQuery] bool isPublic)  // Difficile à maintenir
```

#### 3. `[FromHeader]` - Troisième Choix

**Avantages :**
- Métadonnées HTTP standardisées
- Authentification/autorisation (tokens)
- Versioning d'API
- Traçabilité (correlation IDs, request IDs)

**Cas d'usage :**
- Tokens d'authentification (`Authorization`)
- Clés API (`X-Api-Key`)
- IDs de corrélation (`X-Correlation-Id`)
- Préférences client (`Accept-Language`, `Accept`)

```csharp
// ✅ BON : Token dans header
[HttpGet]
public async Task<ActionResult> GetSecureResource(
    [FromRoute] Guid resourceId,
    [FromHeader(Name = "Authorization")] string authorization,
    [FromHeader(Name = "X-Correlation-Id")] string? correlationId = null)

// ❌ MAUVAIS : Token en query string (exposition dans logs)
[HttpGet]
public async Task<ActionResult> GetSecureResource(
    [FromRoute] Guid resourceId,
    [FromQuery] string token)  // Sécurité compromise
```

#### 4. `[FromForm]` - Quatrième Choix

**Avantages :**
- Upload de fichiers (multipart/form-data)
- Formulaires HTML traditionnels
- Combinaison fichiers + métadonnées

**Cas d'usage :**
- Upload de fichiers (`IFormFile`)
- Formulaires avec fichiers attachés
- Applications hybrides (non-SPA)

```csharp
// ✅ BON : Upload de fichier
[HttpPost]
public async Task<ActionResult> UploadDocument(
    [FromRoute] Guid userId,
    [FromForm] IFormFile file,
    [FromForm] string? description = null)

// ❌ MAUVAIS : Fichier en base64 dans body JSON
[HttpPost]
public async Task<ActionResult> UploadDocument(
    [FromRoute] Guid userId,
    [FromBody] FileUploadRequest request)  // Base64 inefficace
```

#### 5. `[FromQuery]` - Cinquième Choix

**Avantages :**
- Paramètres optionnels de filtrage/pagination
- Facile à tester manuellement (visible dans l'URL)
- Compatible avec GET (pas de body)
- Bookmarkable et partageable

**Cas d'usage :**
- Filtres de recherche (`searchTerm`, `category`)
- Pagination (`page`, `pageSize`)
- Tri (`sortBy`, `sortOrder`)
- Options d'affichage (`includeDeleted`, `expand`)

```csharp
// ✅ BON : Filtres optionnels
[HttpGet]
public async Task<ActionResult<IEnumerable<CollectionResponse>>> GetCollections(
    [FromRoute] Guid userId,
    [FromQuery] string? searchTerm = null,      // Filtre optionnel
    [FromQuery] int page = 1,                   // Pagination
    [FromQuery] int pageSize = 20)

// ❌ MAUVAIS : Identifiant obligatoire en query
[HttpGet]
public async Task<ActionResult<CollectionResponse>> GetCollection(
    [FromRoute] Guid userId,
    [FromQuery] Guid collectionId)  // Devrait être [FromRoute]
```

---

## 🔗 ORDRE DE PRIORITÉ DES SOURCES DE BINDING

### Tableau Décisionnel

| Type de Paramètre | Source Binding | Priorité | Cas d'Usage |
|-------------------|----------------|----------|-------------|
| **Identifiant de ressource** | `[FromRoute]` | 1️⃣ | `/users/{userId}`, `/collections/{collectionId}` |
| **Payload création/modification** | `[FromBody]` | 2️⃣ | POST, PUT, PATCH avec objet complexe |
| **Tokens/Credentials** | `[FromHeader]` | 3️⃣ | `Authorization: Bearer ...`, `X-Api-Key` |
| **Upload fichiers** | `[FromForm]` | 4️⃣ | Multipart form data, IFormFile |
| **Filtres/Recherche** | `[FromQuery]` | 5️⃣ | `?searchTerm=...`, `?category=...` |
| **Pagination** | `[FromQuery]` | 5️⃣ | `?page=1&pageSize=20` |
| **Tri** | `[FromQuery]` | 5️⃣ | `?sortBy=name&sortOrder=asc` |

### Règles de Décision Rapide

**QUESTION 1 : Est-ce un identifiant de ressource ?**
- ✅ OUI → `[FromRoute]` (ex: `userId`, `collectionId`, `orderId`)
- ❌ NON → Passer à la question suivante

**QUESTION 2 : Est-ce un objet complexe ou des données sensibles ?**
- ✅ OUI → `[FromBody]` (ex: création/modification, payload JSON)
- ❌ NON → Passer à la question suivante

**QUESTION 3 : Est-ce une métadonnée HTTP ou authentification ?**
- ✅ OUI → `[FromHeader]` (ex: `Authorization`, `X-Api-Key`, `X-Correlation-Id`)
- ❌ NON → Passer à la question suivante

**QUESTION 4 : Est-ce un upload de fichier ou formulaire ?**
- ✅ OUI → `[FromForm]` (ex: `IFormFile`, multipart/form-data)
- ❌ NON → Passer à la question suivante

**QUESTION 5 : Est-ce un filtre, pagination ou paramètre optionnel ?**
- ✅ OUI → `[FromQuery]` (ex: `searchTerm`, `page`, `sortBy`)
- ❌ NON → Revoir l'architecture de l'endpoint

---

## 🛣️ ATTRIBUTS DE ROUTING

### Contraintes de Route

**TOUJOURS** utiliser des contraintes de type pour les identifiants :

```csharp
// ✅ BON : Contraintes explicites
[HttpGet("{userId:guid}/collections/{collectionId:guid}")]
public async Task<ActionResult> GetCollection(
    [FromRoute] Guid userId,
    [FromRoute] Guid collectionId)

// ❌ MAUVAIS : Pas de contraintes
[HttpGet("{userId}/collections/{collectionId}")]  // Accepte n'importe quoi
```

**Contraintes disponibles :**
- `:guid` - GUID au format standard
- `:int` - Entier 32 bits
- `:long` - Entier 64 bits
- `:bool` - Booléen
- `:datetime` - Date/heure
- `:decimal` - Décimal
- `:alpha` - Lettres uniquement
- `:min(value)` / `:max(value)` - Plages de valeurs
- `:minlength(value)` / `:maxlength(value)` - Longueur de chaîne

### Routes Uniques et Ordre

**TOUJOURS** donner un nom unique à chaque route et spécifier l'ordre :

```csharp
// ✅ BON : Noms uniques et ordre explicite
[HttpGet(Name = "collections-controller-get-collections-for-user", Order = 1)]
public async Task<ActionResult> GetCollections(...)

[HttpGet("{collectionId:guid}", Name = "collections-controller-get-collection-by-id", Order = 2)]
public async Task<ActionResult> GetCollection(...)

// ❌ MAUVAIS : Pas de noms/ordre
[HttpGet]
public async Task<ActionResult> GetCollections(...)

[HttpGet("{collectionId:guid}")]  // Peut créer des conflits
public async Task<ActionResult> GetCollection(...)
```

**Convention de nommage des routes :**
```
{controller-name}-{action-name}-{scope-context}
```

Exemples :
- `collections-controller-get-collections-for-user`
- `prompts-controller-create-prompt-for-current-user`
- `statistics-controller-get-usage-metrics-by-date-range`

---

## 📚 DOCUMENTATION XML

### Documentation Obligatoire

**TOUJOURS** documenter chaque paramètre avec sa source de binding :

```csharp
/// <summary>
/// Récupère une collection spécifique pour un utilisateur donné.
/// </summary>
/// <remarks>
/// Architecture flow: HttpGet → GetCollectionByIdQuery → CollectionQueryHandler → CollectionService → DbContext
///
/// Exemple d'utilisation :
/// GET /v1/users/{userId}/collections/{collectionId}
/// </remarks>
/// <param name="userId">Identifiant unique de l'utilisateur (route). Ne peut être vide.</param>
/// <param name="collectionId">Identifiant unique de la collection (route). Ne peut être vide.</param>
/// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
/// <returns>
/// - 200 OK : Collection trouvée avec ses détails
/// - 400 Bad Request : Identifiants invalides
/// - 404 Not Found : Collection non trouvée
/// - 401 Unauthorized : Non authentifié
/// - 403 Forbidden : Accès interdit
/// </returns>
[HttpGet("{collectionId:guid}", Name = "collections-controller-get-collection-by-id", Order = 2)]
public async Task<ActionResult<CollectionResponse>> GetCollection(
    [FromRoute] Guid userId,
    [FromRoute] Guid collectionId,
    CancellationToken cancellationToken = default)
```

**Points clés de la documentation :**
1. **Summary** : Description courte et claire
2. **Remarks** : Flow d'architecture + exemple d'URL
3. **Param** : Source binding explicite (route/body/query)
4. **Returns** : Liste complète des codes HTTP possibles

---

## ⚠️ GESTION DES ERREURS

### Validation des Paramètres Route

**TOUJOURS** valider les identifiants avant utilisation :

```csharp
// ✅ BON : Validation explicite
[HttpGet("{collectionId:guid}")]
public async Task<ActionResult<CollectionResponse>> GetCollection(
    [FromRoute] Guid userId,
    [FromRoute] Guid collectionId)
{
    if (userId == Guid.Empty)
    {
        return BadRequest("L'identifiant de l'utilisateur ne peut pas être vide");
    }
    
    if (collectionId == Guid.Empty)
    {
        return BadRequest("L'identifiant de la collection ne peut pas être vide");
    }
    
    // Logique métier...
}

// ❌ MAUVAIS : Pas de validation
[HttpGet("{collectionId:guid}")]
public async Task<ActionResult<CollectionResponse>> GetCollection(
    [FromRoute] Guid userId,
    [FromRoute] Guid collectionId)
{
    // Utilisation directe sans validation
    var result = await _service.GetCollection(userId, collectionId);
}
```

### Validation des Paramètres Body

**TOUJOURS** utiliser `[Required]` et Data Annotations :

```csharp
// ✅ BON : Validation via Data Annotations
public class CreateCollectionRequest
{
    [Required(ErrorMessage = "Le nom est obligatoire")]
    [MaxLength(100, ErrorMessage = "Le nom ne peut dépasser 100 caractères")]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Range(0, int.MaxValue, ErrorMessage = "L'ordre doit être positif")]
    public int DisplayOrder { get; set; }
}

[HttpPost]
public async Task<ActionResult<CollectionResponse>> CreateCollection(
    [FromRoute] Guid userId,
    [FromBody] CreateCollectionRequest request)
{
    // ModelState est automatiquement validé par [ApiController]
    // Pas besoin de if (!ModelState.IsValid)
}

// ❌ MAUVAIS : Pas de validation
public class CreateCollectionRequest
{
    public string Name { get; set; }  // Peut être null
    public string Description { get; set; }
}
```

---

## 📋 EXEMPLES COMPLETS

### Exemple 1 : GET avec Identifiants Route

```csharp
/// <summary>
/// Récupère une collection spécifique pour un utilisateur donné.
/// </summary>
/// <remarks>
/// Endpoint RESTful utilisant des identifiants dans la route pour une découvrabilité optimale.
///
/// Architecture flow: HttpGet → GetCollectionByIdQuery → CollectionQueryHandler → CollectionService → DbContext
///
/// Exemple d'utilisation :
/// GET /v1/users/123e4567-e89b-12d3-a456-426614174000/collections/987fcdeb-51a2-43f1-b3c4-123456789abc
/// </remarks>
/// <param name="userId">Identifiant unique de l'utilisateur (route). Ne peut être vide.</param>
/// <param name="collectionId">Identifiant unique de la collection (route). Ne peut être vide.</param>
/// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
/// <returns>
/// - 200 OK : Collection trouvée avec ses détails
/// - 400 Bad Request : Identifiants invalides (GUID vides)
/// - 404 Not Found : Collection non trouvée
/// - 401 Unauthorized : Non authentifié
/// - 403 Forbidden : Accès interdit
/// </returns>
[HttpGet("{collectionId:guid}", Name = "collections-controller-get-collection-by-id-for-user", Order = 2)]
[Tags("User Collections")]
[Consumes("application/json")]
[Produces("application/json")]
[ProducesResponseType(typeof(CollectionResponse), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<CollectionResponse>> GetCollection(
    [Required] [FromRoute] Guid userId,
    [Required] [FromRoute] Guid collectionId,
    CancellationToken cancellationToken = default)
{
    // Validation des identifiants
    if (userId == Guid.Empty)
    {
        return BadRequest("L'identifiant de l'utilisateur ne peut pas être vide");
    }
    
    if (collectionId == Guid.Empty)
    {
        return BadRequest("L'identifiant de la collection ne peut pas être vide");
    }
    
    // Exécution de la requête
    var query = new GetCollectionByIdQuery(userId) { Id = collectionId };
    var result = await _mediatorAdapter.Send(query, cancellationToken);
    
    // Gestion des résultats
    if (result == null || !result.Success)
    {
        return result == null ? NotFound() : BadRequest(result.ErrorMessage);
    }
    
    if (result.Collection == null)
    {
        return NotFound("Collection non trouvée");
    }
    
    return Ok(MapToResponse(result.Collection));
}
```

### Exemple 2 : GET avec Filtres Query

```csharp
/// <summary>
/// Récupère une liste paginée de collections pour un utilisateur avec filtres optionnels.
/// </summary>
/// <remarks>
/// Endpoint de liste avec support de recherche, pagination et tri.
///
/// Architecture flow: HttpGet → GetCollectionsQuery → CollectionQueryHandler → CollectionService → DbContext
///
/// Exemples d'utilisation :
/// - GET /v1/users/{userId}/collections
/// - GET /v1/users/{userId}/collections?searchTerm=ai&page=2&pageSize=10
/// - GET /v1/users/{userId}/collections?sortBy=name&sortOrder=desc
/// </remarks>
/// <param name="userId">Identifiant unique de l'utilisateur (route). Ne peut être vide.</param>
/// <param name="searchTerm">Terme de recherche optionnel (query). Filtre sur nom et description.</param>
/// <param name="page">Numéro de page (query). Valeur par défaut : 1.</param>
/// <param name="pageSize">Nombre d'éléments par page (query). Valeur par défaut : 20. Maximum : 100.</param>
/// <param name="sortBy">Champ de tri optionnel (query). Valeurs : "name", "createdAt", "updatedAt".</param>
/// <param name="sortOrder">Ordre de tri (query). Valeurs : "asc", "desc". Valeur par défaut : "asc".</param>
/// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
/// <returns>
/// - 200 OK : Liste de collections (peut être vide)
/// - 400 Bad Request : Paramètres de pagination invalides
/// - 401 Unauthorized : Non authentifié
/// - 403 Forbidden : Accès interdit
/// </returns>
[HttpGet(Name = "collections-controller-get-collections-for-user", Order = 1)]
[Tags("User Collections")]
[Consumes("application/json")]
[Produces("application/json")]
[ProducesResponseType(typeof(PaginatedResponse<CollectionResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public async Task<ActionResult<PaginatedResponse<CollectionResponse>>> GetCollections(
    [Required] [FromRoute] Guid userId,
    [FromQuery] string? searchTerm = null,
    [FromQuery] [Range(1, int.MaxValue)] int page = 1,
    [FromQuery] [Range(1, 100)] int pageSize = 20,
    [FromQuery] string? sortBy = null,
    [FromQuery] string sortOrder = "asc",
    CancellationToken cancellationToken = default)
{
    // Validation de l'identifiant utilisateur
    if (userId == Guid.Empty)
    {
        return BadRequest("L'identifiant de l'utilisateur ne peut pas être vide");
    }
    
    // Validation du tri
    if (sortOrder != "asc" && sortOrder != "desc")
    {
        return BadRequest("L'ordre de tri doit être 'asc' ou 'desc'");
    }
    
    // Construction de la requête
    var query = new GetCollectionsQuery
    {
        UserId = userId,
        SearchTerm = searchTerm,
        Page = page,
        PageSize = pageSize,
        SortBy = sortBy,
        SortOrder = sortOrder
    };
    
    // Exécution
    var result = await _mediatorAdapter.Send(query, cancellationToken);
    
    // Mapping des résultats
    var response = new PaginatedResponse<CollectionResponse>
    {
        Items = result.Collections.Select(MapToResponse).ToList(),
        TotalCount = result.TotalCount,
        Page = page,
        PageSize = pageSize
    };
    
    return Ok(response);
}
```

### Exemple 3 : POST avec Body

```csharp
/// <summary>
/// Crée une nouvelle collection pour un utilisateur.
/// </summary>
/// <remarks>
/// Endpoint de création utilisant un payload JSON structuré.
///
/// Architecture flow: HttpPost → CreateCollectionCommand → CollectionCommandHandler → CollectionService → DbContext
///
/// Exemple de requête :
/// POST /v1/users/{userId}/collections
/// {
///   "name": "Collections IA",
///   "description": "Prompts liés à l'intelligence artificielle",
///   "displayOrder": 1,
///   "isPublic": false
/// }
/// </remarks>
/// <param name="userId">Identifiant unique de l'utilisateur (route). Ne peut être vide.</param>
/// <param name="request">Données de création de la collection (body). Validées automatiquement.</param>
/// <param name="cancellationToken">Token d'annulation pour les opérations asynchrones.</param>
/// <returns>
/// - 201 Created : Collection créée avec succès. Header Location contient l'URL de la ressource.
/// - 400 Bad Request : Données invalides (validation échouée)
/// - 401 Unauthorized : Non authentifié
/// - 403 Forbidden : Accès interdit
/// - 409 Conflict : Collection avec ce nom existe déjà
/// </returns>
[HttpPost(Name = "collections-controller-create-collection-for-user", Order = 3)]
[Tags("User Collections")]
[Consumes("application/json")]
[Produces("application/json")]
[ProducesResponseType(typeof(CollectionResponse), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ProducesResponseType(StatusCodes.Status409Conflict)]
public async Task<ActionResult<CollectionResponse>> CreateCollection(
    [Required] [FromRoute] Guid userId,
    [Required] [FromBody] CreateCollectionRequest request,
    CancellationToken cancellationToken = default)
{
    // Validation de l'identifiant utilisateur
    if (userId == Guid.Empty)
    {
        return BadRequest("L'identifiant de l'utilisateur ne peut pas être vide");
    }
    
    // ModelState est automatiquement validé par [ApiController]
    // Les Data Annotations sur CreateCollectionRequest sont appliquées
    
    // Construction de la commande
    var command = new CreateCollectionCommand
    {
        UserId = userId,
        Name = request.Name,
        Description = request.Description,
        DisplayOrder = request.DisplayOrder,
        IsPublic = request.IsPublic
    };
    
    // Exécution
    var result = await _mediatorAdapter.Send(command, cancellationToken);
    
    // Gestion des conflits
    if (result.ErrorCode == "COLLECTION_ALREADY_EXISTS")
    {
        return Conflict("Une collection avec ce nom existe déjà");
    }
    
    // Succès : retour 201 Created avec Location header
    var response = MapToResponse(result.Collection);
    return CreatedAtRoute(
        "collections-controller-get-collection-by-id-for-user",
        new { userId, collectionId = result.Collection.Id },
        response);
}
```

### Modèle de Requête Body

```csharp
/// <summary>
/// Requête de création d'une collection.
/// </summary>
public class CreateCollectionRequest
{
    /// <summary>
    /// Nom de la collection (obligatoire, max 100 caractères).
    /// </summary>
    [Required(ErrorMessage = "Le nom est obligatoire")]
    [MaxLength(100, ErrorMessage = "Le nom ne peut dépasser 100 caractères")]
    [MinLength(1, ErrorMessage = "Le nom ne peut être vide")]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Description optionnelle de la collection (max 500 caractères).
    /// </summary>
    [MaxLength(500, ErrorMessage = "La description ne peut dépasser 500 caractères")]
    public string? Description { get; set; }
    
    /// <summary>
    /// Ordre d'affichage (doit être positif ou nul).
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "L'ordre d'affichage doit être positif ou nul")]
    public int DisplayOrder { get; set; }
    
    /// <summary>
    /// Indique si la collection est publique.
    /// </summary>
    public bool IsPublic { get; set; }
}
```

---

## ✅ CHECKLIST DE VALIDATION

Avant de commiter du code de contrôleur WebApi, vérifier :

**Routes :**
- [ ] Identifiants obligatoires dans la route (`[FromRoute]`)
- [ ] Contraintes de type sur les paramètres route (`:guid`, `:int`)
- [ ] Routes uniques avec `Name` explicite
- [ ] Ordre explicite avec `Order`
- [ ] Pas de conflit de routes (même verbe HTTP + même pattern)

**Paramètres :**
- [ ] `[FromRoute]` pour identifiants de ressources
- [ ] `[FromBody]` pour payloads complexes
- [ ] `[FromQuery]` pour filtres/pagination uniquement
- [ ] `[Required]` sur paramètres obligatoires
- [ ] Validation des GUID vides (`.Empty`)

**Documentation :**
- [ ] `<summary>` présent et clair
- [ ] `<remarks>` avec flow d'architecture
- [ ] `<remarks>` avec exemple d'URL
- [ ] `<param>` pour chaque paramètre (avec source binding)
- [ ] `<returns>` avec tous les codes HTTP possibles

**Attributs Swagger :**
- [ ] `[Tags]` pour regroupement logique
- [ ] `[ProducesResponseType]` pour chaque code HTTP
- [ ] `[Produces("application/json")]` si applicable
- [ ] `[Consumes("application/json")]` si applicable

**Validation :**
- [ ] Validation des identifiants route (pas de `Guid.Empty`)
- [ ] Data Annotations sur les modèles de requête
- [ ] Messages d'erreur en français
- [ ] Codes HTTP appropriés (200, 201, 400, 404, 409, etc.)

---

## 🚫 ANTI-PATTERNS À ÉVITER

### ❌ Anti-Pattern 1 : Identifiants en Query String

```csharp
// ❌ MAUVAIS
[HttpGet]
public async Task<ActionResult> GetCollection(
    [FromRoute] Guid userId,
    [FromQuery] Guid collectionId)  // Devrait être dans la route

// ✅ BON
[HttpGet("{collectionId:guid}")]
public async Task<ActionResult> GetCollection(
    [FromRoute] Guid userId,
    [FromRoute] Guid collectionId)
```

**Problèmes :**
- Conflit de routes avec méthode de liste
- URLs non RESTful
- Moins efficace pour le cache HTTP

### ❌ Anti-Pattern 2 : Paramètres Multiples en Query pour Création

```csharp
// ❌ MAUVAIS
[HttpPost]
public async Task<ActionResult> CreateCollection(
    [FromRoute] Guid userId,
    [FromQuery] string name,
    [FromQuery] string? description,
    [FromQuery] bool isPublic)

// ✅ BON
[HttpPost]
public async Task<ActionResult> CreateCollection(
    [FromRoute] Guid userId,
    [FromBody] CreateCollectionRequest request)
```

**Problèmes :**
- Pas de validation structurée
- URLs longues et illisibles
- Difficile à maintenir

### ❌ Anti-Pattern 3 : Routes Sans Contraintes

```csharp
// ❌ MAUVAIS
[HttpGet("{userId}/collections/{collectionId}")]
public async Task<ActionResult> GetCollection(
    [FromRoute] Guid userId,
    [FromRoute] Guid collectionId)

// ✅ BON
[HttpGet("{userId:guid}/collections/{collectionId:guid}")]
public async Task<ActionResult> GetCollection(
    [FromRoute] Guid userId,
    [FromRoute] Guid collectionId)
```

**Problèmes :**
- Accepte n'importe quelle chaîne
- Erreurs 500 au lieu de 400 si format invalide
- Moins de clarté dans Swagger

### ❌ Anti-Pattern 4 : Pas de Validation des GUID Vides

```csharp
// ❌ MAUVAIS
[HttpGet("{collectionId:guid}")]
public async Task<ActionResult> GetCollection(
    [FromRoute] Guid userId,
    [FromRoute] Guid collectionId)
{
    // Utilisation directe sans validation
    var result = await _service.GetCollection(userId, collectionId);
}

// ✅ BON
[HttpGet("{collectionId:guid}")]
public async Task<ActionResult> GetCollection(
    [FromRoute] Guid userId,
    [FromRoute] Guid collectionId)
{
    if (userId == Guid.Empty)
    {
        return BadRequest("L'identifiant de l'utilisateur ne peut pas être vide");
    }
    
    if (collectionId == Guid.Empty)
    {
        return BadRequest("L'identifiant de la collection ne peut pas être vide");
    }
    
    var result = await _service.GetCollection(userId, collectionId);
}
```

**Problèmes :**
- `Guid.Empty` (`00000000-0000-0000-0000-000000000000`) passe la contrainte `:guid`
- Erreurs métier au lieu d'erreurs de validation

---

## 📚 RÉFÉRENCES

- [Microsoft Docs - Routing in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing)
- [Microsoft Docs - Model Binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding)
- [RESTful API Guidelines](https://restfulapi.net/)
- [OpenAPI Specification](https://swagger.io/specification/)

---

## 🔄 HISTORIQUE DES MODIFICATIONS

| Date | Version | Auteur | Changements |
|------|---------|--------|-------------|
| 2025-12-12 | 1.0.0 | Copilot | Création initiale - Standards WebApi avec priorité FromRoute → FromBody → FromQuery |

---

**FIN DES INSTRUCTIONS WEBAPI STANDARDS**
