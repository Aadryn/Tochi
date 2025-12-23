---
description: Playbook structure and best practices for orchestration
name: Ansible_Playbooks_Structure
applyTo: "**/ansible/playbooks/**/*.yml,**/ansible/playbooks/**/*.yaml"
---

# Ansible Playbooks - Guide Expert

## ⛔ À NE PAS FAIRE

- **N'omets jamais** les sections pre_tasks/post_tasks pour les validations
- **Ne laisse jamais** become/gather_facts implicites (sois explicite)
- **Ne crée jamais** de playbook sans nom descriptif
- **N'oublie jamais** de taguer les sections pour l'exécution sélective
- **Ne mélange jamais** orchestration et logique métier dans le même playbook
- **N'exécute jamais** un playbook sans dry-run préalable (`--check --diff`)

## ✅ À FAIRE

- **Structure toujours** avec pre_tasks → roles → tasks → post_tasks
- **Spécifie toujours** explicitement become: true/false et gather_facts: true/false
- **Nomme toujours** chaque play et task de façon descriptive
- **Utilise toujours** des tags pour permettre l'exécution partielle
- **Valide toujours** les prérequis dans pre_tasks avec assert
- **Vérifie toujours** le résultat dans post_tasks
- **Importe toujours** les playbooks avec import_playbook pour la modularité

## 🎯 Actions Obligatoires (Mandatory)

**À TOUJOURS faire lors de la création/modification d'un playbook :**

1. ✅ **Structure complète** : Inclure les sections essentielles
   ```yaml
   - name: Description claire du playbook
     hosts: groupe_cible
     become: true/false          # Explicite
     gather_facts: true/false    # Explicite
     
     pre_tasks:    # Validations et préparations
     roles:        # Rôles à appliquer
     tasks:        # Tasks spécifiques
     post_tasks:   # Vérifications finales
     handlers:     # Gestionnaires d'événements
   ```

2. ✅ **Nom descriptif** : Le nom doit expliquer clairement l'objectif
   - ✅ Bon : `deploy_web_application.yml`, `update_security_patches.yml`
   - ❌ Mauvais : `playbook.yml`, `main.yml`, `test.yml`

3. ✅ **Validation pré-exécution** : Toujours vérifier les prérequis dans `pre_tasks`
4. ✅ **Vérification post-exécution** : Valider le résultat dans `post_tasks`
5. ✅ **Tags** : Définir des tags pour l'exécution sélective
6. ✅ **Idempotence ABSOLUE** : Le playbook DOIT être réexécutable infiniment sans effets de bord
7. ✅ **Dry-run OBLIGATOIRE** : Toujours tester avec `--check --diff` avant exécution réelle
8. ✅ **Test d'idempotence** : 2ème exécution DOIT retourner `changed=0` pour toutes les tasks
9. ✅ **Rollback plan** : Toujours prévoir une stratégie de rollback en cas d'échec

## Structure Complète d'un Playbook

### Template Standard

