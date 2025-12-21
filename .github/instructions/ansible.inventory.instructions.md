---
description: Static and dynamic inventory management, host groups, and variables
name: Ansible_Inventory_Management
applyTo: "**/inventories/**/*.{yml,yaml,ini}"
---

# Ansible Inventory - Guide Expert

## 🎯 Actions Obligatoires (Mandatory)

**À TOUJOURS respecter pour les inventaires :**

1. ✅ **Séparer les environnements** : Un inventaire distinct par environnement
   ```
   inventories/
   ├── production/
   │   ├── hosts
   │   ├── group_vars/
   │   └── host_vars/
   ├── staging/
   └── development/
   ```

2. ✅ **Structure hiérarchique** : Organiser les hôtes en groupes logiques
   ```ini
   [webservers]
   [dbservers]
   [loadbalancers]
   
   [production:children]
   webservers
   dbservers
   loadbalancers
   ```

3. ✅ **Variables par niveau** : Utiliser group_vars et host_vars correctement
4. ✅ **Documentation** : Commenter les inventaires complexes
5. ✅ **Pas de secrets** : Jamais de mots de passe en clair dans les inventaires
6. ✅ **Nommage cohérent** : Convention de nommage claire et maintenue

## Inventaire Statique (INI Format)

### Structure de Base

```ini
# inventories/production/hosts

# ===========================================================================
# WEB TIER - Frontend web servers
# ===========================================================================
[webservers]
web01.example.com ansible_host=10.0.1.10
web02.example.com ansible_host=10.0.1.11
web03.example.com ansible_host=10.0.1.12

[webservers:vars]
# Variables communes aux webservers
nginx_worker_processes=4
nginx_worker_connections=2048

# ===========================================================================
# APPLICATION TIER - Application servers
# ===========================================================================
[appservers]
app01.example.com ansible_host=10.0.2.10 app_memory=8GB
app02.example.com ansible_host=10.0.2.11 app_memory=8GB
app03.example.com ansible_host=10.0.2.12 app_memory=16GB  # Plus de mémoire

[appservers:vars]
app_port=8080
app_pool_size=50

# ===========================================================================
# DATABASE TIER - Database servers
# ===========================================================================
[dbservers]
db01.example.com ansible_host=10.0.3.10 postgresql_role=primary
db02.example.com ansible_host=10.0.3.11 postgresql_role=replica
db03.example.com ansible_host=10.0.3.12 postgresql_role=replica

[dbservers:vars]
postgresql_version=15
postgresql_max_connections=200

# ===========================================================================
# LOAD BALANCERS - HAProxy/Nginx load balancers
# ===========================================================================
[loadbalancers]
lb01.example.com ansible_host=10.0.0.10 priority=100
lb02.example.com ansible_host=10.0.0.11 priority=50  # Backup

[loadbalancers:vars]
lb_algorithm=roundrobin

# ===========================================================================
# MONITORING - Prometheus, Grafana, etc.
# ===========================================================================
[monitoring]
monitor01.example.com ansible_host=10.0.4.10

# ===========================================================================
# PARENT GROUPS - Logical grouping
# ===========================================================================
[production:children]
webservers
appservers
dbservers
loadbalancers
monitoring

[production:vars]
# Variables globales pour production
environment=production
datacenter=paris
ansible_user=deploy
ansible_python_interpreter=/usr/bin/python3

# ===========================================================================
# SPECIAL GROUPS - By functionality
# ===========================================================================
[frontend:children]
webservers
loadbalancers

[backend:children]
appservers
dbservers

[critical:children]
dbservers
loadbalancers
```

### Variables par Hôte

```ini
# Syntaxes pour les variables d'hôte
server01.example.com ansible_host=192.168.1.10 ansible_port=2222
server02.example.com ansible_host=192.168.1.11 custom_var="value with spaces"
server03.example.com ansible_host=192.168.1.12 \
  cpu_cores=16 \
  memory_gb=64 \
  disk_size_tb=2
```

## Inventaire Statique (YAML Format)

### Structure YAML

