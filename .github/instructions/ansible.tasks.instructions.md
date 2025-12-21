---
description: Writing idempotent, robust tasks with proper error handling and conditions
name: Ansible_Tasks_Best_Practices
applyTo: "**/{tasks,handlers}/**/*.{yml,yaml}"
---

# Ansible Tasks - Guide Expert

## 🎯 Actions Obligatoires (Mandatory)

**À TOUJOURS respecter lors de l'écriture de tasks :**

1. ✅ **Nom descriptif** : Chaque task DOIT avoir un `name` clair décrivant l'état souhaité
   ```yaml
   # ✅ Bon - décrit l'état
   - name: Ensure nginx is installed and running
   
   # ❌ Mauvais - décrit l'action
   - name: Install nginx
   ```

2. ✅ **Idempotence ABSOLUE** : La task DOIT être réexécutable 1000 fois avec le MÊME résultat
3. ✅ **Modules natifs OBLIGATOIRES** : JAMAIS shell/command si un module existe
4. ✅ **Gestion d'erreurs** : Utiliser `block/rescue/always` pour les opérations critiques
5. ✅ **Check mode OBLIGATOIRE** : TOUTES les tasks doivent supporter `--check`
6. ✅ **Test d'idempotence** : Vérifier que changed_when est correctement défini
7. ✅ **Pas de secrets** : Utiliser `no_log: true` pour les données sensibles
8. ✅ **Testabilité** : Chaque task doit être testable individuellement

## Idempotence

### Principes de l'Idempotence

Une task idempotente peut être exécutée plusieurs fois sans changer le résultat après la première exécution.

```yaml
---
# ✅ IDEMPOTENT - Utilise des modules natifs
- name: Ensure application user exists
  user:
    name: appuser
    state: present
    system: true

- name: Ensure configuration file is present
  template:
    src: config.j2
    dest: /etc/app/config.yml
  notify: Restart application

# ❌ NON IDEMPOTENT - Ajoute à chaque exécution
- name: Add configuration line
  shell: echo "setting=value" >> /etc/app/config

# ✅ VERSION IDEMPOTENTE
- name: Ensure configuration line is present
  lineinfile:
    path: /etc/app/config
    regexp: "^setting="
    line: "setting=value"
```

### Contrôler le Statut Changed

```yaml
---
- name: Check application status (read-only)
  command: /opt/app/bin/status
  register: app_status
  changed_when: false  # Ne jamais marquer comme changé

- name: Validate configuration
  command: nginx -t
  changed_when: false
  check_mode: false  # Toujours exécuter même en --check

- name: Deploy configuration
  template:
    src: app.conf.j2
    dest: /etc/app/app.conf
  register: config_result
  changed_when: config_result.changed and 'specific_pattern' in config_result.diff

- name: Restart service only if config changed
  systemd:
    name: application
    state: restarted
  when: config_result.changed
```

### Failed_when Personnalisé

```yaml
---
- name: Run database migration
  command: /opt/app/bin/migrate.sh
  register: migration_result
  failed_when:
    - migration_result.rc != 0
    - "'already applied' not in migration_result.stderr"
  changed_when: "'migrations applied' in migration_result.stdout"

- name: Check disk space
  shell: df -h / | tail -1 | awk '{print $5}' | sed 's/%//'
  register: disk_usage
  changed_when: false
  failed_when: disk_usage.stdout | int > 90

- name: Execute script with acceptable return codes
  command: /opt/scripts/deploy.sh
  register: deploy_result
  failed_when: deploy_result.rc not in [0, 2, 10]  # 0=success, 2=already deployed, 10=partial
```

## Modules Natifs vs Command/Shell

### Toujours Privilégier les Modules