```yaml
---
- name: Deploy complete web application stack
  hosts: webservers
  become: true
  gather_facts: true
  
  # Variables spécifiques au playbook
  vars:
    deployment_timestamp: "{{ ansible_date_time.iso8601 }}"
    deployment_user: "{{ ansible_user_id }}"
  
  # Chargement de variables externes
  vars_files:
    - vars/common.yml
    - vars/{{ environment }}.yml
  
  # ==========================================================================
  # PRE_TASKS - Validations et préparations avant les rôles
  # ==========================================================================
  pre_tasks:
    - name: Display deployment information
      debug:
        msg: |
          Deploying to: {{ inventory_hostname }}
          Environment: {{ environment }}
          User: {{ deployment_user }}
          Timestamp: {{ deployment_timestamp }}
      tags: always
    
    - name: Verify system requirements
      assert:
        that:
          - ansible_distribution in ['Ubuntu', 'Debian', 'CentOS', 'RedHat']
          - ansible_distribution_major_version | int >= 8
          - ansible_memtotal_mb >= 2048
          - ansible_processor_vcpus >= 2
        fail_msg: "System does not meet minimum requirements"
        success_msg: "System requirements validated"
      tags: preflight
    
    - name: Check connectivity to external services
      uri:
        url: "{{ item }}"
        timeout: 5
      loop:
        - "https://{{ artifact_repository }}"
        - "https://{{ monitoring_endpoint }}"
      tags: preflight
    
    - name: Update package cache (Debian/Ubuntu)
      apt:
        update_cache: true
        cache_valid_time: 3600
      when: ansible_os_family == 'Debian'
      tags: packages
    
    - name: Create backup before deployment
      archive:
        path: /opt/app
        dest: "/backup/app-{{ deployment_timestamp }}.tar.gz"
      when: app_backup_enabled | default(true)
      tags: backup
  
  # ==========================================================================
  # ROLES - Rôles principaux de l'infrastructure
  # ==========================================================================
  roles:
    - role: common
      tags: common
      vars:
        common_timezone: "Europe/Paris"
    
    - role: firewall
      tags: firewall
      vars:
        firewall_allowed_ports:
          - 22
          - 80
          - 443
    
    - role: nginx
      tags: nginx
      vars:
        nginx_worker_processes: "{{ ansible_processor_vcpus }}"
        nginx_worker_connections: 2048
    
    - role: application
      tags: application
      vars:
        app_version: "{{ deployment_version }}"
  
  # ==========================================================================
  # TASKS - Tasks spécifiques au playbook
  # ==========================================================================
  tasks:
    - name: Deploy custom configuration
      template:
        src: templates/app-config.j2
        dest: /etc/app/config.yml
        mode: '0644'
      notify: Restart application
      tags: configure
    
    - name: Create symbolic link to current version
      file:
        src: "/opt/app/versions/{{ deployment_version }}"
        dest: /opt/app/current
        state: link
      tags: deploy
  
  # ==========================================================================
  # POST_TASKS - Vérifications et notifications après déploiement
  # ==========================================================================
  post_tasks:
    - name: Wait for application to be ready
      uri:
        url: "http://{{ ansible_default_ipv4.address }}:8080/health"
        status_code: 200
        timeout: 5
      retries: 30
      delay: 2
      register: health_check
      tags: verify
    
    - name: Verify application version
      uri:
        url: "http://{{ ansible_default_ipv4.address }}:8080/version"
        return_content: true
      register: version_check
      failed_when: deployment_version not in version_check.content
      tags: verify
    
    - name: Run smoke tests
      command: /opt/app/bin/smoke-tests.sh
      changed_when: false
      tags: verify
    
    - name: Record deployment in log
      lineinfile:
        path: /var/log/deployments.log
        line: "{{ deployment_timestamp }} | {{ deployment_user }} | {{ deployment_version }} | SUCCESS"
        create: true
      tags: logging
    
    - name: Send deployment notification
      uri:
        url: "{{ notification_webhook_url }}"
        method: POST
        body_format: json
        body:
          status: "success"
          environment: "{{ environment }}"
          host: "{{ inventory_hostname }}"
          version: "{{ deployment_version }}"
          timestamp: "{{ deployment_timestamp }}"
      when: notification_webhook_url is defined
      tags: notification
  
  # ==========================================================================
  # HANDLERS - Gestionnaires pour les redémarrages/rechargements
  # ==========================================================================
  handlers:
    - name: Restart application
      systemd:
        name: application
        state: restarted
    
    - name: Reload nginx
      systemd:
        name: nginx
        state: reloaded
```

## Patterns de Playbooks Avancés

### Déploiement Progressif (Rolling Update)

