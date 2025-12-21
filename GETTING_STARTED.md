# Guide de Démarrage Rapide - LLM Proxy

## 🎯 Objectif

Ce guide vous permet de démarrer rapidement avec le projet LLM Proxy.

## 📋 Prérequis

- .NET 9 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- Docker Desktop ([Download](https://www.docker.com/products/docker-desktop))
- Git
- Visual Studio 2022 / VS Code / Rider (optionnel)

## 🚀 Démarrage Rapide (5 minutes)

### 1. Cloner le projet

```bash
git clone <repo-url>
cd LLMProxy
```

### 2. Démarrer les dépendances avec Docker

```bash
docker-compose up -d postgres redis
```

Attendez que les services soient prêts (environ 30 secondes).

### 3. Exécuter les migrations de base de données

```bash
cd src/Infrastructure/LLMProxy.Infrastructure.PostgreSQL
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Lancer le Gateway

```bash
cd ../../Presentation/LLMProxy.Gateway
dotnet run
```

Le Gateway sera accessible sur `http://localhost:5000`

### 5. (Optionnel) Lancer l'Admin API

Dans un nouveau terminal:

```bash
cd src/Presentation/LLMProxy.Admin.API
dotnet run
```

L'Admin API sera accessible sur `http://localhost:5001`

## 🧪 Tester le Proxy

### Exemple de requête simple

```bash
curl -X POST http://localhost:5000/openai/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key-here" \
  -d '{
    "model": "gpt-3.5-turbo",
    "messages": [{"role": "user", "content": "Hello!"}]
  }'
```

### Exemple de streaming

```bash
curl -X POST http://localhost:5000/openai/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key-here" \
  -d '{
    "model": "gpt-3.5-turbo",
    "messages": [{"role": "user", "content": "Tell me a story"}],
    "stream": true
  }'
```

## 🛠️ Développement

### Restaurer les packages

```bash
dotnet restore
```

### Compiler le projet

```bash
dotnet build
```

### Exécuter les tests

```bash
dotnet test
```

### Exécuter les tests avec couverture

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 🐳 Utilisation avec Docker Compose

Pour démarrer l'ensemble du stack (Gateway + Admin API + Dépendances + Observabilité):

```bash
docker-compose up --build
```

Services disponibles:
- Gateway: http://localhost:8080
- Admin API: http://localhost:8081
- PostgreSQL: localhost:5432
- Redis: localhost:6379
- Jaeger UI: http://localhost:16686
- Prometheus: http://localhost:9090
- Grafana: http://localhost:3000 (admin/admin)

## 📊 Observabilité

### Jaeger (Tracing)
Accédez à http://localhost:16686 pour voir les traces distribuées.

### Prometheus (Métriques)
Accédez à http://localhost:9090 pour requêter les métriques.

### Grafana (Dashboards)
Accédez à http://localhost:3000 (admin/admin) pour visualiser les dashboards.

## 🔧 Configuration

### Configurer un nouveau provider LLM

Éditez `src/Presentation/LLMProxy.Gateway/appsettings.json`:

```json
{
  "ReverseProxy": {
    "Routes": {
      "my-provider-route": {
        "ClusterId": "my-provider-cluster",
        "Match": {
          "Path": "/my-provider/{**catch-all}"
        }
      }
    },
    "Clusters": {
      "my-provider-cluster": {
        "Destinations": {
          "destination1": {
            "Address": "https://api.myprovider.com"
          }
        }
      }
    }
  }
}
```

### Variables d'environnement

Créez un fichier `.env` à la racine:

```env
ConnectionStrings__PostgreSQL=Host=localhost;Port=5432;Database=llmproxy;Username=postgres;Password=postgres
ConnectionStrings__Redis=localhost:6379
OPENAI_API_KEY=your-openai-key
ANTHROPIC_API_KEY=your-anthropic-key
```

## 📝 Prochaines Étapes

1. **Implémenter les repositories manquants** (UserRepository, ApiKeyRepository, etc.)
2. **Créer l'Admin API** avec endpoints CRUD pour gérer les tenants, users, providers
3. **Implémenter le service de quotas** avec Redis
4. **Ajouter le support de multiple providers** (Anthropic, Mistral, etc.)
5. **Configurer l'authentification OAuth2/JWT**
6. **Ajouter des tests d'intégration**
7. **Créer le frontend React** pour l'administration

## 🆘 Dépannage

### Erreur de connexion PostgreSQL

Vérifiez que PostgreSQL est démarré:
```bash
docker ps | grep postgres
```

Si non, démarrez-le:
```bash
docker-compose up -d postgres
```

### Erreur de connexion Redis

Vérifiez que Redis est démarré:
```bash
docker ps | grep redis
```

### Port déjà utilisé

Changez le port dans `appsettings.json` ou utilisez:
```bash
dotnet run --urls "http://localhost:5555"
```

## 📚 Documentation

- [Architecture](docs/architecture/README.md)
- [API Documentation](docs/api/README.md)
- [Configuration avancée](docs/configuration/README.md)
- [Déploiement](docs/deployment/README.md)

## 💡 Conseils

- Utilisez **TDD** : écrivez les tests en premier (Red-Green-Refactor)
- Respectez **SOLID** : chaque classe a une seule responsabilité
- Appliquez **YAGNI** : n'implémentez que ce qui est nécessaire maintenant
- Gardez **KISS** : la simplicité est la clé
- Évitez **DRY** : ne vous répétez pas

## 📞 Support

Pour toute question ou problème, ouvrez une issue sur GitHub.
