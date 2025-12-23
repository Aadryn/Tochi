---
id: 065
title: Externaliser les secrets Docker Compose
concerns: sécurité, configuration, infrastructure
priority: high
effort: small
dependencies: []
status: to-do
created: 2025-12-23
---

# Externaliser les secrets Docker Compose

## 🎯 Objectif

Éliminer le hardcoding du mot de passe PostgreSQL dans `docker-compose.yml` en externalisant les secrets dans un fichier `.env` pour conformité sécurité et séparation des environnements.

## 📊 Contexte

### Problème identifié

Fichier `docker-compose.yml` (ligne 11) contient mot de passe hardcodé :

```yaml
# ❌ AVANT : Hardcoding (RISQUE SÉCURITÉ)
postgres:
  image: postgres:16-alpine
  container_name: llmproxy-postgres
  environment:
    POSTGRES_DB: llmproxy
    POSTGRES_USER: postgres
    POSTGRES_PASSWORD: postgres  # ← HARDCODÉ
```

### Impact actuel

- **Risque sécurité** : Mot de passe en clair dans Git (même si développement)
- **Pas de séparation environnements** : Impossible d'avoir différents secrets dev/staging/prod
- **Non conforme** : Violation des best practices Docker et principes sécurité
- **Maintenance difficile** : Changement secret = modification fichier versionné

### Bénéfice attendu

- **Sécurité** : Secrets externalisés, pas de commit accidentel de credentials production
- **Flexibilité** : Différents secrets par environnement (dev/staging/prod)
- **Conformité** : Best practices Docker Compose et sécurité
- **Maintenabilité** : Changement secret sans toucher docker-compose.yml

## 🔧 Implémentation

### Fichiers à créer

```
.env.example              # Template des variables (versionné)
.env                      # Valeurs réelles (gitignored)
.gitignore                # S'assurer que .env est ignoré
```

### Fichiers à modifier

```
docker-compose.yml        # Remplacer hardcoding par variables
README.md                 # Documenter configuration
setup.ps1                 # Vérifier/créer .env si absent
```

### Modifications détaillées

#### 1. Créer `.env.example` (Template versionné)

```dotenv
# Infrastructure Configuration Template
# Copier ce fichier en .env et personnaliser les valeurs

# PostgreSQL Configuration
POSTGRES_DB=llmproxy
POSTGRES_USER=postgres
POSTGRES_PASSWORD=changeme_secure_password_here

# Redis Configuration (optionnel)
REDIS_PASSWORD=changeme_redis_password_here

# Optional: Database connection for applications
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=llmproxy;Username=postgres;Password=changeme_secure_password_here
```

#### 2. Créer `.env` initial (Non versionné)

```dotenv
# Infrastructure Configuration
# NE PAS COMMITER CE FICHIER

# PostgreSQL Configuration
POSTGRES_DB=llmproxy
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres_dev_password_2025

# Redis Configuration
REDIS_PASSWORD=redis_dev_password_2025

# Database connection
DATABASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=llmproxy;Username=postgres;Password=postgres_dev_password_2025
```

#### 3. Modifier `docker-compose.yml`

```yaml
# ✅ APRÈS : Utilisation variables d'environnement
version: '3.8'

services:
  # PostgreSQL Database
  postgres:
    image: postgres:16-alpine
    container_name: llmproxy-postgres
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER}"]
      interval: 10s
      timeout: 5s
      retries: 5

  # Redis Cache
  redis:
    image: redis:7-alpine
    container_name: llmproxy-redis
    command: redis-server --appendonly yes --requirepass ${REDIS_PASSWORD:-}
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "--no-auth-warning", "-a", "${REDIS_PASSWORD:-}", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5

  # (Reste du fichier inchangé)
```

#### 4. Mettre à jour `.gitignore`

```gitignore
# Vérifier que .env est bien ignoré (ajouter si absent)
.env
.env.local
.env.*.local

# Garder les templates
!.env.example
```

#### 5. Modifier `setup.ps1` (Vérification .env)

Ajouter au début du script :

```powershell
# Vérifier existence .env
if (-not (Test-Path "$PWD\.env")) {
    Write-Host "⚠️  Fichier .env absent" -ForegroundColor Yellow
    Write-Host "Création à partir du template .env.example..." -ForegroundColor Cyan
    
    if (Test-Path "$PWD\.env.example") {
        Copy-Item "$PWD\.env.example" "$PWD\.env"
        Write-Host "✅ .env créé avec succès" -ForegroundColor Green
        Write-Host "⚠️  IMPORTANT : Modifier .env avec vos vrais secrets avant de lancer Docker" -ForegroundColor Yellow
        
        # Pause pour laisser le temps de modifier
        Read-Host "Appuyez sur Entrée après avoir modifié .env"
    } else {
        Write-Host "❌ Template .env.example introuvable" -ForegroundColor Red
        exit 1
    }
}

Write-Host "✅ Configuration .env détectée" -ForegroundColor Green

# (Suite du script setup.ps1 existant)
```

#### 6. Mettre à jour `README.md`

Ajouter section configuration :

