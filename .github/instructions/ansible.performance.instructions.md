---
description: Performance optimization, parallelism, caching, and execution strategies
name: Ansible_Performance_Optimization
applyTo: "**/ansible/**/*.yml,**/ansible/**/*.yaml"
---

# Ansible Performance - Guide Expert

## ⛔ À NE PAS FAIRE

- **N'active jamais** gather_facts si les facts ne sont pas nécessaires
- **N'installe jamais** les packages un par un (grouper en une seule task)
- **N'utilise jamais** la stratégie `linear` si `free` est plus adaptée
- **Ne désactive jamais** le pipelining SSH sans raison valable
- **N'ignore jamais** le cache de facts pour les gros inventaires
- **Ne configure jamais** un nombre de forks supérieur aux capacités du contrôleur

## ✅ À FAIRE

- **Configure toujours** le pipelining SSH (`pipelining = True`)
- **Active toujours** le cache de facts pour éviter les collectes répétées
- **Groupe toujours** les installations de packages en une seule task
- **Utilise toujours** `gather_facts: false` quand les facts ne sont pas nécessaires
- **Optimise toujours** le nombre de forks selon l'infrastructure
- **Profile toujours** les playbooks avec callback_whitelist pour identifier les goulots
- **Utilise toujours** async/poll pour les tâches longues

## 🎯 Actions Obligatoires (Mandatory)

**Optimisations À TOUJOURS implémenter :**

1. ✅ **Parallélisme** : Configurer `forks` approprié dans ansible.cfg
2. ✅ **Pipelining SSH** : Activer pour réduire les connexions SSH
3. ✅ **Cache de facts** : Activer le cache pour éviter de collecter les facts à chaque fois
4. ✅ **Gather_facts** : Désactiver si les facts ne sont pas nécessaires
5. ✅ **Stratégie d'exécution** : Choisir la stratégie adaptée (linear, free, host_pinned)
6. ✅ **Groupement des packages** : Installer plusieurs packages en une seule task

## Configuration ansible.cfg Optimisée

### Configuration Complète pour Performance

```ini
# ansible.cfg - Configuration optimisée
[defaults]
# =============================================================================
# PARALLÉLISME
# =============================================================================
# Nombre de processus parallèles (ajuster selon CPU disponible)
forks = 50

# Timeout des connexions
timeout = 30

# =============================================================================
# FACTS CACHING
# =============================================================================
# Mode de collecte des facts
gathering = smart  # Collecter uniquement si nécessaire

# Type de cache (jsonfile, redis, memcached)
fact_caching = jsonfile
fact_caching_connection = /tmp/ansible_facts
fact_caching_timeout = 86400  # 24 heures

# =============================================================================
# SSH OPTIMIZATION
# =============================================================================
[ssh_connection]
# SSH pipelining pour réduire le nombre de connexions
pipelining = True

# Multiplexage SSH pour réutiliser les connexions
ssh_args = -o ControlMaster=auto -o ControlPersist=600s -o ServerAliveInterval=60

# Chemin pour les sockets de contrôle
control_path = /tmp/ansible-ssh-%%h-%%p-%%r

# =============================================================================
# CALLBACKS ET MONITORING
# =============================================================================
[defaults]
# Callbacks pour mesurer les performances
callback_whitelist = profile_tasks, timer

# Affichage plus rapide
stdout_callback = yaml
bin_ansible_callbacks = True

# =============================================================================
# OPTIMISATIONS DIVERSES
# =============================================================================
# Désactiver la vérification des clés SSH (environnements de test)
# host_key_checking = False  # ⚠️ Décommenter uniquement pour dev/test

# Pas de retry files
retry_files_enabled = False

# Moins de verbosité pour les logs
deprecation_warnings = False
command_warnings = False
```

## Parallélisme et Forks

### Ajuster le Nombre de Forks

```ini
# ansible.cfg
[defaults]
# Pour serveurs puissants
forks = 100

# Pour workstation standard
forks = 50

# Pour petits environnements
forks = 10
```

```bash
# Surcharge en ligne de commande
ansible-playbook -f 100 playbook.yml

# Ajuster dynamiquement selon les ressources
ansible-playbook -f $(nproc) playbook.yml
```

### Tasks Parallèles avec async