```yaml
---
- name: Rolling update of web servers
  hosts: webservers
  serial: "25%"  # Déployer sur 25% des serveurs à la fois
  max_fail_percentage: 25  # Arrêter si plus de 25% échouent
  
  pre_tasks:
    - name: Remove server from load balancer
      haproxy:
        state: disabled
        host: "{{ inventory_hostname }}"
        backend: web_backend
      delegate_to: "{{ item }}"
      loop: "{{ groups['loadbalancers'] }}"
  
  roles:
    - application
  
  post_tasks:
    - name: Wait for application to be healthy
      uri:
        url: "http://localhost:8080/health"
        status_code: 200
      retries: 30
      delay: 2
    
    - name: Add server back to load balancer
      haproxy:
        state: enabled
        host: "{{ inventory_hostname }}"
        backend: web_backend
      delegate_to: "{{ item }}"
      loop: "{{ groups['loadbalancers'] }}"
    
    - name: Wait for load balancer to route traffic
      pause:
        seconds: 10
```

### Déploiement Blue-Green

```yaml
---
- name: Blue-Green deployment
  hosts: localhost
  gather_facts: false
  
  vars:
    current_color: "{{ lookup('file', '/tmp/current_color') | default('blue') }}"
    target_color: "{{ 'green' if current_color == 'blue' else 'blue' }}"
  
  tasks:
    - name: Display deployment strategy
      debug:
        msg: "Deploying to {{ target_color }} environment (current: {{ current_color }})"
    
    - name: Deploy to target environment
      include_tasks: deploy_environment.yml
      vars:
        environment_color: "{{ target_color }}"
    
    - name: Run smoke tests on target environment
      uri:
        url: "http://{{ target_color }}.example.com/health"
        status_code: 200
      retries: 10
      delay: 5
    
    - name: Switch load balancer to target environment
      template:
        src: loadbalancer-{{ target_color }}.conf.j2
        dest: /etc/nginx/conf.d/upstream.conf
      delegate_to: "{{ item }}"
      loop: "{{ groups['loadbalancers'] }}"
      notify: Reload load balancer
    
    - name: Update current color marker
      copy:
        content: "{{ target_color }}"
        dest: /tmp/current_color
    
    - name: Keep old environment for rollback (optional)
      debug:
        msg: "Old {{ current_color }} environment still available for rollback"
  
  handlers:
    - name: Reload load balancer
      systemd:
        name: nginx
        state: reloaded
      delegate_to: "{{ item }}"
      loop: "{{ groups['loadbalancers'] }}"
```

### Déploiement avec Rollback Automatique

```yaml
---
- name: Deploy with automatic rollback on failure
  hosts: appservers
  
  vars:
    backup_dir: "/backup/app-{{ ansible_date_time.epoch }}"
    deployment_failed: false
  
  tasks:
    - name: Deployment with rollback capability
      block:
        - name: Create backup of current version
          archive:
            path: /opt/app
            dest: "{{ backup_dir }}.tar.gz"
        
        - name: Stop application service
          systemd:
            name: application
            state: stopped
        
        - name: Deploy new version
          unarchive:
            src: "artifacts/app-{{ new_version }}.tar.gz"
            dest: /opt/app
            owner: appuser
            group: appuser
        
        - name: Start application service
          systemd:
            name: application
            state: started
        
        - name: Wait for application health check
          uri:
            url: "http://localhost:8080/health"
            status_code: 200
          retries: 30
          delay: 2
        
        - name: Run integration tests
          command: /opt/app/tests/integration_tests.sh
          changed_when: false
      
      rescue:
        - name: Mark deployment as failed
          set_fact:
            deployment_failed: true
        
        - name: Stop failed application
          systemd:
            name: application
            state: stopped
          ignore_errors: true
        
        - name: Rollback to previous version
          unarchive:
            src: "{{ backup_dir }}.tar.gz"
            dest: /opt
            remote_src: true
        
        - name: Start application with old version
          systemd:
            name: application
            state: started
        
        - name: Verify rollback successful
          uri:
            url: "http://localhost:8080/health"
            status_code: 200
          retries: 10
          delay: 2
        
        - name: Log rollback event
          lineinfile:
            path: /var/log/deployments.log
            line: "{{ ansible_date_time.iso8601 }} | ROLLBACK | {{ new_version }} | FAILED"
        
        - name: Fail the playbook after rollback
          fail:
            msg: "Deployment failed, rolled back to previous version"
      
      always:
        - name: Clean temporary files
          file:
            path: "/tmp/deployment-*"
            state: absent
```