```yaml
# inventories/production/hosts.yml
---
all:
  children:
    # ========================================================================
    # WEB TIER
    # ========================================================================
    webservers:
      hosts:
        web01.example.com:
          ansible_host: 10.0.1.10
        web02.example.com:
          ansible_host: 10.0.1.11
        web03.example.com:
          ansible_host: 10.0.1.12
      vars:
        nginx_worker_processes: 4
        nginx_worker_connections: 2048
    
    # ========================================================================
    # APPLICATION TIER
    # ========================================================================
    appservers:
      hosts:
        app01.example.com:
          ansible_host: 10.0.2.10
          app_memory: 8GB
        app02.example.com:
          ansible_host: 10.0.2.11
          app_memory: 8GB
        app03.example.com:
          ansible_host: 10.0.2.12
          app_memory: 16GB  # Plus de mémoire pour les traitements lourds
      vars:
        app_port: 8080
        app_pool_size: 50
    
    # ========================================================================
    # DATABASE TIER
    # ========================================================================
    dbservers:
      hosts:
        db01.example.com:
          ansible_host: 10.0.3.10
          postgresql_role: primary
          postgresql_max_wal_senders: 5
        db02.example.com:
          ansible_host: 10.0.3.11
          postgresql_role: replica
          postgresql_primary_host: db01.example.com
        db03.example.com:
          ansible_host: 10.0.3.12
          postgresql_role: replica
          postgresql_primary_host: db01.example.com
      vars:
        postgresql_version: 15
        postgresql_max_connections: 200
    
    # ========================================================================
    # LOAD BALANCERS
    # ========================================================================
    loadbalancers:
      hosts:
        lb01.example.com:
          ansible_host: 10.0.0.10
          priority: 100  # Primary
          virtual_ip: 10.0.0.100
        lb02.example.com:
          ansible_host: 10.0.0.11
          priority: 50   # Backup
          virtual_ip: 10.0.0.100
      vars:
        lb_algorithm: roundrobin
        lb_health_check_interval: 5
    
    # ========================================================================
    # PARENT GROUP - PRODUCTION
    # ========================================================================
    production:
      children:
        webservers:
        appservers:
        dbservers:
        loadbalancers:
      vars:
        environment: production
        datacenter: paris
        ansible_user: deploy
        ansible_python_interpreter: /usr/bin/python3
        ansible_ssh_private_key_file: ~/.ssh/id_rsa_prod
    
    # ========================================================================
    # FUNCTIONAL GROUPS
    # ========================================================================
    frontend:
      children:
        webservers:
        loadbalancers:
    
    backend:
      children:
        appservers:
        dbservers:
```

## Group Vars et Host Vars

### Structure de group_vars

```
inventories/production/
├── hosts
├── group_vars/
│   ├── all.yml                    # Variables pour tous les hôtes
│   ├── production.yml             # Variables pour le groupe production
│   ├── webservers/
│   │   ├── vars.yml              # Variables webservers
│   │   └── vault.yml             # Secrets webservers (chiffré)
│   ├── appservers/
│   │   ├── vars.yml
│   │   └── vault.yml
│   └── dbservers/
│       ├── vars.yml
│       └── vault.yml
└── host_vars/
    ├── web01.example.com.yml      # Variables spécifiques à web01
    └── db01.example.com.yml       # Variables spécifiques à db01
```

### group_vars/all.yml

```yaml
---
# Variables globales pour tous les hôtes de tous les environnements

# Configuration Ansible
ansible_connection: ssh
ansible_user: deploy
ansible_python_interpreter: /usr/bin/python3

# Timezone
timezone: Europe/Paris

# NTP Servers
ntp_servers:
  - 0.fr.pool.ntp.org
  - 1.fr.pool.ntp.org
  - 2.fr.pool.ntp.org

# DNS Servers
dns_servers:
  - 8.8.8.8
  - 8.8.4.4

# Packages de base pour tous les serveurs
base_packages:
  - curl
  - wget
  - vim
  - git
  - htop
  - net-tools

# Logging
log_retention_days: 30

# Monitoring
monitoring_agent_enabled: true
monitoring_endpoint: "https://monitoring.example.com"
```

### group_vars/production.yml

```yaml
---
# Variables spécifiques à l'environnement production

environment: production
domain: example.com

# Backup configuration
backup_enabled: true
backup_retention_days: 90
backup_destination: "s3://backups-production"

# Security
firewall_enabled: true
fail2ban_enabled: true

# SSL/TLS
ssl_enabled: true
ssl_cert_path: /etc/ssl/certs/example.com.crt
ssl_key_path: /etc/ssl/private/example.com.key

# External services
api_endpoint: "https://api.production.example.com"
database_host: "db01.example.com"

# Alerting
alert_email: ops@example.com
slack_webhook_url: "{{ vault_slack_webhook_url }}"  # Depuis vault
```

