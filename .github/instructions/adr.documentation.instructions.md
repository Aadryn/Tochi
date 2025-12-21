---
description: Architecture Decision Records (ADR) - création, structure, format MADR et gestion du cycle de vie
name: ADR_Documentation
applyTo: "**/docs/adr/**/*.adr.md"
---

# Architecture Decision Records (ADR) - Guide Expert

Guide complet pour créer, structurer et gérer les Architecture Decision Records (ADR) au format MADR.

## 🎯 Actions Obligatoires (Mandatory)

### Format et Structure

**TOUJOURS utiliser le format [MADR](https://adr.github.io/madr/) (Markdown Any Decision Records).**

**Structure de fichier OBLIGATOIRE :**

```markdown
# [Numéro]. [Titre court et descriptif de la décision]

Date: YYYY-MM-DD

## Statut

[Proposé | Accepté | Rejeté | Déprécié | Remplacé par ADR-XXX]

## Contexte

[Description du problème ou de la question qui nécessite une décision]

## Décision

[Description claire et factuelle de la décision prise]

## Conséquences

### Positives
- [Avantage 1]
- [Avantage 2]

### Négatives
- [Inconvénient 1 et comment il est mitigé]
- [Inconvénient 2]

### Neutres
- [Impact neutre 1]

## Alternatives considérées

### Option A: [Nom de l'alternative]
- **Description** : [Brève description]
- **Avantages** : [Points positifs]
- **Inconvénients** : [Points négatifs]
- **Raison du rejet** : [Pourquoi cette option n'a pas été choisie]

### Option B: [Nom de l'alternative]
- ...

## Références

- [Lien vers documentation pertinente]
- [Lien vers discussions (PR, issues, etc.)]
- [Lien vers code concerné]
```

### Nommage des Fichiers

**Convention de nommage OBLIGATOIRE** : `NNN-titre-en-kebab-case.adr.md`

- `NNN` : Numéro séquentiel avec padding de zéros (001, 002, 003, ...)
- Titre en kebab-case (minuscules, mots séparés par tirets)
- Extension `.adr.md` OBLIGATOIRE

**Exemples valides :**
```
001-multi-workload-architecture.adr.md
002-systemd-hardening.adr.md
003-nginx-reverse-proxy.adr.md
015-migration-postgresql-16.adr.md
```

**Exemples INVALIDES :**
```
❌ adr-001.md (extension incorrecte)
❌ 1-architecture.adr.md (numérotation sans padding)
❌ 001-Architecture_Decision.adr.md (PascalCase, underscore)
❌ architecture-decision.adr.md (pas de numéro)
```

### Emplacement des Fichiers

**Tous les ADR DOIVENT être stockés dans** : `docs/adr/`

**Structure du répertoire :**
```
docs/
└── adr/
    ├── README.md                                    # Index et documentation
    ├── 001-multi-workload-architecture.adr.md
    ├── 002-systemd-hardening.adr.md
    └── 003-nginx-reverse-proxy.adr.md
```

## 📝 Contenu des Sections

### Section "Contexte"

**DOIT contenir :**
- Description claire du problème ou de la question
- Contraintes techniques identifiées
- Contraintes métier ou organisationnelles
- État actuel du système (si applicable)
- Forces en présence qui influencent la décision

**Exemple :**
```markdown
## Contexte

Le projet nécessite de déployer des applications .NET complexes composées de plusieurs types de composants :
- Applications web (webapp) exposées via HTTP/HTTPS
- API REST (webapi) avec points d'entrée spécifiques
- Workers (services background) sans exposition HTTP
- Sites statiques (HTML/CSS/JS) sans runtime .NET

Contraintes identifiées :
- **Réutilisabilité** : Éviter de créer un rôle Ansible différent pour chaque type d'application
- **Convention de nommage** : Maintenir une structure cohérente pour tous les composants
- **Flexibilité** : Permettre d'activer/désactiver des composants selon les environnements
- **Dépendances** : Certains composants (workers) dépendent d'autres (webapi) et doivent démarrer dans le bon ordre
```

### Section "Décision"

**DOIT contenir :**
- Énoncé clair et factuel de la décision prise
- Approche technique choisie
- Justification basée sur les contraintes du contexte
- Exemples concrets si applicable

**Caractéristiques :**
- ✅ Factuelle, sans opinion subjective
- ✅ Actionnable et précise
- ✅ Référence au contexte établi
- ❌ Pas d'ambiguïté sur ce qui a été décidé

**Exemple :**
```markdown
## Décision

Implémenter une **architecture multi-workload** dans un seul rôle Ansible `dotnet-app`, où chaque composant de l'application est défini via la variable `dotnet_app_components`.

Structure de configuration adoptée :

```yaml
dotnet_app_components:
  - name: myapp-webapi
    type: webapi
    port: 5001
    enabled: true
  - name: myapp-worker
    type: worker
    enabled: true
    depends_on: myapp-webapi
```

Cette approche permet de :
- Gérer tous les types de composants avec un seul rôle
- Activer/désactiver des composants via la variable `enabled`
- Gérer les dépendances via `depends_on`
```

### Section "Conséquences"

**DOIT contenir trois sous-sections :**

#### Positives (Avantages)

- Bénéfices techniques directs
- Gains en maintenabilité, performance, sécurité
- Améliorations opérationnelles

**Exemple :**
```markdown
### Positives
- **Réutilisabilité maximale** : Un seul rôle pour tous les types d'applications .NET
- **Convention cohérente** : Nommage uniforme pour tous les composants
- **Flexibilité environnement** : Activation conditionnelle des composants
- **Gestion des dépendances** : Ordre de démarrage contrôlé via `depends_on`
```

#### Négatives (Inconvénients et Mitigations)

- Coûts, limitations, complexité ajoutée
- **TOUJOURS inclure comment l'inconvénient est mitigé**

**Exemple :**
```markdown
### Négatives
- **Complexité accrue du rôle** : Plus de logique conditionnelle dans les tasks
  - *Mitigation* : Documentation claire et tests Molecule couvrant tous les scénarios
- **Courbe d'apprentissage** : Nécessite de comprendre la structure `dotnet_app_components`
  - *Mitigation* : Exemples fournis dans `inventories/dev/group_vars/`
```

#### Neutres (Impacts sans valeur positive/négative)

- Changements qui ne sont ni bénéfiques ni problématiques
- Impacts sur d'autres parties du système

**Exemple :**
```markdown
### Neutres
- Nécessite une variable `dotnet_app_components` structurée dans l'inventaire
- Impact sur la structure des fichiers de variables d'inventaire
```

### Section "Alternatives considérées"

**DOIT contenir :**
- Au moins 2-3 alternatives sérieusement évaluées
- Pour chaque alternative : description, avantages, inconvénients, raison du rejet
- Arguments factuels pour chaque rejet

**Structure OBLIGATOIRE pour chaque alternative :**
```markdown
### Option A: [Nom descriptif]
- **Description** : [Explication concise]
- **Avantages** : 
  - Avantage 1
  - Avantage 2
- **Inconvénients** : 
  - Inconvénient 1
  - Inconvénient 2
- **Raison du rejet** : [Justification factuelle du pourquoi cette option n'a pas été retenue]
```

**Exemple :**
```markdown
## Alternatives considérées

### Option A: Rôles séparés par type (dotnet-webapp, dotnet-webapi, dotnet-worker)
- **Description** : Créer un rôle Ansible distinct pour chaque type de composant
- **Avantages** :
  - Séparation des responsabilités plus claire
  - Rôles plus simples individuellement
- **Inconvénients** :
  - Duplication massive de code entre rôles
  - Maintenance de 4+ rôles distincts
  - Pas de gestion unifiée des dépendances entre composants
- **Raison du rejet** : Violation du principe DRY (Don't Repeat Yourself), maintenance non soutenable à long terme

### Option B: Rôle générique + rôles spécifiques héritant du générique
- **Description** : Créer un rôle `dotnet-base` et des rôles spécialisés
- **Avantages** :
  - Réutilisation du code de base
  - Spécialisation possible par type
- **Inconvénients** :
  - Complexité de l'héritage de rôles Ansible
  - Difficulté à gérer les dépendances inter-composants
  - Plus de fichiers à maintenir
- **Raison du rejet** : Complexité architecturale supérieure sans bénéfice opérationnel significatif
```

### Section "Références"

**DOIT contenir :**
- Liens vers documentation technique pertinente
- Liens vers discussions (Pull Requests, Issues, ADR liés)
- Liens vers code concerné dans le repository
- Références externes (articles, RFC, documentation officielle)

**Exemple :**
```markdown
## Références

- [Documentation Ansible sur les rôles réutilisables](https://docs.ansible.com/ansible/latest/user_guide/playbooks_reuse_roles.html)
- Code source : `roles/dotnet-app/tasks/main.yml`
- Variables : `inventories/dev/group_vars/dotnet_servers.yml`
- Issue #42: Discussion sur l'architecture multi-composants
- ADR-002: Systemd hardening (dépendance)
```

## 🔄 Cycle de Vie des ADR

### Statuts Possibles

**UTILISER UNIQUEMENT ces statuts :**

- **Proposé** : ADR en cours de review, décision pas encore prise
- **Accepté** : ADR validé et décision appliquée
- **Rejeté** : ADR évalué mais décision rejetée (garder la trace)
- **Déprécié** : ADR accepté mais devenu obsolète
- **Remplacé par ADR-XXX** : ADR remplacé par une nouvelle décision

### Principe d'Immutabilité

**RÈGLE ABSOLUE** : Un ADR accepté NE DOIT JAMAIS être modifié.

**Actions autorisées :**
- ✅ Changer le statut de "Accepté" à "Déprécié" ou "Remplacé par ADR-XXX"
- ✅ Corriger des fautes de frappe mineures
- ✅ Ajouter des références complémentaires

**Actions INTERDITES :**
- ❌ Modifier le contexte, la décision ou les conséquences
- ❌ Changer les alternatives considérées
- ❌ Réécrire un ADR pour refléter une nouvelle réalité

**Pour faire évoluer une décision :**
1. Créer un NOUVEL ADR avec le numéro suivant
2. Référencer l'ADR précédent dans le contexte
3. Marquer l'ancien ADR comme "Remplacé par ADR-XXX"
4. Mettre à jour l'index dans `docs/adr/README.md`

**Exemple de dépréciation :**
```markdown
# 002. Systemd hardening par défaut

Date: 2025-11-27

## Statut

~~Accepté~~ **Remplacé par ADR-007** (2025-12-15)

**Raison du remplacement** : Nouvelle approche avec profils de sécurité contextuels suite à incompatibilités identifiées avec certaines applications legacy.

[... reste de l'ADR inchangé ...]
```

## 📋 Processus de Création d'un ADR

### Étape 1: Identifier une Décision Significative

**Créer un ADR lorsque :**
- ✅ La décision a un impact significatif sur l'architecture du système
- ✅ La décision est difficile ou coûteuse à inverser
- ✅ Plusieurs alternatives viables existent et un choix justifié est nécessaire
- ✅ La décision affecte plusieurs équipes, composants ou services
- ✅ Le contexte et la justification doivent être préservés pour le futur
- ✅ La décision influence des choix technologiques futurs

**NE PAS créer d'ADR pour :**
- ❌ Décisions triviales ou évidentes
- ❌ Choix purement tactiques sans impact architectural
- ❌ Décisions facilement réversibles sans coût
- ❌ Préférences personnelles sans justification technique

### Étape 2: Obtenir le Numéro Séquentiel

```bash
# Lister les ADR existants pour identifier le prochain numéro
ls docs/adr/*.adr.md | sort
# Résultat: 001, 002, 003 → prochain numéro = 004
```

### Étape 3: Créer le Fichier

```bash
# Créer le nouveau fichier ADR
touch docs/adr/004-titre-de-la-decision.adr.md
```

### Étape 4: Rédiger le Contenu

1. **Commencer par le contexte** : Décrire le problème objectivement
2. **Évaluer les alternatives** : Lister et comparer plusieurs options
3. **Documenter la décision** : Énoncer clairement la solution retenue
4. **Analyser les conséquences** : Positives, négatives (avec mitigations), neutres
5. **Ajouter les références** : Documentation, code, discussions

**Principes de rédaction :**
- ✅ **Factualité** : S'en tenir aux faits et contraintes techniques
- ✅ **Clarté** : Privilégier la lisibilité et la concision
- ✅ **Complétude** : Fournir suffisamment d'information pour comprendre le "pourquoi"
- ❌ **Éviter les opinions subjectives** : "Je pense que...", "C'est mieux parce que..."

### Étape 5: Mettre à Jour l'Index

**Ajouter une ligne dans** `docs/adr/README.md` :

```markdown
| [004](004-titre-de-la-decision.adr.md) | Titre court | Proposé | 2025-11-27 |
```

**TOUJOURS maintenir l'index trié par numéro croissant.**

### Étape 6: Soumettre en Review

- Créer une Pull Request avec l'ADR
- Solliciter feedback de l'équipe
- Ajuster si nécessaire (seulement tant que statut = "Proposé")
- Fusionner une fois consensus atteint

### Étape 7: Changer le Statut

Une fois l'ADR accepté :
1. Modifier le statut de "Proposé" à "Accepté"
2. Mettre à jour l'index dans README.md
3. Implémenter la décision dans le code

## ✅ Checklist de Validation

**AVANT de considérer un ADR comme terminé :**

- [ ] Frontmatter présent avec titre court et date
- [ ] Numérotation correcte (NNN avec padding de zéros)
- [ ] Extension `.adr.md` utilisée
- [ ] Fichier placé dans `docs/adr/`
- [ ] Statut défini (Proposé, Accepté, Rejeté, Déprécié, Remplacé)
- [ ] Section "Contexte" : Problème et contraintes clairement décrits
- [ ] Section "Décision" : Solution retenue énoncée factuellement
- [ ] Section "Conséquences" : Positives, négatives (avec mitigations), neutres documentées
- [ ] Section "Alternatives" : Au moins 2-3 options évaluées avec justification du rejet
- [ ] Section "Références" : Liens vers documentation, code, discussions
- [ ] Contenu factuel sans opinions subjectives
- [ ] Syntaxe Markdown valide
- [ ] Index mis à jour dans `docs/adr/README.md`
- [ ] Format MADR respecté intégralement

## 💡 Exemples Complets

### Exemple 1 : ADR Simple et Clair

```markdown
# 003. Utilisation de nginx comme reverse proxy

Date: 2025-11-27

## Statut

Accepté

## Contexte

Les applications .NET Kestrel peuvent être exposées directement sur Internet ou derrière un reverse proxy.

Contraintes identifiées :
- **Performance** : Kestrel optimisé pour le dynamique, moins pour le statique
- **Sécurité** : Exposition directe = surface d'attaque plus large
- **Opérations** : Besoin de rate limiting, compression, caching
- **SSL/TLS** : Terminaison SSL centralisée préférable
- **Standardisation** : Infrastructure cohérente pour tous les services

## Décision

Utiliser **nginx comme reverse proxy obligatoire** devant toutes les applications .NET exposées (webapp, webapi).

Configuration standard :
- Nginx écoute sur ports 80/443
- Kestrel écoute sur localhost uniquement (ports 5000+)
- Nginx proxy_pass vers Kestrel
- Nginx gère SSL/TLS, compression, rate limiting

## Conséquences

### Positives
- **Performance statique** : Nginx sert directement CSS/JS/images sans solliciter Kestrel
- **Sécurité renforcée** : Kestrel non exposé directement, nginx filtre les requêtes malformées
- **Rate limiting** : Protection contre abus et DDoS au niveau nginx
- **SSL centralisé** : Certificats gérés au même endroit pour tous les services
- **Compression** : Gzip/Brotli géré par nginx, soulage Kestrel
- **Logs unifiés** : Format de logs standardisé via nginx

### Négatives
- **Latence additionnelle** : +1-2ms par requête pour le proxy
  - *Mitigation* : Négligeable comparé aux bénéfices, HTTP/2 et keepalive compensent
- **Composant supplémentaire** : Maintenance de nginx en plus de Kestrel
  - *Mitigation* : Nginx stable, configuration versionnée, monitoring en place
- **Complexité de debug** : Une couche de plus à analyser en cas de problème
  - *Mitigation* : Logs nginx détaillés, headers X-Forwarded-* pour traçabilité

### Neutres
- Nécessite configuration nginx pour chaque application
- Impact sur la structure des templates Ansible

## Alternatives considérées

### Option A: Exposition directe de Kestrel
- **Description** : Kestrel écoute directement sur ports 80/443 sans reverse proxy
- **Avantages** :
  - Architecture plus simple (moins de composants)
  - Latence minimale (pas de hop supplémentaire)
  - Configuration plus directe
- **Inconvénients** :
  - Kestrel moins performant pour servir du contenu statique
  - Pas de rate limiting natif dans Kestrel
  - Surface d'attaque plus large (Kestrel exposé)
  - Gestion SSL individuelle par application
- **Raison du rejet** : Manque de fonctionnalités opérationnelles critiques (rate limiting, compression, caching). Performance statique médiocre. Posture de sécurité faible.

### Option B: Utilisation d'Azure Application Gateway / AWS ALB
- **Description** : Reverse proxy cloud-native géré par le fournisseur
- **Avantages** :
  - Pas de maintenance de l'infrastructure reverse proxy
  - Haute disponibilité garantie par le cloud provider
  - Intégration native avec WAF
- **Inconvénients** :
  - Coût mensuel élevé (>100€/mois)
  - Lock-in avec le cloud provider
  - Configuration via portail/API moins flexible
  - Déploiements on-premise impossibles
- **Raison du rejet** : Infrastructure cible est principalement on-premise. Coût récurrent non justifié. Besoin de portabilité cloud/on-prem.

### Option C: Utilisation de Traefik ou Envoy
- **Description** : Reverse proxy moderne avec configuration dynamique
- **Avantages** :
  - Configuration dynamique via labels/annotations
  - Support natif de service mesh
  - Metrics et tracing intégrés
- **Inconvénients** :
  - Moins mature et éprouvé que nginx en production
  - Équipe moins familière avec Traefik/Envoy
  - Documentation et communauté plus restreintes
  - Complexité supplémentaire pour cas d'usage simples
- **Raison du rejet** : Nginx répond parfaitement au besoin. Expertise équipe sur nginx existante. Pas de justification pour introduire une nouvelle technologie.

## Références

- [Documentation nginx reverse proxy](https://docs.nginx.com/nginx/admin-guide/web-server/reverse-proxy/)
- [Microsoft: Best practices for Kestrel behind reverse proxy](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/when-to-use-a-reverse-proxy)
- Configuration : `roles/dotnet-app/templates/nginx.conf.j2`
- ADR-001: Architecture multi-workload (dépendance)
- ADR-002: Systemd hardening (dépendance)
```

### Exemple 2 : ADR Déprécié

```markdown
# 005. Utilisation de PostgreSQL 12

Date: 2025-06-15

## Statut

~~Accepté~~ **Remplacé par ADR-015** (2025-11-30)

**Raison du remplacement** : PostgreSQL 12 arrive en fin de support (EOL novembre 2024). Migration vers PostgreSQL 16 pour bénéficier du support à long terme et des nouvelles fonctionnalités.

## Contexte

[... contenu original inchangé ...]

## Décision

[... contenu original inchangé ...]

## Conséquences

[... contenu original inchangé ...]

## Alternatives considérées

[... contenu original inchangé ...]

## Références

- [... références originales ...]
- **Nouveau** : [ADR-015: Migration vers PostgreSQL 16](015-migration-postgresql-16.adr.md)
```

## 🎓 Bonnes Pratiques

### Pour la Rédaction

1. **Écrire au présent** : "Nous utilisons nginx" plutôt que "Nous allons utiliser nginx"
2. **Être objectif** : Faits et données plutôt qu'opinions
3. **Quantifier quand possible** : "Latence de +2ms" plutôt que "légère latence"
4. **Documenter les trade-offs** : Reconnaître les inconvénients et les mitigations
5. **Penser au futur** : L'ADR doit être compréhensible dans 2-3 ans

### Pour la Maintenance

1. **Ne JAMAIS modifier un ADR accepté** : Créer un nouvel ADR à la place
2. **Maintenir l'index à jour** : `docs/adr/README.md` doit refléter l'état actuel
3. **Référencer les ADR liés** : Créer un graphe de dépendances implicite
4. **Archiver les ADR dépréciés** : Garder la trace, ne pas supprimer

### Pour la Review

1. **Vérifier la factualité** : Pas d'opinions déguisées en faits
2. **Challenger les alternatives** : Au moins 2-3 options doivent être évaluées
3. **Valider les mitigations** : Chaque inconvénient doit avoir une mitigation
4. **Vérifier les références** : Liens valides et documentation accessible

## 🔗 Références

- [Architecture Decision Records (ADR.github.io)](https://adr.github.io/)
- [MADR Template](https://adr.github.io/madr/)
- [Why Write ADRs - GitHub Blog](https://github.blog/2020-08-13-why-write-adrs/)
- [Architectural Decision Records - Martin Fowler](https://martinfowler.com/articles/documenting-architecture-decisions.html)
- [ADR Tools](https://github.com/npryce/adr-tools)
- Copilot instructions: `.github/copilot-instructions.md` (section Conformité ADR)