### Orchestration Multi-Environnement

```yaml
---
- name: Multi-tier application orchestration
  hosts: all
  gather_facts: false
  
  tasks:
    # Phase 1: Base de données
    - name: Update database servers
      include_tasks: deploy_database.yml
      when: "'dbservers' in group_names"
      tags: database
    
    # Phase 2: Application tier
    - name: Update application servers
      include_tasks: deploy_application.yml
      when: "'appservers' in group_names"
      tags: application
    
    # Phase 3: Web tier
    - name: Update web servers
      include_tasks: deploy_web.yml
      when: "'webservers' in group_names"
      tags: web

---
# Playbook avec orchestration séquentielle par groupe
- name: Phase 1 - Database tier
  hosts: dbservers
  serial: 1  # Un par un pour la base de données
  roles:
    - postgresql
  tags: database

- name: Phase 2 - Application tier
  hosts: appservers
  serial: "50%"
  roles:
    - application
  tags: application

- name: Phase 3 - Web tier
  hosts: webservers
  serial: "33%"
  roles:
    - nginx
  tags: web

- name: Phase 4 - Verify entire stack
  hosts: localhost
  gather_facts: false
  tasks:
    - name: Run end-to-end tests
      command: /opt/tests/e2e_tests.sh
      changed_when: false
  tags: verify
```

## Gestion des Erreurs et Conditions

### Block/Rescue/Always

```yaml
---
- name: Deploy with comprehensive error handling
  hosts: appservers
  
  tasks:
    - name: Complex deployment with error handling
      block:
        - name: Pre-deployment validation
          command: /opt/scripts/validate_environment.sh
          changed_when: false
        
        - name: Deploy application
          copy:
            src: "app-{{ version }}.jar"
            dest: /opt/app/current.jar
        
        - name: Migrate database
          command: /opt/app/bin/migrate.sh
          register: migration_result
        
        - name: Restart services
          systemd:
            name: "{{ item }}"
            state: restarted
          loop:
            - application
            - worker
      
      rescue:
        - name: Capture error details
          set_fact:
            error_timestamp: "{{ ansible_date_time.iso8601 }}"
            error_host: "{{ inventory_hostname }}"
        
        - name: Attempt graceful recovery
          include_tasks: recovery_procedures.yml
        
        - name: Send alert notification
          uri:
            url: "{{ alert_webhook }}"
            method: POST
            body_format: json
            body:
              severity: "critical"
              host: "{{ error_host }}"
              timestamp: "{{ error_timestamp }}"
              error: "{{ ansible_failed_result }}"
        
        - name: Fail with detailed message
          fail:
            msg: |
              Deployment failed on {{ error_host }}
              Time: {{ error_timestamp }}
              Error: {{ ansible_failed_result.msg | default('Unknown error') }}
      
      always:
        - name: Collect deployment logs
          fetch:
            src: /var/log/application.log
            dest: "logs/{{ inventory_hostname }}-{{ ansible_date_time.epoch }}.log"
            flat: true
        
        - name: Clean temporary files
          file:
            path: /tmp/deployment-*
            state: absent
```

### Conditions et Assertions

```yaml
---
- name: Conditional deployment based on environment
  hosts: all
  
  tasks:
    - name: Verify environment is defined
      assert:
        that:
          - environment is defined
          - environment in ['dev', 'staging', 'production']
        fail_msg: "environment must be defined and valid"
    
    - name: Production-only safety checks
      block:
        - name: Require manual approval variable for production
          assert:
            that:
              - production_deployment_approved | default(false) | bool
            fail_msg: "Production deployment requires explicit approval"
        
        - name: Verify backup exists
          stat:
            path: "/backup/latest-production-backup.tar.gz"
          register: backup_stat
          failed_when: not backup_stat.stat.exists
        
        - name: Require maintenance window
          assert:
            that:
              - ansible_date_time.hour | int >= 22 or ansible_date_time.hour | int <= 6
            fail_msg: "Production deployments only allowed during maintenance window (22:00-06:00)"
      when: environment == 'production'
    
    - name: Deploy based on environment
      include_tasks: "deploy_{{ environment }}.yml"
```