### group_vars/webservers/vars.yml

```yaml
---
# Configuration spécifique aux serveurs web

# Nginx configuration
nginx_version: "1.24"
nginx_worker_processes: "{{ ansible_processor_vcpus }}"
nginx_worker_connections: 2048
nginx_keepalive_timeout: 65
nginx_client_max_body_size: 10M

# Upstream servers
nginx_upstream_servers: "{{ groups['appservers'] }}"

# Virtual hosts
nginx_vhosts:
  - server_name: www.example.com
    listen: 80
    ssl: false
    locations:
      - path: /
        proxy_pass: http://app_backend
  
  - server_name: www.example.com
    listen: 443
    ssl: true
    ssl_certificate: "{{ ssl_cert_path }}"
    ssl_certificate_key: "{{ ssl_key_path }}"
    locations:
      - path: /
        proxy_pass: http://app_backend

# Logging
nginx_access_log: /var/log/nginx/access.log
nginx_error_log: /var/log/nginx/error.log
```

### group_vars/dbservers/vars.yml

```yaml
---
# Configuration PostgreSQL

postgresql_version: 15
postgresql_port: 5432
postgresql_listen_addresses: "*"

# Performance tuning
postgresql_shared_buffers: "{{ (ansible_memtotal_mb * 0.25) | int }}MB"
postgresql_effective_cache_size: "{{ (ansible_memtotal_mb * 0.75) | int }}MB"
postgresql_work_mem: "16MB"
postgresql_maintenance_work_mem: "512MB"

# Replication
postgresql_wal_level: replica
postgresql_max_wal_senders: 5
postgresql_wal_keep_size: "1GB"

# Authentication (passwords depuis vault)
postgresql_databases:
  - name: production_db
    owner: appuser
    encoding: UTF8

postgresql_users:
  - name: appuser
    password: "{{ vault_postgresql_appuser_password }}"
    privileges: ALL
    database: production_db
  
  - name: readonly
    password: "{{ vault_postgresql_readonly_password }}"
    privileges: SELECT
    database: production_db
```

### group_vars/dbservers/vault.yml (CHIFFRÉ)

```yaml
---
# Secrets pour les bases de données (fichier chiffré avec ansible-vault)

vault_postgresql_appuser_password: "AppUser_SecretPass123!"
vault_postgresql_readonly_password: "ReadOnly_SecretPass456!"
vault_postgresql_replication_password: "Replication_SecretPass789!"
```

### host_vars/db01.example.com.yml

```yaml
---
# Configuration spécifique à db01 (primary)

postgresql_role: primary

# Configuration primaire spécifique
postgresql_max_wal_senders: 5
postgresql_synchronous_commit: on
postgresql_synchronous_standby_names: "db02,db03"

# Backup sur le primaire
postgresql_backup_enabled: true
postgresql_backup_schedule: "0 2 * * *"  # 2h du matin
```

### host_vars/db02.example.com.yml

```yaml
---
# Configuration spécifique à db02 (replica)

postgresql_role: replica
postgresql_primary_host: db01.example.com
postgresql_primary_port: 5432
postgresql_replication_user: replicator
postgresql_replication_password: "{{ vault_postgresql_replication_password }}"

# Pas de backup sur les replicas
postgresql_backup_enabled: false
```

## Inventaire Dynamique

### AWS EC2 Dynamic Inventory

```yaml
# inventory.aws_ec2.yml
---
plugin: amazon.aws.aws_ec2

# Régions AWS à interroger
regions:
  - eu-west-1
  - eu-west-3

# Filtres pour sélectionner les instances
filters:
  tag:Environment: production
  instance-state-name: running

# Créer des groupes basés sur les tags
keyed_groups:
  # Groupe par rôle (tag Role)
  - key: tags.Role
    prefix: role
    separator: _
  
  # Groupe par environnement
  - key: tags.Environment
    prefix: env
  
  # Groupe par type d'instance
  - key: instance_type
    prefix: instance_type
  
  # Groupe par zone de disponibilité
  - key: placement.availability_zone
    prefix: az

# Variables d'hôte à partir des attributs EC2
hostnames:
  - tag:Name
  - dns-name
  - private-ip-address

# Composer des groupes personnalisés
compose:
  ansible_host: public_ip_address
  ansible_user: ubuntu
  datacenter: placement.region

# Exclure certaines instances
exclude_filters:
  - tag:Exclude: true
```

