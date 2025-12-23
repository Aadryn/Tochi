using LLMProxy.Application.ApiTransformation.Interfaces;
using LLMProxy.Domain.LLM;

namespace LLMProxy.Application.ApiTransformation;

/// <summary>
/// Interface de la factory pour obtenir les transformers appropriés.
/// </summary>
public interface ITransformerFactory
{
    /// <summary>
    /// Obtient le transformer de requêtes pour un format donné.
    /// </summary>
    /// <param name="format">Le format API souhaité.</param>
    /// <returns>Le transformer de requêtes correspondant.</returns>
    /// <exception cref="NotSupportedException">Si le format n'est pas supporté.</exception>
    IRequestTransformer GetRequestTransformer(ApiFormat format);

    /// <summary>
    /// Obtient le transformer de réponses pour un format donné.
    /// </summary>
    /// <param name="format">Le format API souhaité.</param>
    /// <returns>Le transformer de réponses correspondant.</returns>
    /// <exception cref="NotSupportedException">Si le format n'est pas supporté.</exception>
    IResponseTransformer GetResponseTransformer(ApiFormat format);

    /// <summary>
    /// Vérifie si un format est supporté.
    /// </summary>
    /// <param name="format">Le format à vérifier.</param>
    /// <returns>True si le format est supporté, false sinon.</returns>
    bool IsFormatSupported(ApiFormat format);

    /// <summary>
    /// Obtient la liste des formats supportés.
    /// </summary>
    /// <returns>La liste des formats API supportés.</returns>
    IEnumerable<ApiFormat> GetSupportedFormats();
}