```yaml
---
- name: Long running tasks in parallel
  hosts: webservers
  
  tasks:
    # Lancer plusieurs tâches longues en parallèle
    - name: Deploy application (async)
      copy:
        src: "app-{{ item }}.jar"
        dest: "/opt/app/module-{{ item }}/"
      loop:
        - module1
        - module2
        - module3
        - module4
      async: 300  # Timeout de 5 minutes
      poll: 0     # Ne pas attendre, exécuter en arrière-plan
      register: deploy_jobs
    
    # Attendre que toutes les tâches async se terminent
    - name: Wait for all deployments to complete
      async_status:
        jid: "{{ item.ansible_job_id }}"
      loop: "{{ deploy_jobs.results }}"
      register: job_results
      until: job_results.finished
      retries: 60
      delay: 5

# Exemple : Redémarrage parallèle avec async
- name: Restart services in parallel
  hosts: all
  
  tasks:
    - name: Restart service (async)
      systemd:
        name: "{{ item }}"
        state: restarted
      loop:
        - nginx
        - application
        - monitoring-agent
      async: 60
      poll: 0
      register: restart_jobs
    
    - name: Wait for all services to restart
      async_status:
        jid: "{{ item.ansible_job_id }}"
      loop: "{{ restart_jobs.results }}"
      register: restart_results
      until: restart_results.finished
      retries: 12
      delay: 5
```

## Stratégies d'Exécution

### Strategy: linear (par défaut)

```yaml
---
# Attendre que tous les hôtes terminent chaque task avant de passer à la suivante
- name: Deploy with linear strategy
  hosts: webservers
  strategy: linear  # Par défaut, pas besoin de le spécifier
  
  tasks:
    - name: Stop service
      systemd:
        name: application
        state: stopped
    
    # Tous les hôtes ont arrêté le service avant de continuer
    - name: Deploy new version
      copy:
        src: app.jar
        dest: /opt/app/
```

### Strategy: free

```yaml
---
# Chaque hôte progresse indépendamment
- name: Deploy with free strategy
  hosts: webservers
  strategy: free  # Hôtes rapides ne sont pas bloqués par les lents
  
  tasks:
    - name: Download large file
      get_url:
        url: https://example.com/large-file.tar.gz
        dest: /tmp/
    
    - name: Extract archive
      unarchive:
        src: /tmp/large-file.tar.gz
        dest: /opt/
        remote_src: true
    
    - name: Configure application
      template:
        src: config.j2
        dest: /etc/app/config.yml
```

### Strategy: host_pinned

```yaml
---
# Maintenir l'affinité hôte-worker (utile pour grandes inventaires)
- name: Deploy with host_pinned strategy
  hosts: all
  strategy: host_pinned
  
  tasks:
    - name: Task 1
      debug:
        msg: "Processing {{ inventory_hostname }}"
```

### Strategy: debug

```yaml
---
# Pour le débogage interactif
- name: Debug problematic playbook
  hosts: webservers
  strategy: debug
  
  tasks:
    - name: Potentially failing task
      command: /some/command
```

## Optimisation du Gathering des Facts

### Désactiver gather_facts

```yaml
---
# Désactiver la collecte de facts si non nécessaires
- name: Simple task without facts
  hosts: all
  gather_facts: false
  
  tasks:
    - name: Ping hosts
      ping:

# Collecter uniquement certains facts
- name: Selective fact gathering
  hosts: all
  gather_facts: true
  
  tasks:
    - name: Gather only network facts
      setup:
        filter:
          - 'ansible_default_ipv4'
          - 'ansible_all_ipv4_addresses'
          - 'ansible_hostname'
      when: false  # Skip si facts déjà présents

# Utiliser le cache de facts
- name: Use cached facts
  hosts: all
  gather_facts: smart  # Utiliser le cache si disponible
```

### Configuration du Cache de Facts

```ini
# ansible.cfg
[defaults]
gathering = smart
fact_caching = jsonfile
fact_caching_connection = ~/.ansible/facts_cache
fact_caching_timeout = 86400  # 24 heures

# Ou avec Redis pour environnement distribué
# fact_caching = redis
# fact_caching_connection = localhost:6379:0
# fact_caching_timeout = 86400
```

```yaml
---
# Forcer le refresh des facts
- name: Force fact gathering
  hosts: all
  gather_facts: true
  
  pre_tasks:
    - name: Clear fact cache
      file:
        path: ~/.ansible/facts_cache/{{ inventory_hostname }}
        state: absent
      delegate_to: localhost
      run_once: true
```

## Optimisation des Tasks

### Grouper les Packages

```yaml
---
# ❌ LENT - Une task par package
- name: Install nginx
  apt:
    name: nginx
    state: present

- name: Install curl
  apt:
    name: curl
    state: present

- name: Install git
  apt:
    name: git
    state: present

# ✅ RAPIDE - Tous les packages en une fois
- name: Install required packages
  apt:
    name:
      - nginx
      - curl
      - git
      - vim
      - htop
    state: present
    update_cache: true
    cache_valid_time: 3600  # Cache APT valide 1 heure
```

