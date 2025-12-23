---
description: Ansible Fundamentals - ADR compliance, folder structure, project organization, inventory, playbooks
name: Ansible_Fundamentals
applyTo: "**/ansible/**/*.yml,**/ansible/**/*.yaml"
---

# Ansible - Règles Fondamentales

Guide des principes fondamentaux pour le développement Ansible.

## � Types de Fichiers à Créer

| Type de fichier | Usage | Nomenclature |
|----------------|-------|-------------|
| `playbooks/*.yml` | Playbooks d'orchestration | `[action].yml` (ex: `deploy.yml`, `provision.yml`, `site.yml`) |
| `roles/[role]/tasks/main.yml` | Tâches principales d'un rôle | `main.yml` (point d'entrée) + fichiers incluables |
| `roles/[role]/handlers/main.yml` | Handlers de notifications | `main.yml` (handlers réactifs aux changements) |
| `roles/[role]/defaults/main.yml` | Variables par défaut du rôle | `main.yml` (valeurs overridables) |
| `roles/[role]/vars/main.yml` | Variables internes du rôle | `main.yml` (valeurs fixes) |
| `inventories/[env]/hosts.yml` | Inventaire par environnement | `hosts.yml` (production, staging, development) |
| `inventories/[env]/group_vars/*.yml` | Variables par groupe d'hôtes | `[group].yml` (ex: `webservers.yml`, `databases.yml`) |
| `inventories/[env]/host_vars/*.yml` | Variables par hôte spécifique | `[hostname].yml` (ex: `server01.yml`) |

## ⛔ À NE PAS FAIRE

- **Ne génère jamais** de code Ansible sans consulter les ADR dans `docs/adr/`
- **N'utilise jamais** de commandes shell/command si un module natif existe
- **N'écris jamais** de tâches non idempotentes
- **Ne stocke jamais** de secrets en clair dans les fichiers YAML
- **Ne crée jamais** de rôle sans la structure standardisée complète
- **N'omets jamais** le préfixe du rôle sur les variables
- **N'oublie jamais** les FQCN (`ansible.builtin.file` au lieu de `file`)
- **Ne déploie jamais** sans avoir testé avec `--check --diff`

## ✅ À FAIRE

- **Consulte toujours** les ADR avant de coder (surtout ADR-022 Idempotence)
- **Utilise toujours** les modules natifs Ansible
- **Préfixe toujours** les variables par le nom du rôle
- **Chiffre toujours** les secrets avec Ansible Vault
- **Documente toujours** chaque rôle avec un README.md
- **Teste toujours** l'idempotence (2 exécutions, changed=0 la 2ème)
- **Structure toujours** les inventaires par environnement
- **Valide toujours** avec ansible-lint avant de commiter

## 🎯 Actions Obligatoires (Mandatory)

### ⚠️ LECTURE ADR OBLIGATOIRE

**AVANT de générer du code Ansible, TOUJOURS lire les ADR applicables dans `docs/adr/` :**

1. ✅ **Consulter les ADR architecturaux** :
   - [002-principe-kiss.adr.md](../../docs/adr/002-principe-kiss.adr.md) - Keep It Simple, Stupid
   - [003-principe-dry.adr.md](../../docs/adr/003-principe-dry.adr.md) - Don't Repeat Yourself
   - [004-principe-yagni.adr.md](../../docs/adr/004-principe-yagni.adr.md) - You Ain't Gonna Need It
   - [022-idempotence.adr.md](../../docs/adr/022-idempotence.adr.md) - Idempotence (CRITIQUE pour Ansible)
   - [019-convention-over-configuration.adr.md](../../docs/adr/019-convention-over-configuration.adr.md) - Convention over Configuration

2. ✅ **Vérifier les ADR spécifiques au projet** avant toute implémentation

3. ✅ **Respecter les décisions documentées** - Ne jamais contourner un ADR sans justification

## 📁 Structure de Dossiers OBLIGATOIRE

### Structure Standard du Projet