**Utilisation :**
```bash
# Lister les hôtes
ansible-inventory -i inventory.aws_ec2.yml --graph

# Exécuter un playbook
ansible-playbook -i inventory.aws_ec2.yml playbook.yml

# Limiter à un groupe spécifique
ansible-playbook -i inventory.aws_ec2.yml -l role_webserver playbook.yml
```

### Azure Dynamic Inventory

```yaml
# inventory.azure_rm.yml
---
plugin: azure.azcollection.azure_rm

# Authentification Azure
auth_source: auto  # Utilise les credentials configurées (az login)

# Inclure uniquement les VMs dans certains groupes de ressources
include_vm_resource_groups:
  - production-rg
  - production-web-rg

# Filtres
exclude_host_filters:
  - powerstate != 'running'

# Créer des groupes basés sur les tags
keyed_groups:
  - prefix: tag
    key: tags
  - prefix: azure_location
    key: location
  - prefix: azure_resourcegroup
    key: resource_group

# Variables d'hôte
hostvar_expressions:
  ansible_host: public_ipv4_addresses[0] | default(private_ipv4_addresses[0])
  ansible_user: admin_username

# Groupes conditionnels
conditional_groups:
  webservers: "'web' in tags.role"
  dbservers: "'database' in tags.role"
```

### GCP Dynamic Inventory

```yaml
# inventory.gcp_compute.yml
---
plugin: google.cloud.gcp_compute

# Projets GCP
projects:
  - production-project-123456

# Filtres
filters:
  - labels.environment = production
  - status = RUNNING

# Zones
zones:
  - europe-west1-b
  - europe-west1-c

# Groupes basés sur les labels
keyed_groups:
  - key: labels.role
    prefix: role
  - key: labels.environment
    prefix: env
  - key: zone
    prefix: zone

# Variables
hostnames:
  - name
compose:
  ansible_host: networkInterfaces[0].accessConfigs[0].natIP | default(networkInterfaces[0].networkIP)
  ansible_user: ansible
```

### Script Personnalisé d'Inventaire Dynamique

```python
#!/usr/bin/env python3
# inventory_custom.py

import json
import sys

def get_inventory():
    """Retourne l'inventaire au format JSON"""
    
    inventory = {
        '_meta': {
            'hostvars': {
                'server01.example.com': {
                    'ansible_host': '10.0.1.10',
                    'ansible_user': 'deploy',
                    'app_version': '2.1.0'
                },
                'server02.example.com': {
                    'ansible_host': '10.0.1.11',
                    'ansible_user': 'deploy',
                    'app_version': '2.1.0'
                }
            }
        },
        'webservers': {
            'hosts': ['server01.example.com', 'server02.example.com'],
            'vars': {
                'nginx_port': 80
            }
        },
        'production': {
            'children': ['webservers']
        }
    }
    
    return inventory

def get_host(hostname):
    """Retourne les variables pour un hôte spécifique"""
    inventory = get_inventory()
    return inventory['_meta']['hostvars'].get(hostname, {})

if __name__ == '__main__':
    if len(sys.argv) == 2 and sys.argv[1] == '--list':
        print(json.dumps(get_inventory(), indent=2))
    elif len(sys.argv) == 3 and sys.argv[1] == '--host':
        print(json.dumps(get_host(sys.argv[2]), indent=2))
    else:
        print("Usage: %s --list or --host <hostname>" % sys.argv[0])
        sys.exit(1)
```

**Rendre exécutable :**
```bash
chmod +x inventory_custom.py
ansible-playbook -i inventory_custom.py playbook.yml
```

## Patterns et Sélection d'Hôtes

### Patterns de Base

```bash
# Tous les hôtes
ansible all -m ping

# Un groupe spécifique
ansible webservers -m ping

# Plusieurs groupes
ansible 'webservers:dbservers' -m ping

# Exclusion
ansible 'all:!webservers' -m ping

# Intersection
ansible 'webservers:&production' -m ping

# Un hôte spécifique
ansible web01.example.com -m ping

# Pattern avec wildcard
ansible 'web*.example.com' -m ping

# Plage numérique
ansible 'web[01:10].example.com' -m ping
```