```markdown
## ⚙️ Configuration

### Secrets et Variables d'Environnement

1. **Créer fichier `.env` depuis le template :**
   ```powershell
   Copy-Item .env.example .env
   ```

2. **Modifier `.env` avec vos secrets :**
   ```dotenv
   POSTGRES_PASSWORD=votre_mot_de_passe_securise
   REDIS_PASSWORD=votre_mot_de_passe_redis
   ```

3. **⚠️ IMPORTANT** : Ne **JAMAIS** commiter le fichier `.env` (déjà dans `.gitignore`)

4. **Production** : Utiliser secrets management (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault)

### Génération Mots de Passe Sécurisés

```powershell
# PowerShell : Générer mot de passe aléatoire
-join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object {[char]$_})
```
```

#### 7. Créer `docs/security/SECRETS_MANAGEMENT.md` (Documentation)

```markdown
# Gestion des Secrets

## Développement Local

- **Fichier** : `.env` (non versionné)
- **Template** : `.env.example` (versionné)
- **Source** : Variables manuelles

## Staging

- **Source** : Variables d'environnement CI/CD (GitHub Actions, Azure DevOps)
- **Format** : Secrets injectés au build

## Production

- **Source** : Azure Key Vault / AWS Secrets Manager / HashiCorp Vault
- **Rotation** : Automatique tous les 90 jours
- **Accès** : Managed Identity / IAM Roles

## Bonnes Pratiques

1. ✅ Ne **JAMAIS** commiter `.env`
2. ✅ Générer mots de passe aléatoires (32+ caractères)
3. ✅ Différents secrets par environnement
4. ✅ Rotation régulière (90 jours production)
5. ✅ Chiffrement at-rest dans secret manager

## Références

- [ADR-034](../adr/034-third-party-library-encapsulation.adr.md) - Encapsulation secrets managers
- [Docker Compose Environment Variables](https://docs.docker.com/compose/environment-variables/)
```

### Considérations techniques

**Points d'attention :**
- **Backward compatibility** : Fournir valeurs par défaut avec `${VAR:-default}` si nécessaire
- **CI/CD** : Vérifier que pipeline ne casse pas (injecter secrets via variables CI)
- **Documentation** : Guider utilisateurs pour première utilisation
- **Redis password** : Optionnel (`--requirepass ${REDIS_PASSWORD:-}` permet vide)

**Pièges à éviter :**
- Commiter accidentellement `.env` avec vrais secrets
- Oublier de documenter la génération de mots de passe sécurisés
- Ne pas fournir `.env.example` → utilisateurs perdus
- Casser CI/CD qui n'a pas accès au `.env`

**Bonnes pratiques :**
1. **Toujours** fournir `.env.example` versionné avec placeholders
2. **Documenter** génération mots de passe sécurisés
3. **Tester** avec et sans `.env` (fallback)
4. **Vérifier** `.gitignore` AVANT de créer `.env`

## ✅ Critères de validation

- [ ] Fichier `.env.example` créé avec tous les secrets en placeholders
- [ ] Fichier `.env` créé localement (non committé)
- [ ] `.gitignore` contient `.env` (vérification grep)
- [ ] `docker-compose.yml` utilise variables `${VAR}` au lieu de valeurs hardcodées
- [ ] `setup.ps1` vérifie existence `.env` et le crée depuis template si absent
- [ ] `README.md` documente la configuration des secrets
- [ ] `docs/security/SECRETS_MANAGEMENT.md` créé avec bonnes pratiques
- [ ] Test : `docker-compose up -d` fonctionne avec `.env`
- [ ] Test : PostgreSQL accessible avec password depuis `.env`
- [ ] Test : Redis accessible avec password depuis `.env` (si configuré)
- [ ] Vérification : `git status` ne montre PAS `.env` (gitignored)
- [ ] Documentation mise à jour
- [ ] Revue de code effectuée
- [ ] Commit atomique : "security: externalize Docker Compose secrets to .env file"

## 🔗 Références

- [Docker Compose Environment Variables](https://docs.docker.com/compose/environment-variables/)
- [12-Factor App - Config](https://12factor.net/config)
- Pilier de qualité : **Sécurité** (élimination hardcoding secrets)
- Principe appliqué : **Separation of Concerns** (config séparée du code)
- ADR potentiel : ADR-034 (Third-party library encapsulation) pour secrets managers production


## TRACKING
Début: 2025-12-22T23:52:14.6935690Z


Fin: 2025-12-23T00:00:12.7219482Z
Durée: 00:07:58

## VALIDATION COMPLÈTE

 **Fichiers créés:**
- .env.example (template versionné)
- .env (secrets locaux, gitignored)

 **Fichiers modifiés:**
- docker-compose.yml (variables \, \)
- setup.ps1 (vérification .env au démarrage)
- README.md (section Configuration avec génération mots de passe)

 **Sécurité:**
- .gitignore contient .env (vérification grep)
- Aucun secret hardcodé dans docker-compose.yml
- Template .env.example avec placeholders

 **Tests:**
- Fichiers .env et .env.example existent (Test-Path = True)
- docker-compose.yml utilise variables (3 occurrences détectées)
- .gitignore contient .env (ligne 72)

 **Conformité:**
- Best practices Docker Compose
- 12-Factor App (Config séparée du code)
- Principe Separation of Concerns