```
ansible/
├── ansible.cfg                   # Configuration Ansible
├── requirements.yml              # Collections et rôles externes
│
├── inventories/                  # Inventaires par environnement
│   ├── production/
│   │   ├── hosts.yml             # Hosts de production
│   │   ├── group_vars/
│   │   │   ├── all.yml           # Variables globales prod
│   │   │   └── [group].yml       # Variables par groupe
│   │   └── host_vars/
│   │       └── [hostname].yml    # Variables par host
│   ├── staging/
│   │   └── ...
│   └── development/
│       └── ...
│
├── playbooks/                    # Playbooks d'orchestration
│   ├── site.yml                  # Playbook principal
│   ├── deploy.yml                # Déploiement applicatif
│   ├── provision.yml             # Provisionnement infrastructure
│   └── [action].yml              # Playbooks par action
│
├── roles/                        # Rôles Ansible
│   └── [role_name]/
│       ├── README.md             # Documentation du rôle
│       ├── defaults/
│       │   └── main.yml          # Variables par défaut
│       ├── vars/
│       │   └── main.yml          # Variables internes
│       ├── tasks/
│       │   ├── main.yml          # Point d'entrée
│       │   └── [task].yml        # Fichiers de tâches
│       ├── handlers/
│       │   └── main.yml          # Handlers
│       ├── templates/
│       │   └── [file].j2         # Templates Jinja2
│       ├── files/
│       │   └── [file]            # Fichiers statiques
│       └── meta/
│           └── main.yml          # Métadonnées et dépendances
│
├── group_vars/                   # Variables globales (tous environnements)
│   └── all.yml
│
├── library/                      # Modules custom (si nécessaire)
├── filter_plugins/               # Filtres Jinja2 custom
└── callback_plugins/             # Callback plugins custom
```

### Structure d'un Rôle

```
roles/[role_name]/
├── README.md                     # OBLIGATOIRE - Documentation
├── defaults/
│   └── main.yml                  # Variables par défaut (override par l'utilisateur)
├── vars/
│   └── main.yml                  # Variables internes (pas d'override)
│   └── [os_family].yml           # Variables OS-specific
├── tasks/
│   ├── main.yml                  # Point d'entrée (dispatch)
│   ├── install.yml               # Installation des packages
│   ├── configure.yml             # Configuration
│   ├── service.yml               # Gestion du service
│   └── [platform].yml            # Tâches OS-specific
├── handlers/
│   └── main.yml                  # Handlers (restart, reload, etc.)
├── templates/
│   └── [config].conf.j2          # Templates de configuration
├── files/
│   └── [static_file]             # Fichiers statiques
├── meta/
│   └── main.yml                  # Dépendances et métadonnées
└── molecule/                     # Tests Molecule
    └── default/
        ├── molecule.yml
        ├── converge.yml
        └── verify.yml
```

## 📝 Conventions de Nommage

### Noms des Fichiers et Répertoires

| Type | Convention | Exemple |
|------|------------|---------|
| **Rôles** | snake_case | `nginx_proxy`, `postgresql_server` |
| **Playbooks** | snake_case.yml | `deploy_app.yml`, `configure_db.yml` |
| **Variables** | snake_case | `nginx_worker_processes` |
| **Handlers** | Phrase descriptive | `Restart nginx`, `Reload systemd` |
| **Tags** | snake_case | `install`, `configure`, `nginx` |
| **Templates** | nom_original.ext.j2 | `nginx.conf.j2`, `app.service.j2` |

### Préfixage des Variables

```yaml
# ✅ BON : Préfixe par nom du rôle
nginx_worker_processes: 4
nginx_client_max_body_size: "64m"
nginx_proxy_connect_timeout: 60

postgresql_version: "15"
postgresql_max_connections: 200
postgresql_shared_buffers: "256MB"

# ❌ MAUVAIS : Pas de préfixe
worker_processes: 4  # Conflit potentiel
version: "15"        # Ambigu
```

## 🔧 Structure des Playbooks

### Playbook Principal (site.yml)

```yaml
---
# site.yml - Playbook principal d'orchestration
# LIRE docs/adr/ avant modification

- name: Provision infrastructure
  import_playbook: provision.yml
  tags: [provision]

- name: Configure common settings
  import_playbook: common.yml
  tags: [common]

- name: Deploy applications
  import_playbook: deploy.yml
  tags: [deploy]
```

### Structure d'un Playbook

```yaml
---
# playbooks/deploy_app.yml
# Description: Déploiement de l'application [nom]
# ADR applicables: ADR-022 (Idempotence), ADR-002 (KISS)

- name: Deploy application
  hosts: app_servers
  become: true
  gather_facts: true

  vars:
    # Variables locales au playbook
    deploy_timeout: 300

  pre_tasks:
    - name: Validate deployment prerequisites
      ansible.builtin.assert:
        that:
          - app_version is defined
          - app_version | length > 0
        fail_msg: "app_version must be defined"

  roles:
    - role: common
      tags: [common]
    - role: app_deploy
      tags: [deploy]

  post_tasks:
    - name: Verify application is running
      ansible.builtin.uri:
        url: "http:/{{ inventory_hostname }}:{{ app_port }}/health"
        status_code: 200
      retries: 5
      delay: 10
```

## 📦 Structure des Tasks

### Point d'Entrée (tasks/main.yml)

