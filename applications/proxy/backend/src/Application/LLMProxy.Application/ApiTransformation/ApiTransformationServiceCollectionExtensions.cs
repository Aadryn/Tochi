using LLMProxy.Application.ApiTransformation.Interfaces;
using LLMProxy.Application.ApiTransformation.Ollama;
using LLMProxy.Application.ApiTransformation.OpenAI;
using Microsoft.Extensions.DependencyInjection;

namespace LLMProxy.Application.ApiTransformation;

/// <summary>
/// Extensions pour l'enregistrement des services de transformation API dans le conteneur DI.
/// </summary>
public static class ApiTransformationServiceCollectionExtensions
{
    /// <summary>
    /// Ajoute les services de transformation API au conteneur de services.
    /// </summary>
    /// <param name="services">La collection de services.</param>
    /// <returns>La collection de services pour chaînage.</returns>
    public static IServiceCollection AddApiTransformation(this IServiceCollection services)
    {
        // Enregistrer les transformers individuels
        services.AddSingleton<IRequestTransformer, OpenAIRequestTransformer>();
        services.AddSingleton<IRequestTransformer, OllamaRequestTransformer>();
        services.AddSingleton<IResponseTransformer, OpenAIResponseTransformer>();
        services.AddSingleton<IResponseTransformer, OllamaResponseTransformer>();

        // Enregistrer la factory avec injection des transformers
        services.AddSingleton<ITransformerFactory>(sp =>
        {
            var requestTransformers = sp.GetServices<IRequestTransformer>();
            var responseTransformers = sp.GetServices<IResponseTransformer>();
            return new TransformerFactory(requestTransformers, responseTransformers);
        });

        // Enregistrer le détecteur de format
        services.AddSingleton<IApiFormatDetector, ApiFormatDetector>();

        return services;
    }
}