## Variables et Configuration

### Chargement de Variables par Environnement

```yaml
---
- name: Multi-environment deployment
  hosts: appservers
  
  vars_files:
    - vars/common.yml
    - "vars/{{ environment }}.yml"
    - "vars/{{ environment }}/{{ inventory_hostname }}.yml"
  
  vars:
    # Variables calculées
    deployment_id: "{{ ansible_date_time.epoch }}-{{ environment }}"
    
  tasks:
    - name: Display loaded configuration
      debug:
        msg:
          - "Environment: {{ environment }}"
          - "App Version: {{ app_version }}"
          - "Database: {{ db_host }}"
          - "Deployment ID: {{ deployment_id }}"
```

### Variables Promptées

```yaml
---
- name: Interactive deployment
  hosts: appservers
  
  vars_prompt:
    - name: deployment_version
      prompt: "Enter the version to deploy"
      private: false
    
    - name: deployment_confirmation
      prompt: "Type 'yes' to confirm deployment to {{ environment }}"
      private: false
    
    - name: vault_password
      prompt: "Enter vault password for secrets"
      private: true
  
  pre_tasks:
    - name: Verify confirmation
      assert:
        that:
          - deployment_confirmation == 'yes'
        fail_msg: "Deployment cancelled by user"
```

## Tags et Exécution Sélective

### Stratégie de Tags

```yaml
---
- name: Complete infrastructure setup
  hosts: all
  
  tasks:
    - name: Install base packages
      apt:
        name: "{{ base_packages }}"
        state: present
      tags:
        - install
        - packages
        - base
    
    - name: Configure firewall
      ufw:
        rule: allow
        port: "{{ item }}"
      loop: "{{ allowed_ports }}"
      tags:
        - configure
        - security
        - firewall
    
    - name: Deploy application
      copy:
        src: "app.jar"
        dest: /opt/app/
      tags:
        - deploy
        - application
    
    - name: Start services
      systemd:
        name: "{{ item }}"
        state: started
        enabled: true
      loop: "{{ services }}"
      tags:
        - service
        - startup
    
    - name: Run health checks
      uri:
        url: "http://localhost:8080/health"
      tags:
        - verify
        - health-check
        - never  # Ne s'exécute que si explicitement demandé
```

**Utilisation :**
```bash
# Exécuter uniquement les tasks de déploiement
ansible-playbook playbook.yml --tags deploy

# Exécuter plusieurs tags
ansible-playbook playbook.yml --tags "install,configure"

# Exécuter tout sauf certains tags
ansible-playbook playbook.yml --skip-tags "deploy"

# Forcer l'exécution de tags marqués 'never'
ansible-playbook playbook.yml --tags "health-check"
```

## Anti-Patterns à Éviter

### ❌ Mauvaises Pratiques

```yaml
# ❌ Pas de nom descriptif
- hosts: all
  tasks:
    - command: /some/script.sh

# ❌ Pas de validation
- hosts: production
  tasks:
    - name: Delete data
      file:
        path: /data
        state: absent

# ❌ Hardcoding de valeurs
- name: Deploy
  hosts: web
  tasks:
    - copy:
        src: app.jar
        dest: /opt/app/

# ❌ Pas de gestion d'erreurs
- name: Critical operation
  command: critical_command
  ignore_errors: true
```

### ✅ Bonnes Pratiques