```yaml
---
# ❌ MAUVAIS - Utilise shell
- name: Create directory
  shell: mkdir -p /opt/app

# ✅ BON - Module natif
- name: Ensure directory exists
  file:
    path: /opt/app
    state: directory
    owner: appuser
    group: appuser
    mode: '0755'

# ❌ MAUVAIS - Utilise command
- name: Install package
  command: apt-get install -y nginx

# ✅ BON - Module natif
- name: Ensure nginx is installed
  apt:
    name: nginx
    state: present
    update_cache: true

# ❌ MAUVAIS - Utilise shell
- name: Copy file
  shell: cp /src/file /dest/file

# ✅ BON - Module natif
- name: Ensure file is copied
  copy:
    src: /src/file
    dest: /dest/file
    owner: root
    group: root
    mode: '0644'

# ❌ MAUVAIS - Utilise curl en shell
- name: Download file
  shell: curl -o /tmp/file.tar.gz https://example.com/file.tar.gz

# ✅ BON - Module natif
- name: Ensure file is downloaded
  get_url:
    url: https://example.com/file.tar.gz
    dest: /tmp/file.tar.gz
    checksum: sha256:abc123...
```

### Quand Utiliser Command/Shell

Utiliser `command` ou `shell` **uniquement** quand aucun module n'existe.

```yaml
---
# Cas légitime : commande applicative spécifique
- name: Initialize application database
  command: /opt/app/bin/init-db --force
  args:
    creates: /var/lib/app/db.initialized  # Idempotence
  register: init_result
  changed_when: "'Database initialized' in init_result.stdout"

# Cas légitime : script propriétaire
- name: Run custom deployment script
  shell: |
    set -e
    cd /opt/app
    ./deploy.sh --env production
  args:
    executable: /bin/bash
    creates: /opt/app/deployed.marker
  environment:
    APP_ENV: production
```

## Gestion d'Erreurs Avancée

### Block/Rescue/Always

```yaml
---
- name: Deploy application with rollback capability
  block:
    # Tentative de déploiement
    - name: Stop application service
      systemd:
        name: application
        state: stopped
    
    - name: Backup current version
      archive:
        path: /opt/app
        dest: "/backup/app-{{ ansible_date_time.epoch }}.tar.gz"
    
    - name: Deploy new version
      unarchive:
        src: "artifacts/app-{{ new_version }}.tar.gz"
        dest: /opt/app
        owner: appuser
        group: appuser
    
    - name: Run database migrations
      command: /opt/app/bin/migrate.sh
      register: migration_result
      failed_when: migration_result.rc not in [0, 2]
    
    - name: Start application service
      systemd:
        name: application
        state: started
    
    - name: Wait for application health check
      uri:
        url: http://localhost:8080/health
        status_code: 200
        timeout: 5
      retries: 30
      delay: 2
      register: health_check
  
  rescue:
    # En cas d'erreur, rollback
    - name: Log deployment failure
      lineinfile:
        path: /var/log/deployments.log
        line: "{{ ansible_date_time.iso8601 }} | FAILED | {{ new_version }} | {{ ansible_failed_result.msg }}"
    
    - name: Stop failed application
      systemd:
        name: application
        state: stopped
      ignore_errors: true
    
    - name: Restore backup
      unarchive:
        src: "/backup/app-{{ ansible_date_time.epoch }}.tar.gz"
        dest: /opt
        remote_src: true
    
    - name: Start application with old version
      systemd:
        name: application
        state: started
    
    - name: Send alert
      uri:
        url: "{{ alert_webhook }}"
        method: POST
        body_format: json
        body:
          severity: critical
          message: "Deployment failed and rolled back"
          version: "{{ new_version }}"
      when: alert_webhook is defined
    
    - name: Fail the playbook after rollback
      fail:
        msg: "Deployment failed. System rolled back to previous version."
  
  always:
    # Toujours exécuté, succès ou échec
    - name: Collect application logs
      fetch:
        src: /var/log/application.log
        dest: "logs/{{ inventory_hostname }}-{{ ansible_date_time.epoch }}.log"
        flat: true
    
    - name: Clean temporary files
      file:
        path: /tmp/deployment-*
        state: absent
```

### Retry et Until

