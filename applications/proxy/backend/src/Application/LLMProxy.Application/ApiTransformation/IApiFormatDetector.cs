using LLMProxy.Domain.LLM;
using Microsoft.AspNetCore.Http;

namespace LLMProxy.Application.ApiTransformation;

/// <summary>
/// Interface pour la détection automatique du format API.
/// </summary>
public interface IApiFormatDetector
{
    /// <summary>
    /// Détecte le format API à partir d'une requête HTTP.
    /// </summary>
    /// <param name="request">La requête HTTP entrante.</param>
    /// <returns>Le format API détecté.</returns>
    ApiFormat DetectFormat(HttpRequest request);

    /// <summary>
    /// Détecte le format API à partir du chemin de requête.
    /// </summary>
    /// <param name="path">Le chemin de la requête.</param>
    /// <returns>Le format API correspondant au chemin.</returns>
    ApiFormat DetectFromPath(string path);

    /// <summary>
    /// Tente de détecter le format à partir du header X-Api-Format.
    /// </summary>
    /// <param name="request">La requête HTTP entrante.</param>
    /// <param name="format">Le format détecté si présent.</param>
    /// <returns>True si un header de format valide a été trouvé.</returns>
    bool TryDetectFromHeader(HttpRequest request, out ApiFormat format);
}
