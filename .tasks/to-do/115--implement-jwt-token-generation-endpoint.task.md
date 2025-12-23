# 115. Implement JWT Token Generation Endpoint

**Statut:** À faire  
**Priorité:** HIGH (Week 1)  
**Catégorie:** Authentication & Authorization  
**Dépendances:** Aucune

## OBJECTIF

Créer un endpoint `/api/auth/token` dans l'Admin API pour générer des tokens JWT d'authentification utilisateur, permettant :
- Authentification utilisateur par email + mot de passe
- Génération de JWT avec claims (userId, tenantId, roles)
- Durée de vie configurable des tokens
- Refresh token pour renouvellement

## CONTEXTE

Actuellement, l'application utilise uniquement l'authentification par API Key. Pour l'Admin UI et les utilisateurs finaux, une authentification JWT est nécessaire pour :
- Accéder à l'Admin API de manière sécurisée
- Maintenir une session utilisateur dans l'UI
- Autoriser les actions en fonction des rôles (Admin, User, etc.)

## CRITÈRES DE SUCCÈS

- [ ] Endpoint `POST /api/auth/token` créé dans Admin API
- [ ] Validation des credentials (email + password) contre base de données
- [ ] Génération JWT avec claims : userId, tenantId, email, roles, exp, iat
- [ ] Configuration JWT dans `appsettings.json` (secret, issuer, audience, expiration)
- [ ] Support refresh token (optionnel mais recommandé)
- [ ] Hashage sécurisé des mots de passe (BCrypt ou PBKDF2)
- [ ] Tests unitaires validant génération et validation de tokens
- [ ] Documentation Swagger/OpenAPI complète
- [ ] Build : 0 erreurs, 0 warnings

## FICHIERS CONCERNÉS

- `src/Presentation/LLMProxy.Admin.API/Controllers/AuthController.cs` (nouveau)
- `src/Application/LLMProxy.Application/Auth/Commands/GenerateTokenCommand.cs` (nouveau)
- `src/Application/LLMProxy.Application/Auth/Commands/GenerateTokenCommandHandler.cs` (nouveau)
- `src/Infrastructure/LLMProxy.Infrastructure.Security/Services/JwtTokenGenerator.cs` (nouveau)
- `src/Infrastructure/LLMProxy.Infrastructure.Security/Services/IPasswordHasher.cs` (nouveau)
- `src/Infrastructure/LLMProxy.Infrastructure.Security/Services/PasswordHasher.cs` (nouveau)
- `tests/LLMProxy.Application.Tests/Auth/GenerateTokenCommandHandlerTests.cs` (nouveau)
- `src/Presentation/LLMProxy.Admin.API/appsettings.json` (modifier)

## APPROCHE TECHNIQUE

### 1. Créer AuthController

```csharp
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LLMProxy.Admin.API.Controllers;

/// <summary>
/// Contrôleur d'authentification pour génération de tokens JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Initialise une nouvelle instance du contrôleur.
    /// </summary>
    /// <param name="mediator">Médiateur MediatR pour dispatch des commandes.</param>
    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Génère un token JWT d'authentification.
    /// </summary>
    /// <param name="request">Requête contenant email et mot de passe.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Token JWT et refresh token.</returns>
    /// <response code="200">Token généré avec succès.</response>
    /// <response code="401">Credentials invalides.</response>
    [HttpPost("token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GenerateTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateToken(
        [FromBody] GenerateTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new GenerateTokenCommand(request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            onSuccess: token => Ok(new GenerateTokenResponse
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresIn = token.ExpiresIn,
                TokenType = "Bearer"
            }),
            onFailure: error => Unauthorized(new { error.Code, error.Message })
        );
    }

    /// <summary>
    /// Renouvelle un token JWT expiré via refresh token.
    /// </summary>
    /// <param name="request">Requête contenant le refresh token.</param>
    /// <param name="cancellationToken">Token d'annulation.</param>
    /// <returns>Nouveau token JWT.</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(GenerateTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            onSuccess: token => Ok(new GenerateTokenResponse
            {
                AccessToken = token.AccessToken,
                RefreshToken = token.RefreshToken,
                ExpiresIn = token.ExpiresIn,
                TokenType = "Bearer"
            }),
            onFailure: error => Unauthorized(new { error.Code, error.Message })
        );
    }
}

/// <summary>
/// Requête de génération de token JWT.
/// </summary>
public record GenerateTokenRequest(string Email, string Password);

/// <summary>
/// Requête de renouvellement de token JWT.
/// </summary>
public record RefreshTokenRequest(string RefreshToken);

/// <summary>
/// Réponse contenant le token JWT généré.
/// </summary>
public record GenerateTokenResponse
{
    /// <summary>Access token JWT.</summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>Refresh token pour renouvellement.</summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>Durée de validité en secondes.</summary>
    public int ExpiresIn { get; init; }

    /// <summary>Type de token (toujours "Bearer").</summary>
    public string TokenType { get; init; } = "Bearer";
}
```

### 2. Créer GenerateTokenCommand