```yaml
---
- name: Wait for service to be ready
  uri:
    url: http://localhost:8080/health
    status_code: 200
    timeout: 5
  register: health_check
  retries: 30
  delay: 2
  until: health_check.status == 200

- name: Wait for database to be ready
  postgresql_ping:
    db: postgres
  register: db_check
  retries: 10
  delay: 5
  until: db_check is success

- name: Wait for file to appear
  stat:
    path: /opt/app/ready.marker
  register: marker_file
  retries: 60
  delay: 1
  until: marker_file.stat.exists

- name: Poll external API until ready
  uri:
    url: "https://api.example.com/status"
    return_content: true
  register: api_status
  retries: 20
  delay: 10
  until: 
    - api_status.status == 200
    - "'ready' in api_status.content"
```

## Conditions et Logique

### When Conditions

```yaml
---
# Condition simple
- name: Install package on Debian/Ubuntu only
  apt:
    name: nginx
    state: present
  when: ansible_os_family == 'Debian'

# Conditions multiples (ET logique)
- name: Deploy to production web servers only
  copy:
    src: app.jar
    dest: /opt/app/
  when:
    - environment == 'production'
    - inventory_hostname in groups['webservers']
    - app_version is version('2.0.0', '>=')

# Conditions multiples (OU logique)
- name: Install on Red Hat or CentOS
  yum:
    name: nginx
    state: present
  when: ansible_distribution == 'RedHat' or ansible_distribution == 'CentOS'

# Condition sur variable définie
- name: Configure SSL if certificate is defined
  template:
    src: ssl-config.j2
    dest: /etc/nginx/ssl.conf
  when:
    - ssl_enabled | default(false)
    - ssl_certificate is defined
    - ssl_certificate | length > 0

# Condition sur résultat de task précédente
- name: Check if config changed
  template:
    src: app.conf.j2
    dest: /etc/app/app.conf
  register: config_result

- name: Restart only if config changed
  systemd:
    name: application
    state: restarted
  when: config_result.changed

# Condition complexe avec filtres
- name: Deploy to hosts with enough disk space
  copy:
    src: large-file.tar.gz
    dest: /opt/
  when: ansible_mounts | selectattr('mount', 'equalto', '/opt') | map(attribute='size_available') | first | int > 10000000000
```

### Assertions

```yaml
---
- name: Validate deployment prerequisites
  assert:
    that:
      - deployment_version is defined
      - deployment_version is version('1.0.0', '>=')
      - environment in ['dev', 'staging', 'production']
      - ansible_memtotal_mb >= 2048
      - ansible_processor_vcpus >= 2
    fail_msg: |
      Deployment prerequisites not met:
      - Version: {{ deployment_version | default('undefined') }}
      - Environment: {{ environment | default('undefined') }}
      - Memory: {{ ansible_memtotal_mb }}MB (minimum 2048MB)
      - CPUs: {{ ansible_processor_vcpus }} (minimum 2)
    success_msg: "All deployment prerequisites validated"
    quiet: false

- name: Validate production deployment
  assert:
    that:
      - production_approved | default(false) | bool
      - backup_completed | default(false) | bool
    fail_msg: "Production deployment requires explicit approval and completed backup"
  when: environment == 'production'
```

## Loops et Itérations

### Loop Basique

```yaml
---
# Loop simple
- name: Install multiple packages
  apt:
    name: "{{ item }}"
    state: present
  loop:
    - nginx
    - curl
    - git
    - vim

# Loop avec dictionnaires
- name: Create multiple users
  user:
    name: "{{ item.name }}"
    groups: "{{ item.groups }}"
    shell: "{{ item.shell }}"
    state: present
  loop:
    - { name: 'alice', groups: 'developers', shell: '/bin/bash' }
    - { name: 'bob', groups: 'ops', shell: '/bin/bash' }
    - { name: 'charlie', groups: 'developers,ops', shell: '/bin/zsh' }

# Loop sur variables
- name: Deploy configuration files
  template:
    src: "{{ item }}.j2"
    dest: "/etc/app/{{ item }}"
  loop: "{{ config_files }}"
  vars:
    config_files:
      - app.conf
      - database.conf
      - logging.conf
```