### Optimiser les Loops

```yaml
---
# ❌ LENT - Loop avec module qui accepte une liste
- name: Install packages (slow)
  apt:
    name: "{{ item }}"
    state: present
  loop:
    - nginx
    - curl
    - git

# ✅ RAPIDE - Pas de loop, liste directe
- name: Install packages (fast)
  apt:
    name:
      - nginx
      - curl
      - git
    state: present

# Pour les modules ne supportant pas les listes
- name: Create multiple users
  user:
    name: "{{ item.name }}"
    groups: "{{ item.groups }}"
  loop:
    - { name: 'alice', groups: 'developers' }
    - { name: 'bob', groups: 'ops' }
  # Pas d'alternative ici, le loop est nécessaire
```

### Utiliser include_tasks Efficacement

```yaml
---
# Charger dynamiquement uniquement ce qui est nécessaire
- name: Main playbook
  hosts: all
  
  tasks:
    - name: Include OS-specific tasks
      include_tasks: "{{ ansible_os_family }}.yml"
    
    - name: Include role-specific tasks
      include_tasks: "{{ server_role }}.yml"
      when: server_role is defined

# tasks/Debian.yml
---
- name: Update apt cache
  apt:
    update_cache: true
    cache_valid_time: 3600

- name: Install Debian-specific packages
  apt:
    name: "{{ debian_packages }}"
    state: present

# tasks/RedHat.yml
---
- name: Install RedHat-specific packages
  yum:
    name: "{{ redhat_packages }}"
    state: present
```

## Optimisation SSH

### Pipelining SSH

```ini
# ansible.cfg
[ssh_connection]
# Activer le pipelining SSH
pipelining = True

# ⚠️ Nécessite requiretty désactivé dans /etc/sudoers
# Defaults !requiretty
```

### ControlMaster et Multiplexage

```ini
# ansible.cfg
[ssh_connection]
ssh_args = -o ControlMaster=auto -o ControlPersist=600s -o ServerAliveInterval=60 -o ServerAliveCountMax=5

# Chemin pour les sockets de contrôle
control_path = /tmp/ansible-ssh-%%h-%%p-%%r

# Ou dans le home directory
# control_path = ~/.ssh/ansible-%%r@%%h:%%p
```

### Optimiser les Connexions SSH

```yaml
---
- name: Optimized SSH connections
  hosts: all
  
  vars:
    # Variables de connexion optimisées
    ansible_ssh_common_args: >-
      -o ControlMaster=auto
      -o ControlPersist=600s
      -o ServerAliveInterval=60
      -o ServerAliveCountMax=5
      -o Compression=yes
      -o TCPKeepAlive=yes
```

## Optimisation des Playbooks

### Run_once pour Tasks Globales

```yaml
---
- name: Initialization tasks
  hosts: all
  
  tasks:
    # Exécuter une seule fois pour tout le groupe
    - name: Download shared artifact
      get_url:
        url: https://example.com/artifact.tar.gz
        dest: /tmp/artifact.tar.gz
      run_once: true
      delegate_to: localhost
    
    - name: Initialize shared database
      command: /opt/scripts/init-db.sh
      run_once: true
      delegate_to: "{{ groups['dbservers'][0] }}"
```

### Delegate_facts pour Optimiser

```yaml
---
- name: Gather facts efficiently
  hosts: all
  
  tasks:
    # Collecter des infos depuis un hôte et les rendre disponibles
    - name: Get database version from primary
      command: psql --version
      delegate_to: "{{ groups['dbservers'][0] }}"
      delegate_facts: true
      run_once: true
      register: db_version
    
    - name: Use delegated facts
      debug:
        msg: "Database version: {{ hostvars[groups['dbservers'][0]]['db_version'] }}"
```

### Serial pour Déploiements Progressifs

```yaml
---
# Déployer progressivement pour limiter l'impact
- name: Rolling deployment
  hosts: webservers
  serial:
    - 1        # Premier serveur seul
    - 25%      # Puis 25% des serveurs
    - 50%      # Puis 50% des restants
    - 100%     # Enfin tous les restants
  
  max_fail_percentage: 25  # Arrêter si plus de 25% échouent
  
  tasks:
    - name: Deploy new version
      copy:
        src: app.jar
        dest: /opt/app/

# Déploiement batch par batch
- name: Batch deployment
  hosts: webservers
  serial: 5  # 5 serveurs à la fois
  
  tasks:
    - name: Update servers
      apt:
        upgrade: dist
```

## Profilage et Monitoring

### Activer les Callbacks de Performance

