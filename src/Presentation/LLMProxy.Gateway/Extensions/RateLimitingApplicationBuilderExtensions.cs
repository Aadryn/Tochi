using LLMProxy.Presentation.Middleware;
using Microsoft.AspNetCore.Builder;

namespace LLMProxy.Presentation.Extensions;

/// <summary>
/// Extensions pour enregistrer le middleware de rate limiting dans le pipeline HTTP.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-041 Rate Limiting et Throttling.
/// </para>
/// <para>
/// Cette classe fournit les méthodes d'extension pour activer le middleware
/// de rate limiting dans le pipeline de requêtes ASP.NET Core.
/// </para>
/// </remarks>
public static class RateLimitingApplicationBuilderExtensions
{
    /// <summary>
    /// Active le middleware de rate limiting dans le pipeline HTTP.
    /// </summary>
    /// <param name="app">Builder d'application ASP.NET Core.</param>
    /// <returns>Builder pour chaînage fluent.</returns>
    /// <remarks>
    /// <para>
    /// <strong>IMPORTANT - Ordre du middleware :</strong>
    /// </para>
    /// <para>
    /// Ce middleware DOIT être placé :
    /// <list type="number">
    /// <item><description>APRÈS UseRouting() : pour avoir accès au endpoint</description></item>
    /// <item><description>AVANT UseAuthentication() : protection DDoS avant authentification</description></item>
    /// <item><description>AVANT UseAuthorization() : vérifier limites avant accès aux ressources</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Exemple d'ordre correct dans Program.cs :</strong>
    /// </para>
    /// <code>
    /// var app = builder.Build();
    /// 
    /// app.UseRouting();            // 1. Routing en premier
    /// app.UseRateLimiting();       // 2. Rate limiting avant auth
    /// app.UseAuthentication();     // 3. Authentification
    /// app.UseAuthorization();      // 4. Autorisation
    /// 
    /// app.MapControllers();
    /// app.Run();
    /// </code>
    /// </para>
    /// <para>
    /// <strong>Pourquoi cet ordre ?</strong>
    /// <list type="bullet">
    /// <item><description>Routing : Nécessaire pour extraire l'endpoint cible</description></item>
    /// <item><description>RateLimiting avant Auth : Protège contre les tentatives de brute-force sur /login</description></item>
    /// <item><description>RateLimiting avant Authz : Évite les appels coûteux si quota dépassé</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app)
    {
        if (app == null)
            throw new ArgumentNullException(nameof(app));

        return app.UseMiddleware<RateLimitingMiddleware>();
    }
}
