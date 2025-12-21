---
description: Role development best practices - structure, tasks, variables, handlers, templates
name: Ansible_Roles_Development
applyTo: "**/roles/**/*.{yml,yaml}"
---

# Ansible Roles - Guide Expert

## 🎯 Actions Obligatoires (Mandatory)

**À TOUJOURS faire lors de la création/modification d'un rôle :**

1. ✅ **Structure standardisée** : Créer la structure complète du rôle
   ```
   role_name/
   ├── defaults/main.yml    # Variables par défaut
   ├── tasks/main.yml       # Point d'entrée des tasks
   ├── handlers/main.yml    # Gestionnaires d'événements
   ├── templates/           # Fichiers Jinja2
   ├── files/               # Fichiers statiques
   ├── vars/                # Variables spécifiques
   ├── meta/main.yml        # Métadonnées et dépendances
   └── README.md            # Documentation
   ```

2. ✅ **Nommer le rôle** : `snake_case` avec responsabilité unique claire
   - ✅ Bon : `nginx_reverse_proxy`, `postgresql_ha`, `docker_registry`
   - ❌ Mauvais : `web-server`, `MyRole`, `configure_everything`

3. ✅ **Variables préfixées** : Toutes les variables doivent avoir le préfixe du rôle
   ```yaml
   # ✅ Bon
   nginx_worker_processes: 4
   nginx_port: 80
   
   # ❌ Mauvais
   worker_processes: 4
   port: 80
   ```

4. ✅ **Documentation** : README.md avec description, variables, exemples
5. ✅ **Idempotence OBLIGATOIRE** : Chaque task DOIT être réexécutable sans effets de bord
6. ✅ **Dry-run compatible** : Le rôle DOIT fonctionner avec `--check` et `--diff`
7. ✅ **Tests automatisés** : Tests Molecule OBLIGATOIRES pour chaque rôle
8. ✅ **Test d'idempotence** : Le rôle DOIT passer le test idempotence (2ème run = 0 changes)
9. ✅ **Secrets** : Toujours utiliser Vault pour les mots de passe/clés
   ```yaml
   app_admin_password: "{{ vault_app_admin_password }}"
   ```

## Structure des Tasks

### Organisation des Tasks (main.yml)

```yaml
# roles/example/tasks/main.yml
---
# 1. Charger les variables spécifiques à l'OS
- name: Include OS-specific variables
  include_vars: "{{ ansible_os_family }}.yml"
  tags: always

# 2. Validation pré-installation
- name: Include pre-flight checks
  import_tasks: preflight.yml
  tags: preflight

# 3. Installation des packages
- name: Include installation tasks
  import_tasks: install.yml
  tags: install

# 4. Configuration
- name: Include configuration tasks
  import_tasks: configure.yml
  tags: configure

# 5. Gestion du service
- name: Include service management
  import_tasks: service.yml
  tags: service
```

### Découpage Logique des Fichiers

**Créer des fichiers séparés pour chaque responsabilité :**

```yaml
# tasks/preflight.yml - Validations avant exécution
---
- name: Verify required variables are defined
  assert:
    that:
      - example_version is defined
      - example_port is defined
    fail_msg: "Required variables are not defined"

- name: Check if port is available
  wait_for:
    port: "{{ example_port }}"
    state: stopped
    timeout: 5
  ignore_errors: true
  register: port_check

- name: Fail if port is already in use
  fail:
    msg: "Port {{ example_port }} is already in use"
  when: port_check is failed
```

```yaml
# tasks/install.yml - Installation des packages
---
- name: Install required packages (Debian/Ubuntu)
  apt:
    name: "{{ example_packages }}"
    state: present
    update_cache: true
    cache_valid_time: 3600
  when: ansible_os_family == 'Debian'

- name: Install required packages (RHEL/CentOS)
  yum:
    name: "{{ example_packages }}"
    state: present
  when: ansible_os_family == 'RedHat'

- name: Create service user
  user:
    name: "{{ example_user }}"
    system: true
    shell: /bin/false
    create_home: false
```

```yaml
# tasks/configure.yml - Configuration
---
- name: Create configuration directory
  file:
    path: "{{ example_config_dir }}"
    state: directory
    owner: "{{ example_user }}"
    group: "{{ example_group }}"
    mode: '0755'

- name: Deploy configuration file
  template:
    src: config.j2
    dest: "{{ example_config_dir }}/config.conf"
    owner: "{{ example_user }}"
    group: "{{ example_group }}"
    mode: '0644'
    validate: "{{ example_binary }} --validate-config %s"
  notify: Restart example service
```