```ini
# ansible.cfg
[defaults]
callback_whitelist = profile_tasks, timer, profile_roles

# Ou en variable d'environnement
# export ANSIBLE_CALLBACKS_ENABLED=profile_tasks,timer
```

**Sortie avec profile_tasks :**
```
PLAY RECAP *****************************************************
web01 : ok=10 changed=3 unreachable=0 failed=0

Monday 27 November 2025  14:23:45 +0100 (0:00:02.134)
===============================================================================
Install packages ------------------------------------------- 45.23s
Deploy application ----------------------------------------- 23.45s
Configure nginx -------------------------------------------- 12.34s
...
```

### Mesurer les Performances d'un Playbook

```bash
# Avec time
time ansible-playbook playbook.yml

# Avec ANSIBLE_CALLBACKS
ANSIBLE_CALLBACKS_ENABLED=profile_tasks ansible-playbook playbook.yml

# Verbose pour plus de détails
ansible-playbook playbook.yml -vvv
```

### Script de Benchmark

```bash
#!/bin/bash
# benchmark.sh - Mesurer les performances d'un playbook

PLAYBOOK="$1"
INVENTORY="$2"
ITERATIONS="${3:-3}"

echo "Benchmarking $PLAYBOOK with $ITERATIONS iterations"
echo "=================================================="

total_time=0

for i in $(seq 1 $ITERATIONS); do
  echo "Run $i/$ITERATIONS..."
  
  start=$(date +%s)
  ansible-playbook -i "$INVENTORY" "$PLAYBOOK" > /dev/null 2>&1
  end=$(date +%s)
  
  duration=$((end - start))
  total_time=$((total_time + duration))
  
  echo "  Duration: ${duration}s"
done

avg_time=$((total_time / ITERATIONS))
echo "=================================================="
echo "Average execution time: ${avg_time}s"
```

## Optimisations Avancées

### Mitogen Strategy Plugin

```bash
# Installation de Mitogen pour accélération significative
pip install mitogen

# Télécharger le plugin
wget https://github.com/mitogen-hq/mitogen/archive/refs/heads/master.zip
unzip master.zip
```

```ini
# ansible.cfg
[defaults]
strategy_plugins = /path/to/mitogen/ansible_mitogen/plugins/strategy
strategy = mitogen_linear

[ssh_connection]
# Mitogen gère SSH mieux qu'Ansible natif
```

### Désactiver les Warnings

```ini
# ansible.cfg
[defaults]
deprecation_warnings = False
command_warnings = False
system_warnings = False
```

### Optimiser les Templates

```yaml
---
# Éviter de recalculer les templates à chaque fois
- name: Deploy configuration
  template:
    src: config.j2
    dest: /etc/app/config.yml
  register: config_result
  changed_when: config_result.changed

# Utiliser validate pour éviter de déployer si invalide
- name: Deploy nginx config with validation
  template:
    src: nginx.conf.j2
    dest: /etc/nginx/nginx.conf
    validate: 'nginx -t -c %s'  # Valide avant de copier
  notify: Reload nginx
```

## Checklist Performance ET Testabilité

### Performance

- [ ] `forks` configuré approprié (50-100)
- [ ] `pipelining` SSH activé
- [ ] Cache de facts configuré (`gathering = smart`)
- [ ] `gather_facts: false` quand non nécessaire
- [ ] Packages groupés (pas de loop inutile)
- [ ] Stratégie d'exécution adaptée (free, linear)
- [ ] `run_once` pour tasks globales
- [ ] `serial` pour déploiements progressifs
- [ ] Callbacks de profiling activés
- [ ] SSH ControlMaster configuré
- [ ] `async` utilisé pour tâches longues
- [ ] Templates validés avant déploiement
- [ ] Pas de commandes inutiles en loop
- [ ] Benchmarks effectués et documentés

### Testabilité (PRIORITAIRE sur Performance)

- [ ] **✅ Idempotence testée et validée**
- [ ] **✅ Check mode fonctionne correctement**
- [ ] **✅ Performance mesurée AVEC tests d'idempotence**
- [ ] **✅ Optimisations ne cassent PAS l'idempotence**

**⚠️ IMPORTANT : Ne JAMAIS sacrifier l'idempotence pour la performance**

## Commandes d'Analyse

```bash
# Profiler un playbook
ANSIBLE_CALLBACKS_ENABLED=profile_tasks,timer ansible-playbook playbook.yml

# Analyser les connexions SSH
ansible-playbook playbook.yml -vvv 2>&1 | grep "SSH:"

# Mesurer le temps d'exécution
time ansible-playbook playbook.yml

# Analyser l'utilisation des facts
ansible-playbook playbook.yml -vvv 2>&1 | grep "Gathering Facts"
```