### Loops Avancés

```yaml
---
# Loop avec index
- name: Create numbered directories
  file:
    path: "/opt/data/shard-{{ item }}"
    state: directory
  loop: "{{ range(0, 10) | list }}"

# Loop sur résultat de commande
- name: Get list of log files
  find:
    paths: /var/log
    patterns: "*.log"
  register: log_files

- name: Archive old log files
  archive:
    path: "{{ item.path }}"
    dest: "/backup/{{ item.path | basename }}.gz"
  loop: "{{ log_files.files }}"
  when: item.mtime < (ansible_date_time.epoch | int - 86400)

# Loop avec condition
- name: Install packages conditionally
  apt:
    name: "{{ item.name }}"
    state: present
  loop:
    - { name: 'nginx', when: true }
    - { name: 'apache2', when: false }
    - { name: 'haproxy', when: "{{ use_haproxy | default(false) }}" }
  when: item.when | bool

# Loop avec gestion d'erreurs
- name: Try multiple mirrors
  get_url:
    url: "{{ item }}/package.tar.gz"
    dest: /tmp/package.tar.gz
  loop:
    - https://mirror1.example.com
    - https://mirror2.example.com
    - https://mirror3.example.com
  register: download_result
  until: download_result is success
  retries: 3
  delay: 5
```

### Loop Control

```yaml
---
# Contrôle du loop
- name: Deploy configurations with progress
  template:
    src: "{{ item }}.j2"
    dest: "/etc/app/{{ item }}"
  loop:
    - config1.conf
    - config2.conf
    - config3.conf
  loop_control:
    label: "{{ item }}"  # Afficher uniquement le nom, pas tout le contenu
    pause: 2  # Pause de 2s entre chaque itération
    index_var: config_index  # Variable d'index accessible

- name: Parallel execution with loop
  command: /opt/scripts/process.sh {{ item }}
  loop: "{{ large_list }}"
  loop_control:
    pause: 0
  async: 300  # Timeout de 5 minutes
  poll: 0  # Exécution en arrière-plan
  register: async_results

- name: Wait for all background tasks
  async_status:
    jid: "{{ item.ansible_job_id }}"
  loop: "{{ async_results.results }}"
  register: job_results
  until: job_results.finished
  retries: 30
  delay: 10
```

## Check Mode et Diff - OBLIGATOIRE

### Support du Check Mode (DRY-RUN)

**RÈGLE D'OR : TOUTES les tasks doivent supporter `--check` correctement**

```yaml
---
# ✅ Task normale - supporte --check automatiquement
- name: Ensure package is installed
  apt:
    name: nginx
    state: present
  # Fonctionne en --check : simule l'installation

# ✅ Task toujours exécutée même en --check (validations)
- name: Validate configuration before applying
  command: nginx -t
  changed_when: false
  check_mode: false  # OBLIGATOIRE pour validations

# ✅ Task de lecture - toujours exécuter en check
- name: Check current version
  command: /opt/app/bin/version
  register: current_version
  changed_when: false
  check_mode: false  # Les lectures doivent toujours s'exécuter

# ⚠️ Task dangereuse - skip en check mode
- name: Dangerous operation (backup first!)
  command: rm -rf /data/old/*
  when: not ansible_check_mode  # Skip en mode check

# ✅ Alternative sécurisée
- name: Remove old data safely
  file:
    path: /data/old
    state: absent
  # Module file supporte --check nativement

# ✅ Diff pour templates (TOUJOURS activer)
- name: Deploy configuration with diff
  template:
    src: config.j2
    dest: /etc/app/config.yml
    mode: '0644'
  diff: true  # OBLIGATOIRE - voir les changements
```

### Tester le Support Check Mode