```yaml
---
# roles/nginx/tasks/main.yml
# Respecter ADR-022 (Idempotence) pour toutes les tâches

- name: Include OS-specific variables
  ansible.builtin.include_vars: "{{ item }}"
  with_first_found:
    - "{{ ansible_distribution }}-{{ ansible_distribution_major_version }}.yml"
    - "{{ ansible_distribution }}.yml"
    - "{{ ansible_os_family }}.yml"
    - "default.yml"

- name: Install nginx
  ansible.builtin.include_tasks: install.yml
  tags: [install]

- name: Configure nginx
  ansible.builtin.include_tasks: configure.yml
  tags: [configure]

- name: Manage nginx service
  ansible.builtin.include_tasks: service.yml
  tags: [service]
```

### Tâches Idempotentes

```yaml
---
# roles/nginx/tasks/install.yml

- name: Install nginx package
  ansible.builtin.package:
    name: "{{ nginx_package_name }}"
    state: present
  notify: Restart nginx

- name: Ensure nginx directories exist
  ansible.builtin.file:
    path: "{{ item }}"
    state: directory
    owner: "{{ nginx_user }}"
    group: "{{ nginx_group }}"
    mode: "0755"
  loop:
    - "{{ nginx_conf_dir }}"
    - "{{ nginx_sites_available_dir }}"
    - "{{ nginx_sites_enabled_dir }}"
    - "{{ nginx_log_dir }}"
```

## 📋 Variables

### Defaults (defaults/main.yml)

```yaml
---
# roles/nginx/defaults/main.yml
# Variables par défaut - peuvent être overridées par l'utilisateur

# Package
nginx_package_name: nginx
nginx_package_state: present

# Paths
nginx_conf_dir: /etc/nginx
nginx_sites_available_dir: "{{ nginx_conf_dir }}/sites-available"
nginx_sites_enabled_dir: "{{ nginx_conf_dir }}/sites-enabled"
nginx_log_dir: /var/log/nginx

# Configuration
nginx_worker_processes: auto
nginx_worker_connections: 1024
nginx_client_max_body_size: "64m"

# Service
nginx_service_name: nginx
nginx_service_state: started
nginx_service_enabled: true

# User/Group
nginx_user: www-data
nginx_group: www-data
```

### Vars Internes (vars/main.yml)

```yaml
---
# roles/nginx/vars/main.yml
# Variables internes - NE PAS overrider

# Mapping OS-specific
__nginx_packages:
  Debian: nginx
  RedHat: nginx
  
__nginx_user:
  Debian: www-data
  RedHat: nginx

# Résolution
nginx_package_name: "{{ __nginx_packages[ansible_os_family] | default('nginx') }}"
nginx_user: "{{ __nginx_user[ansible_os_family] | default('nginx') }}"
```

## 🔔 Handlers

```yaml
---
# roles/nginx/handlers/main.yml

- name: Restart nginx
  ansible.builtin.systemd:
    name: "{{ nginx_service_name }}"
    state: restarted
    daemon_reload: true
  listen: "Restart nginx"

- name: Reload nginx
  ansible.builtin.systemd:
    name: "{{ nginx_service_name }}"
    state: reloaded
  listen: "Reload nginx"

- name: Validate nginx configuration
  ansible.builtin.command: nginx -t
  changed_when: false
  listen: "Validate nginx config"
```

## 📄 Templates Jinja2

```jinja2
{# templates/nginx.conf.j2 #}
# {{ ansible_managed }}
# Configuration générée par Ansible - Ne pas modifier manuellement

user {{ nginx_user }};
worker_processes {{ nginx_worker_processes }};
pid /run/nginx.pid;

events {
    worker_connections {{ nginx_worker_connections }};
    multi_accept on;
}

http {
    sendfile on;
    tcp_nopush on;
    tcp_nodelay on;
    keepalive_timeout 65;
    types_hash_max_size 2048;

    client_max_body_size {{ nginx_client_max_body_size }};

    include /etc/nginx/mime.types;
    default_type application/octet-stream;

    access_log {{ nginx_log_dir }}/access.log;
    error_log {{ nginx_log_dir }}/error.log;

    gzip on;
    gzip_disable "msie6";

{% for site in nginx_sites %}
    include {{ nginx_sites_enabled_dir }}/{{ site.name }};
{% endfor %}
}
```

## ✅ Checklist Ansible

**Avant de compléter du code Ansible, VÉRIFIER :**

- [ ] ADR pertinents consultés (surtout ADR-022 Idempotence)
- [ ] Structure de dossiers conforme
- [ ] Variables préfixées par le nom du rôle
- [ ] Tâches idempotentes (relancer = pas de changement)
- [ ] FQCN utilisés (`ansible.builtin.file`, pas `file`)
- [ ] Handlers définis pour les redémarrages
- [ ] Tags appropriés sur toutes les tâches
- [ ] Templates avec `{{ ansible_managed }}`
- [ ] README.md dans chaque rôle
- [ ] Tests Molecule si applicable
- [ ] `ansible-lint` sans erreurs