```yaml
# ✅ Nom descriptif et structure complète
- name: Deploy application to production
  hosts: production_web
  
  pre_tasks:
    - name: Verify deployment prerequisites
      assert:
        that:
          - deployment_version is defined
          - backup_completed | default(false)
  
  tasks:
    - name: Deploy application version {{ deployment_version }}
      copy:
        src: "artifacts/app-{{ deployment_version }}.jar"
        dest: /opt/app/
      notify: Restart application
  
  post_tasks:
    - name: Verify application is healthy
      uri:
        url: "http://localhost:8080/health"
        status_code: 200
  
  handlers:
    - name: Restart application
      systemd:
        name: application
        state: restarted
```

## Workflow de Test OBLIGATOIRE

**Avant CHAQUE exécution en production, suivre CE workflow :**

```bash
# 1. Validation syntaxique (OBLIGATOIRE)
ansible-playbook -i inventories/production playbook.yml --syntax-check

# 2. Linting (OBLIGATOIRE - 0 erreurs)
ansible-lint playbook.yml

# 3. Dry-run complet (OBLIGATOIRE)
ansible-playbook -i inventories/production playbook.yml --check --diff

# 4. Exécution en environnement de test (OBLIGATOIRE)
ansible-playbook -i inventories/test playbook.yml

# 5. Test d'idempotence en test (OBLIGATOIRE - doit retourner changed=0)
ansible-playbook -i inventories/test playbook.yml

# 6. Validation post-exécution test (OBLIGATOIRE)
# Vérifier que l'application fonctionne correctement

# 7. SEULEMENT APRÈS - Dry-run production
ansible-playbook -i inventories/production playbook.yml --check --diff

# 8. SEULEMENT APRÈS - Exécution production avec approbation
ansible-playbook -i inventories/production playbook.yml
```

### Exemple de Playbook 100% Testable

```yaml
---
- name: Idempotent and testable deployment
  hosts: webservers
  
  # Support du check mode
  check_mode: false  # Le playbook lui-même n'est pas en check_mode
  
  pre_tasks:
    # Validation qui fonctionne en check mode
    - name: Validate prerequisites
      assert:
        that:
          - deployment_version is defined
          - ansible_memtotal_mb >= 2048
        fail_msg: "Prerequisites not met"
      check_mode: false  # Toujours exécuter
    
    # Dry-run compatible
    - name: Check if service exists
      systemd:
        name: application
      register: service_check
      check_mode: false
      ignore_errors: true
  
  tasks:
    # Task idempotente avec support check mode
    - name: Ensure configuration is present
      template:
        src: config.j2
        dest: /etc/app/config.yml
      register: config_result
      # Automatiquement compatible --check
    
    # Task idempotente avec gestion explicite
    - name: Run migration script
      command: /opt/app/bin/migrate.sh
      args:
        creates: /var/lib/app/migration_{{ deployment_version }}.done
      register: migration
      changed_when: "'migrations applied' in migration.stdout"
  
  post_tasks:
    # Vérification en check mode
    - name: Verify configuration syntax
      command: /opt/app/bin/validate-config
      changed_when: false
      check_mode: false
```

## Checklist Playbook

- [ ] Nom descriptif et explicite
- [ ] `hosts` défini correctement
- [ ] `become` explicitement défini
- [ ] `gather_facts` explicitement défini
- [ ] `pre_tasks` avec validations
- [ ] `post_tasks` avec vérifications
- [ ] Tags définis pour exécution sélective
- [ ] Gestion d'erreurs avec block/rescue
- [ ] Variables externalisées (pas de hardcoding)
- [ ] Handlers pour les redémarrages
- [ ] **✅ Testé avec `--syntax-check` - PASSÉ**
- [ ] **✅ Testé avec `--check --diff` - PASSÉ**
- [ ] **✅ Test d'idempotence - 2ème run = changed=0 - PASSÉ**
- [ ] **✅ Validé avec `ansible-lint` - 0 erreurs**
- [ ] **✅ Testé en environnement de test - PASSÉ**
- [ ] **✅ Rollback testé et fonctionnel**