```bash
# Test complet du check mode
#!/bin/bash
set -e

PLAYBOOK="$1"
INVENTORY="$2"

echo "=== Testing Check Mode Support ==="

# 1. Syntax check
echo "1. Syntax validation..."
ansible-playbook "$PLAYBOOK" --syntax-check

# 2. Check mode (dry-run)
echo "2. Check mode (dry-run)..."
ansible-playbook -i "$INVENTORY" "$PLAYBOOK" --check --diff | tee /tmp/check_run.log

if grep -q "fatal" /tmp/check_run.log; then
  echo "❌ FAILED: Check mode encountered errors"
  exit 1
fi

# 3. Première exécution réelle
echo "3. First real run..."
ansible-playbook -i "$INVENTORY" "$PLAYBOOK" | tee /tmp/first_run.log

# 4. Test d'idempotence (CRITIQUE)
echo "4. Idempotence test..."
ansible-playbook -i "$INVENTORY" "$PLAYBOOK" | tee /tmp/second_run.log

if grep -q "changed=0" /tmp/second_run.log; then
  echo "✅ PASSED: Idempotence test successful"
else
  echo "❌ FAILED: Changes detected on second run (not idempotent!)"
  exit 1
fi

echo "✅ All tests passed!"
```

### Gestion des Tasks Non-Idempotentes

```yaml
---
# ❌ INTERDIT - Command non idempotent
- name: Initialize database
  command: /opt/app/bin/init-db.sh

# ✅ CORRECT - Idempotent avec creates
- name: Initialize database (idempotent)
  command: /opt/app/bin/init-db.sh
  args:
    creates: /var/lib/app/db.initialized
  register: init_result
  changed_when: "'Database initialized' in init_result.stdout"

# ✅ CORRECT - Idempotent avec stat + when
- name: Check if database is initialized
  stat:
    path: /var/lib/app/db.initialized
  register: db_state
  check_mode: false

- name: Initialize database if needed
  command: /opt/app/bin/init-db.sh
  when: not db_state.stat.exists
  register: init_result

- name: Create initialization marker
  file:
    path: /var/lib/app/db.initialized
    state: touch
  when: init_result is changed
```

## Délégation et Exécution Locale

### Delegate_to

```yaml
---
# Exécuter sur un autre hôte
- name: Update load balancer
  haproxy:
    state: disabled
    host: "{{ inventory_hostname }}"
    backend: web_backend
  delegate_to: "{{ item }}"
  loop: "{{ groups['loadbalancers'] }}"

# Exécuter localement
- name: Generate configuration locally
  template:
    src: config.j2
    dest: "/tmp/{{ inventory_hostname }}-config.yml"
  delegate_to: localhost

# Run once pour tout le groupe
- name: Initialize shared resource
  command: /opt/scripts/initialize.sh
  run_once: true
  delegate_to: "{{ groups['dbservers'][0] }}"
```

### Local Action

```yaml
---
# Raccourci pour delegate_to: localhost
- name: Calculate configuration
  local_action:
    module: template
    src: config.j2
    dest: "/tmp/{{ inventory_hostname }}-config.yml"

# Alternative plus lisible
- name: Calculate configuration
  template:
    src: config.j2
    dest: "/tmp/{{ inventory_hostname }}-config.yml"
  delegate_to: localhost
```

## Tags et Organisation

### Utilisation des Tags

```yaml
---
- name: Install application packages
  apt:
    name: "{{ app_packages }}"
    state: present
  tags:
    - install
    - packages
    - never  # Jamais exécuté sauf si explicitement demandé

- name: Configure application
  template:
    src: app.conf.j2
    dest: /etc/app/app.conf
  tags:
    - configure
    - config

- name: Start application service
  systemd:
    name: application
    state: started
  tags:
    - service
    - startup

- name: Run database migrations
  command: /opt/app/bin/migrate.sh
  tags:
    - deploy
    - database
    - migrations

- name: Dangerous cleanup operation
  file:
    path: /tmp/old-data
    state: absent
  tags:
    - cleanup
    - never  # Nécessite --tags cleanup pour être exécuté
```