```csharp
using LLMProxy.Domain.Common;
using MediatR;

namespace LLMProxy.Application.Auth.Commands;

/// <summary>
/// Commande de génération de token JWT d'authentification.
/// </summary>
/// <param name="Email">Email de l'utilisateur.</param>
/// <param name="Password">Mot de passe en clair.</param>
public record GenerateTokenCommand(string Email, string Password) : IRequest<Result<TokenDto>>;

/// <summary>
/// DTO contenant les tokens générés.
/// </summary>
public record TokenDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public int ExpiresIn { get; init; }
}
```

### 3. Créer GenerateTokenCommandHandler

```csharp
using LLMProxy.Application.Common.Interfaces;
using LLMProxy.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Application.Auth.Commands;

/// <summary>
/// Handler pour la commande de génération de token JWT.
/// </summary>
public sealed class GenerateTokenCommandHandler : IRequestHandler<GenerateTokenCommand, Result<TokenDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<GenerateTokenCommandHandler> _logger;

    /// <summary>
    /// Initialise une nouvelle instance du handler.
    /// </summary>
    public GenerateTokenCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<GenerateTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Result<TokenDto>> Handle(GenerateTokenCommand request, CancellationToken cancellationToken)
    {
        // 1. Récupérer utilisateur par email
        var userResult = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (userResult.IsFailure)
        {
            _logger.LogWarning("Tentative de connexion échouée pour {Email}", request.Email);
            return Error.User.InvalidCredentials;
        }

        var user = userResult.Value;

        // 2. Vérifier mot de passe
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            _logger.LogWarning("Mot de passe invalide pour {Email}", request.Email);
            return Error.User.InvalidCredentials;
        }

        // 3. Vérifier que l'utilisateur est actif
        if (!user.IsActive)
        {
            _logger.LogWarning("Utilisateur inactif {UserId}", user.Id);
            return Error.User.Inactive(user.Id);
        }

        // 4. Générer tokens JWT
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user);

        _logger.LogInformation("Token généré pour {UserId} ({Email})", user.Id, user.Email);

        return new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 3600 // 1 heure
        };
    }
}
```

### 4. Créer IJwtTokenGenerator

```csharp
using LLMProxy.Domain.Entities;

namespace LLMProxy.Application.Common.Interfaces;

/// <summary>
/// Interface pour la génération de tokens JWT.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Génère un access token JWT pour un utilisateur.
    /// </summary>
    /// <param name="user">Utilisateur pour lequel générer le token.</param>
    /// <returns>Token JWT encodé.</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Génère un refresh token pour renouvellement.
    /// </summary>
    /// <param name="user">Utilisateur pour lequel générer le refresh token.</param>
    /// <returns>Refresh token encodé.</returns>
    string GenerateRefreshToken(User user);

    /// <summary>
    /// Valide un refresh token et retourne le userId.
    /// </summary>
    /// <param name="refreshToken">Refresh token à valider.</param>
    /// <returns>UserId si valide, null sinon.</returns>
    Guid? ValidateRefreshToken(string refreshToken);
}
```

### 5. Implémenter JwtTokenGenerator

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LLMProxy.Application.Common.Interfaces;
using LLMProxy.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LLMProxy.Infrastructure.Security.Services;

/// <summary>
/// Générateur de tokens JWT pour authentification.
/// </summary>
public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    /// <inheritdoc/>
    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("tenantId", user.TenantId.ToString()),
            new Claim("name", user.Name),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }

    /// <inheritdoc/>
    public string GenerateRefreshToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("tokenType", "refresh"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.RefreshTokenSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7), // Refresh token valide 7 jours
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }

    /// <inheritdoc/>
    public Guid? ValidateRefreshToken(string refreshToken)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.RefreshTokenSecret));

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _options.Issuer,
                ValidAudience = _options.Audience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            var principal = _tokenHandler.ValidateToken(refreshToken, validationParameters, out _);
            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub);

            return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId)
                ? userId
                : null;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Options de configuration JWT.
/// </summary>
public class JwtOptions
{
    public string Secret { get; set; } = string.Empty;
    public string RefreshTokenSecret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}
```

### 6. Configuration appsettings.json

```json
{
  "Jwt": {
    "Secret": "VotreCléSecrèteTrèsLongueEtComplexe_MinimumLength256Bits!",
    "RefreshTokenSecret": "VotreCléSecrèteRefreshTokenTrèsLongueEtComplexe_MinimumLength256Bits!",
    "Issuer": "LLMProxy.AdminAPI",
    "Audience": "LLMProxy.Clients",
    "ExpirationMinutes": 60
  }
}
```

### 7. Configurer DependencyInjection

```csharp
// Dans Program.cs Admin API

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();

// Configurer authentification JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration.GetSection("Jwt").Get<JwtOptions>();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
```

## DÉFINITION DE TERMINÉ

- [ ] Endpoint `/api/auth/token` créé et testé
- [ ] Validation credentials + génération JWT fonctionnelle
- [ ] Refresh token implémenté
- [ ] Configuration JWT dans appsettings.json
- [ ] Tests unitaires pour GenerateTokenCommandHandler
- [ ] Tests d'intégration pour AuthController
- [ ] Documentation Swagger complète avec exemples
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Tests : 100% passing

## RÉFÉRENCES

- **Source:** `docs/NEXT_STEPS.md` (High Priority - Authentication & Authorization)
- **JWT Best Practices:** https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn
- **ADR-005:** SOLID Principles (Dependency Inversion)