### Patterns Avancés

```bash
# Groupes imbriqués
ansible 'production:&frontend' -m ping

# Multiple conditions
ansible 'webservers:&production:!staging' -m ping

# Regex
ansible '~web[0-9]+\.example\.com' -m ping

# Variables
ansible 'all:&environment_production' --extra-vars "environment=production" -m ping
```

## Bonnes Pratiques

### Organisation Multi-Environnement

```
inventories/
├── production/
│   ├── hosts
│   ├── group_vars/
│   │   ├── all.yml
│   │   ├── webservers.yml
│   │   └── dbservers/
│   │       ├── vars.yml
│   │       └── vault.yml
│   └── host_vars/
│       └── db01.example.com.yml
├── staging/
│   ├── hosts
│   └── group_vars/
│       └── all.yml
└── development/
    ├── hosts
    └── group_vars/
        └── all.yml
```

### Variables d'Inventaire Utiles

```yaml
---
# Connexion
ansible_host: 192.168.1.10          # IP ou hostname réel
ansible_port: 22                     # Port SSH (défaut 22)
ansible_user: deploy                 # Utilisateur SSH
ansible_ssh_private_key_file: ~/.ssh/id_rsa
ansible_ssh_common_args: '-o StrictHostKeyChecking=no'

# Python
ansible_python_interpreter: /usr/bin/python3

# Become
ansible_become: true
ansible_become_user: root
ansible_become_method: sudo
ansible_become_password: "{{ vault_sudo_password }}"

# Connexion alternative
ansible_connection: local           # Pour localhost
ansible_connection: docker          # Pour conteneurs
```

## Checklist Inventaire - Tests OBLIGATOIRES

### Structure

- [ ] Un inventaire par environnement
- [ ] Groupes logiques bien définis
- [ ] Hiérarchie de groupes (children) utilisée
- [ ] group_vars et host_vars organisés proprement
- [ ] Variables communes dans group_vars/all.yml
- [ ] Documentation des groupes et variables
- [ ] Conventions de nommage respectées

### Sécurité

- [ ] Secrets dans vault.yml chiffrés
- [ ] **✅ Aucun secret en clair détecté**
- [ ] **✅ Vault files testés (déchiffrement OK)**

### Validation et Tests

- [ ] **✅ Inventaire validé avec `ansible-inventory --list`**
- [ ] **✅ Inventaire parsé sans erreurs**
- [ ] **✅ Patterns d'hôtes testés**
- [ ] **✅ Test de connexion à tous les hôtes réussi**
- [ ] **✅ Variables d'inventaire testées**

### Tests Automatiques

```bash
#!/bin/bash
# test-inventory.sh - Tests d'inventaire OBLIGATOIRES

INVENTORY="$1"

if [ -z "$INVENTORY" ]; then
  echo "Usage: $0 <inventory_path>"
  exit 1
fi

echo "=== Testing Inventory: $INVENTORY ==="

# Test 1: Parse inventory
echo "[1/4] Testing inventory parsing..."
ansible-inventory -i "$INVENTORY" --list > /dev/null || {
  echo "❌ FAILED: Inventory parse error"
  exit 1
}
echo "✅ PASSED"

# Test 2: Verify vault files
echo "[2/4] Testing vault files..."
find "$INVENTORY" -name "vault.yml" | while read vault_file; do
  if grep -q '$ANSIBLE_VAULT' "$vault_file"; then
    echo "  ✓ $vault_file is encrypted"
  else
    echo "  ❌ $vault_file is NOT encrypted!"
    exit 1
  fi
done
echo "✅ PASSED"

# Test 3: Check for secrets in clear
echo "[3/4] Checking for secrets in clear..."
if grep -r "password:.*[^{]" "$INVENTORY" --include="*.yml" --exclude="*vault.yml"; then
  echo "❌ FAILED: Secrets found in clear text"
  exit 1
fi
echo "✅ PASSED: No secrets in clear"

# Test 4: Test connectivity
echo "[4/4] Testing host connectivity..."
ansible all -i "$INVENTORY" -m ping --one-line || {
  echo "⚠️  WARNING: Some hosts unreachable (may be expected)"
}
echo "✅ Inventory tests complete"
```