```yaml
# tasks/service.yml - Gestion du service
---
- name: Deploy systemd unit file
  template:
    src: example.service.j2
    dest: /etc/systemd/system/example.service
    mode: '0644'
  notify:
    - Reload systemd
    - Restart example service

- name: Enable and start service
  systemd:
    name: example
    enabled: true
    state: started
    daemon_reload: true
```

## Variables et Defaults

### Structure de defaults/main.yml

```yaml
# roles/example/defaults/main.yml
---
# =============================================================================
# VERSION ET PACKAGES
# =============================================================================
example_version: "1.2.3"
example_packages:
  - example-server
  - example-client
  - example-tools

# =============================================================================
# UTILISATEUR ET GROUPE
# =============================================================================
example_user: example
example_group: example

# =============================================================================
# CHEMINS ET RÉPERTOIRES
# =============================================================================
example_install_dir: /opt/example
example_config_dir: /etc/example
example_data_dir: /var/lib/example
example_log_dir: /var/log/example

# =============================================================================
# CONFIGURATION RÉSEAU
# =============================================================================
example_port: 8080
example_bind_address: "0.0.0.0"
example_max_connections: 100

# =============================================================================
# AUTHENTIFICATION ET SÉCURITÉ
# =============================================================================
example_enable_auth: true
example_admin_user: "admin"
example_admin_password: "{{ vault_example_admin_password }}"  # Depuis vault

# =============================================================================
# SSL/TLS
# =============================================================================
example_enable_ssl: true
example_ssl_cert_path: "/etc/ssl/certs/example.crt"
example_ssl_key_path: "/etc/ssl/private/example.key"
example_ssl_protocols: "TLSv1.2 TLSv1.3"

# =============================================================================
# PERFORMANCE
# =============================================================================
example_worker_processes: "{{ ansible_processor_vcpus }}"
example_worker_connections: 1024
example_cache_size: "256m"

# =============================================================================
# FEATURES FLAGS
# =============================================================================
example_enable_monitoring: true
example_enable_backup: true
example_enable_replication: false

# =============================================================================
# CONFIGURATION AVANCÉE (OVERRIDES)
# =============================================================================
# Permet aux utilisateurs d'ajouter des configurations personnalisées
example_config_overrides: {}
```

### Hiérarchie des Variables

**Ordre de priorité (du plus faible au plus fort) :**
1. `defaults/main.yml` - Valeurs par défaut sûres
2. `vars/main.yml` - Variables spécifiques au rôle
3. `group_vars/` - Configuration par groupe
4. `host_vars/` - Configuration par hôte
5. Extra vars (`-e`) - Surcharge ponctuelle

## Handlers

### Bonnes Pratiques pour Handlers

```yaml
# roles/example/handlers/main.yml
---
# Handler simple
- name: Restart example service
  systemd:
    name: example
    state: restarted

# Handler avec validation préalable
- name: Reload example service
  block:
    - name: Validate configuration before reload
      command: example --validate-config
      changed_when: false
    
    - name: Reload service
      systemd:
        name: example
        state: reloaded

# Handlers groupés avec 'listen'
- name: Validate example configuration
  command: example --validate-config
  changed_when: false
  listen: "restart example"

- name: Stop example service
  systemd:
    name: example
    state: stopped
  listen: "restart example"

- name: Start example service
  systemd:
    name: example
    state: started
  listen: "restart example"

- name: Wait for service to be ready
  wait_for:
    port: "{{ example_port }}"
    delay: 2
    timeout: 30
  listen: "restart example"

# Handler pour systemd daemon-reload
- name: Reload systemd
  systemd:
    daemon_reload: true
```

**Utilisation dans les tasks :**
```yaml
- name: Deploy configuration
  template:
    src: config.j2
    dest: /etc/example/config.conf
  notify: restart example  # Déclenche tous les handlers avec 'listen: restart example'
```

## Templates Jinja2

### Structure de Template

