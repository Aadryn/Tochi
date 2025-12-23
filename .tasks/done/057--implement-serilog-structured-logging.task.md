# Tâche 057 - Implémenter Serilog pour Logging Structuré

## PRIORITÉ
🔴 **P1 - CRITIQUE** (Priorité 2/8 de la refonte)

## OBJECTIF

Remplacer le logging par défaut ASP.NET Core par Serilog avec logs structurés JSON, enrichissement contextuel, et multiples sinks (Console, File, Seq, OpenTelemetry).

## CONTEXTE

### État Actuel
- Logging ASP.NET Core par défaut (`ILogger<T>`)
- Pas de logs structurés JSON
- Pas de sinks vers systèmes d'agrégation

### État Cible
- Serilog comme provider de logging
- Logs JSON structurés (CompactJsonFormatter)
- Enrichissement automatique (TenantId, RequestId, UserId, TraceId)
- Sinks : Console (dev), File (rolling), Seq/Elasticsearch (prod)
- Intégration OpenTelemetry pour logs distribués

## IMPLÉMENTATION

### Phase 1 : Packages NuGet
```xml
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Serilog.Formatting.Compact" Version="2.0.0" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="2.3.0" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="3.1.0" />
<PackageReference Include="Serilog.Expressions" Version="4.0.0" />
```

### Phase 2 : Configuration Serilog
```
src/Presentation/LLMProxy.Gateway/
├── Extensions/
│   └── SerilogExtensions.cs
├── Enrichers/
│   ├── TenantIdEnricher.cs
│   ├── ApiKeyEnricher.cs
│   └── LLMContextEnricher.cs
```

### Phase 3 : appsettings.json
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.Console", "Serilog.Sinks.File"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Yarp": "Information",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console", "Args": { "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact" } },
      { "Name": "File", "Args": { 
        "path": "logs/llmproxy-.json",
        "rollingInterval": "Day",
        "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
      }}
    ],
    "Enrich": ["FromLogContext", "WithMachineName", "WithThreadId"]
  }
}
```

### Phase 4 : Program.cs Integration
```csharp
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "LLMProxy.Gateway"));
```

## CRITÈRES DE SUCCÈS

- [ ] Serilog configuré comme provider de logging
- [ ] Logs JSON structurés en console et fichier
- [ ] Enrichissement TenantId, RequestId, TraceId automatique
- [ ] Rolling file logs (1 fichier/jour)
- [ ] Middleware de logging request/response avec Serilog
- [ ] Suppression des anciens using Microsoft.Extensions.Logging où nécessaire
- [ ] Tests unitaires pour enrichers
- [ ] Build : 0 erreurs, 0 warnings
- [ ] Documentation XML complète (français)

## DÉPENDANCES

- Aucune (standalone)

## ESTIMATION

**Effort** : 4h
**Complexité** : Faible-Moyenne

## RÉFÉRENCES

- [Serilog ASP.NET Core](https://github.com/serilog/serilog-aspnetcore)
- ADR-031 (Structured Logging)
- ADR-054 (Request/Response Logging)


## TRACKING
Début: 2025-12-22T17:11:35.0407448Z


Fin: 2025-12-22T17:20:41.1058444Z

## STATUT:  COMPLÉTÉ

### Fichiers créés
- `src/Presentation/LLMProxy.Gateway/Extensions/SerilogExtensions.cs`
- `src/Presentation/LLMProxy.Gateway/Logging/TenantIdEnricher.cs`
- `src/Presentation/LLMProxy.Gateway/Logging/ApiKeyEnricher.cs`
- `src/Presentation/LLMProxy.Gateway/Logging/LLMContextEnricher.cs`

### Fichiers modifiés
- `src/Presentation/LLMProxy.Gateway/LLMProxy.Gateway.csproj` (packages Serilog)
- `src/Presentation/LLMProxy.Gateway/Program.cs` (intégration Serilog)
- `src/Presentation/LLMProxy.Gateway/appsettings.json` (configuration Serilog)

### Tests
- Build :  0 erreurs
- Gateway.Tests :  30 réussis