**Exécution avec tags :**
```bash
# Exécuter uniquement les tasks de configuration
ansible-playbook playbook.yml --tags configure

# Exécuter install et configure
ansible-playbook playbook.yml --tags "install,configure"

# Tout sauf les services
ansible-playbook playbook.yml --skip-tags service

# Forcer l'exécution des tags 'never'
ansible-playbook playbook.yml --tags cleanup
```

## Anti-Patterns à Éviter

```yaml
---
# ❌ Pas de nom
- command: /some/command

# ❌ Nom non descriptif
- name: Task 1
  command: /some/command

# ❌ Non idempotent
- name: Add line to file
  shell: echo "line" >> /etc/config

# ❌ Utilise shell inutilement
- name: Create directory
  shell: mkdir /opt/app

# ❌ Ignore toutes les erreurs
- name: Risky operation
  command: /risky/command
  ignore_errors: true

# ❌ Secrets visibles dans les logs
- name: Set password
  command: /set-password MySecretPassword123

# ✅ BONNES PRATIQUES
- name: Ensure configuration line is present in file
  lineinfile:
    path: /etc/config
    regexp: "^line="
    line: "line=value"

- name: Ensure application directory exists
  file:
    path: /opt/app
    state: directory
    mode: '0755'

- name: Attempt operation with specific error handling
  command: /risky/command
  register: result
  failed_when: result.rc not in [0, 2]

- name: Set password securely
  command: /set-password {{ app_password }}
  no_log: true
```

## Checklist Tasks - TESTABILITÉ OBLIGATOIRE

### Avant de Committer

- [ ] Chaque task a un `name` descriptif
- [ ] **✅ Tasks 100% idempotentes - TESTÉ**
- [ ] **✅ Test idempotence passé (2ème run = changed: false)**
- [ ] Modules natifs utilisés plutôt que shell/command
- [ ] Gestion d'erreurs avec block/rescue pour opérations critiques
- [ ] `changed_when` et `failed_when` définis si nécessaire
- [ ] `no_log: true` pour tasks manipulant des secrets
- [ ] **✅ Support check mode testé et fonctionnel**
- [ ] **✅ `check_mode: false` uniquement pour validations/lectures**
- [ ] **✅ Diff activé (`diff: true`) pour voir les changements**
- [ ] Tags appropriés définis
- [ ] Conditions `when` utilisées correctement
- [ ] Retry/until pour opérations asynchrones
- [ ] Pas de hardcoding de valeurs
- [ ] **✅ Testé en mode `--check --diff` - PASSÉ**
- [ ] **✅ Testé 2 fois de suite - PASSÉ (0 changes 2ème fois)**
- [ ] **✅ Task testable individuellement avec tags**

### Script de Test Automatique

```bash
#!/bin/bash
# test-task-idempotence.sh
# Tester l'idempotence d'une task spécifique

PLAYBOOK="$1"
TAG="$2"
INVENTORY="${3:-localhost,}"

if [ -z "$PLAYBOOK" ] || [ -z "$TAG" ]; then
  echo "Usage: $0 <playbook.yml> <tag> [inventory]"
  exit 1
fi

echo "=== Testing task idempotence for tag: $TAG ==="

# Première exécution
echo "First run..."
ansible-playbook -i "$INVENTORY" "$PLAYBOOK" --tags "$TAG" | tee /tmp/first.log

# Deuxième exécution (doit être idempotente)
echo "Second run (checking idempotence)..."
ansible-playbook -i "$INVENTORY" "$PLAYBOOK" --tags "$TAG" | tee /tmp/second.log

# Vérification
if grep "changed=0" /tmp/second.log; then
  echo "✅ Task '$TAG' is IDEMPOTENT"
  exit 0
else
  echo "❌ Task '$TAG' is NOT IDEMPOTENT - Changes on second run!"
  diff /tmp/first.log /tmp/second.log
  exit 1
fi
```