```jinja
{# roles/example/templates/config.j2 #}
# {{ ansible_managed }}
# Configuration file for {{ example_service_name | default('example') }}
# DO NOT EDIT MANUALLY - This file is managed by Ansible
# Generated on: {{ ansible_date_time.iso8601 }}
# Host: {{ inventory_hostname }}

# =============================================================================
# SERVER CONFIGURATION
# =============================================================================
[server]
port = {{ example_port }}
bind_address = {{ example_bind_address }}
max_connections = {{ example_max_connections }}

{% if example_worker_processes is defined %}
worker_processes = {{ example_worker_processes }}
{% endif %}

# =============================================================================
# AUTHENTICATION
# =============================================================================
{% if example_enable_auth %}
[authentication]
enabled = true
admin_user = {{ example_admin_user }}
{% if example_admin_password is defined %}
admin_password = {{ example_admin_password }}
{% endif %}
{% else %}
[authentication]
enabled = false
{% endif %}

# =============================================================================
# SSL/TLS CONFIGURATION
# =============================================================================
{% if example_enable_ssl %}
[ssl]
enabled = true
certificate = {{ example_ssl_cert_path }}
private_key = {{ example_ssl_key_path }}
protocols = {{ example_ssl_protocols }}
{% endif %}

# =============================================================================
# LOGGING
# =============================================================================
[logging]
log_dir = {{ example_log_dir }}
log_level = {{ example_log_level | default('info') }}

# =============================================================================
# CUSTOM OVERRIDES
# =============================================================================
{% if example_config_overrides is defined and example_config_overrides | length > 0 %}
[custom]
{% for key, value in example_config_overrides.items() %}
{{ key }} = {{ value }}
{% endfor %}
{% endif %}
```

### Templates pour Systemd

```jinja
{# roles/example/templates/example.service.j2 #}
# {{ ansible_managed }}
[Unit]
Description={{ example_service_description | default('Example Service') }}
After=network.target
{% if example_requires_database %}
Requires=postgresql.service
After=postgresql.service
{% endif %}

[Service]
Type={{ example_service_type | default('simple') }}
User={{ example_user }}
Group={{ example_group }}
ExecStart={{ example_install_dir }}/bin/example --config {{ example_config_dir }}/config.conf
ExecReload=/bin/kill -HUP $MAINPID
Restart={{ example_restart_policy | default('on-failure') }}
RestartSec=5

# Security hardening
PrivateTmp=true
NoNewPrivileges=true
{% if example_restrict_address_families | default(true) %}
RestrictAddressFamilies=AF_UNIX AF_INET AF_INET6
{% endif %}

# Resource limits
{% if example_limit_nofile is defined %}
LimitNOFILE={{ example_limit_nofile }}
{% endif %}
{% if example_limit_nproc is defined %}
LimitNPROC={{ example_limit_nproc }}
{% endif %}

[Install]
WantedBy=multi-user.target
```

## Métadonnées (meta/main.yml)

```yaml
# roles/example/meta/main.yml
---
galaxy_info:
  role_name: example
  author: Votre Nom
  description: Description courte et claire du rôle
  company: Votre Entreprise (optionnel)
  license: MIT
  min_ansible_version: "2.14"
  
  platforms:
    - name: Ubuntu
      versions:
        - focal
        - jammy
    - name: Debian
      versions:
        - bullseye
        - bookworm
    - name: EL
      versions:
        - 8
        - 9
  
  galaxy_tags:
    - system
    - web
    - database
    - monitoring

# Dépendances vers d'autres rôles
dependencies: []
  # - role: common
  #   vars:
  #     common_packages:
  #       - curl
  #       - git

# Collections requises
collections:
  - community.general
  - ansible.posix
```

## README.md du Rôle

```markdown
# Ansible Role: example

Description courte du rôle et de son objectif.

## Requirements

- Ansible >= 2.14
- OS supportés : Ubuntu 20.04+, Debian 11+, RHEL/CentOS 8+
- Collections requises :
  - `community.general`
  - `ansible.posix`

## Role Variables

### Variables obligatoires

Aucune variable n'est obligatoire, toutes ont des valeurs par défaut.

### Variables principales

| Variable | Default | Description |
|----------|---------|-------------|
| `example_version` | `"1.2.3"` | Version du logiciel à installer |
| `example_port` | `8080` | Port d'écoute du service |
| `example_enable_ssl` | `true` | Activer SSL/TLS |
| `example_admin_password` | `{{ vault_example_admin_password }}` | Mot de passe admin (depuis vault) |

### Variables avancées

Voir `defaults/main.yml` pour la liste complète des variables configurables.

## Dependencies

Aucune dépendance vers d'autres rôles.

## Example Playbook

```yaml
- hosts: servers
  roles:
    - role: example
      vars:
        example_port: 9090
        example_enable_ssl: true
        example_worker_processes: 8
```

## Testing

Utiliser Molecule pour tester le rôle :

```bash
cd roles/example
molecule test
```

## License

MIT

## Author Information

Votre Nom - votre.email@example.com
```

## Anti-Patterns à Éviter

### ❌ Mauvaises Pratiques

```yaml
# ❌ Variables sans préfixe
port: 8080
user: admin

# ❌ Hardcoder des valeurs
- name: Configure service
  lineinfile:
    path: /etc/app/config
    line: "port=8080"

# ❌ Utiliser command/shell quand un module existe
- name: Create directory
  command: mkdir -p /opt/app

# ❌ Ignorer les erreurs sans raison
- name: Some task
  command: risky_command
  ignore_errors: true

# ❌ Tasks non idempotentes
- name: Configure system
  shell: echo "config=value" >> /etc/app.conf
```

### ✅ Bonnes Pratiques

```yaml
# ✅ Variables préfixées
example_port: 8080
example_user: admin

# ✅ Utiliser des variables
- name: Configure service
  lineinfile:
    path: /etc/app/config
    regexp: "^port="
    line: "port={{ example_port }}"

# ✅ Utiliser le module approprié
- name: Create directory
  file:
    path: /opt/app
    state: directory
    mode: '0755'

# ✅ Gérer les erreurs explicitement
- name: Some task
  command: risky_command
  register: result
  failed_when: result.rc not in [0, 2]

# ✅ Tasks idempotentes
- name: Configure system
  lineinfile:
    path: /etc/app.conf
    regexp: "^config="
    line: "config=value"
```

## Idempotence et Testabilité - OBLIGATOIRE

### Exigences d'Idempotence

**CHAQUE rôle DOIT être idempotent :**

```yaml
# ✅ BON - Idempotent
- name: Ensure configuration file is present
  template:
    src: config.j2
    dest: /etc/app/config.yml
  register: config_result

# ❌ INTERDIT - Non idempotent
- name: Add configuration
  shell: echo "setting=value" >> /etc/app/config.yml
```

### Support Dry-Run (Check Mode)

**CHAQUE rôle DOIT supporter `--check --diff` :**

```yaml
# Tasks en check mode
- name: Validate configuration before applying
  command: nginx -t
  changed_when: false
  check_mode: false  # Toujours exécuter pour valider

- name: Deploy configuration (check mode compatible)
  template:
    src: config.j2
    dest: /etc/app/config.yml
  # Automatiquement compatible avec --check
```

### Tests OBLIGATOIRES

**Avant de considérer un rôle terminé :**

```bash
# 1. Test syntaxique
ansible-playbook tests/test.yml --syntax-check

# 2. Dry-run
ansible-playbook tests/test.yml --check --diff

# 3. Première exécution
ansible-playbook tests/test.yml

# 4. Test d'idempotence (DOIT retourner 0 changes)
ansible-playbook tests/test.yml

# 5. Tests Molecule
molecule test
```

## Checklist de Revue de Rôle

Avant de considérer un rôle comme terminé :

- [ ] Structure complète avec tous les répertoires nécessaires
- [ ] Variables toutes préfixées avec le nom du rôle
- [ ] README.md complet avec exemples
- [ ] meta/main.yml avec métadonnées correctes
- [ ] Tous les secrets utilisent Ansible Vault
- [ ] **✅ Tasks 100% idempotentes - TEST PASSÉ**
- [ ] **✅ Compatible --check --diff - TEST PASSÉ**
- [ ] **✅ Test idempotence (2ème run = 0 changes) - PASSÉ**
- [ ] Handlers définis pour les redémarrages/rechargements
- [ ] Templates avec header `{{ ansible_managed }}`
- [ ] Support multi-OS (si applicable)
- [ ] Tags définis sur les tasks importantes
- [ ] **✅ Tests Molecule créés et TOUS passants**
- [ ] **✅ Code validé avec `ansible-lint` - 0 erreurs**
- [ ] **✅ Testé en environnement de test avant production**
