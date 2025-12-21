---
description: Security, Ansible Vault, secrets management, and privilege escalation
name: Ansible_Security_Best_Practices
applyTo: "**/*.{yml,yaml}"
---

# Ansible Security - Guide Expert

## 🎯 Actions Obligatoires (Mandatory)

**Règles de sécurité À TOUJOURS respecter :**

1. ✅ **JAMAIS de secrets en clair** : Tous les secrets doivent être chiffrés avec Ansible Vault
   ```yaml
   # ❌ INTERDIT
   db_password: "MyPassword123"
   
   # ✅ OBLIGATOIRE
   db_password: "{{ vault_db_password }}"
   ```

2. ✅ **Fichiers vault séparés** : Un fichier vault par environnement
   ```
   group_vars/
   ├── production/
   │   ├── vars.yml       # Variables non sensibles
   │   └── vault.yml      # Variables sensibles CHIFFRÉES
   └── staging/
       ├── vars.yml
       └── vault.yml
   ```

3. ✅ **Préfixe `vault_`** : Toutes les variables dans vault doivent commencer par `vault_`
   ```yaml
   # vault.yml (chiffré)
   vault_db_password: "secret123"
   vault_api_key: "sk-abc123"
   
   # vars.yml (clair)
   db_password: "{{ vault_db_password }}"
   api_key: "{{ vault_api_key }}"
   ```

4. ✅ **Moindre privilège** : Utiliser `become` uniquement quand nécessaire
5. ✅ **Validation des entrées** : Toujours valider les variables critiques
6. ✅ **Fichiers vault dans .gitignore** : NE JAMAIS commit les mots de passe vault

## Ansible Vault - Chiffrement des Secrets

### Création et Gestion de Fichiers Vault

```bash
# Créer un nouveau fichier vault chiffré
ansible-vault create group_vars/production/vault.yml

# Éditer un fichier vault existant
ansible-vault edit group_vars/production/vault.yml

# Chiffrer un fichier existant
ansible-vault encrypt group_vars/production/secrets.yml

# Déchiffrer un fichier (temporairement)
ansible-vault decrypt group_vars/production/vault.yml

# Rechiffrer après décryptage
ansible-vault encrypt group_vars/production/vault.yml

# Voir le contenu sans éditer
ansible-vault view group_vars/production/vault.yml

# Changer le mot de passe d'un vault
ansible-vault rekey group_vars/production/vault.yml
```

### Structure Recommandée des Fichiers Vault

```yaml
# group_vars/production/vault.yml (CHIFFRÉ avec ansible-vault)
---
# ============================================================================
# CREDENTIALS BASE DE DONNÉES
# ============================================================================
vault_postgresql_admin_password: "SuperSecret123!"
vault_postgresql_replication_password: "ReplSecret456!"
vault_mysql_root_password: "MySQLRoot789!"

# ============================================================================
# API KEYS ET TOKENS
# ============================================================================
vault_github_api_token: "ghp_xxxxxxxxxxx"
vault_aws_access_key: "AKIAXXXXX"
vault_aws_secret_key: "xxxxxxxxx"
vault_slack_webhook_url: "https://hooks.slack.com/services/xxx"

# ============================================================================
# CERTIFICATES ET KEYS
# ============================================================================
vault_ssl_private_key: |
  -----BEGIN PRIVATE KEY-----
  MIIEvQIBADANBgkqhkiG9w0BAQEFA...
  -----END PRIVATE KEY-----

vault_jwt_secret_key: "your-256-bit-secret"

# ============================================================================
# PASSWORDS APPLICATIFS
# ============================================================================
vault_app_admin_password: "AdminPass123!"
vault_app_database_password: "DbPass456!"
vault_ldap_bind_password: "LdapPass789!"

# ============================================================================
# CREDENTIALS SERVICES EXTERNES
# ============================================================================
vault_monitoring_api_key: "mon-api-key-xxx"
vault_backup_encryption_password: "backup-secret"
vault_smtp_password: "smtp-pass"
```

```yaml
# group_vars/production/vars.yml (NON CHIFFRÉ)
---
# Références aux secrets du vault
postgresql_admin_password: "{{ vault_postgresql_admin_password }}"
postgresql_replication_password: "{{ vault_postgresql_replication_password }}"

github_api_token: "{{ vault_github_api_token }}"
aws_access_key: "{{ vault_aws_access_key }}"
aws_secret_key: "{{ vault_aws_secret_key }}"

app_admin_password: "{{ vault_app_admin_password }}"
app_database_password: "{{ vault_app_database_password }}"

# Configuration non sensible
postgresql_version: "15"
postgresql_port: 5432
postgresql_max_connections: 100
```

### Utilisation dans les Playbooks

```bash
# Exécuter avec mot de passe prompt
ansible-playbook playbook.yml --ask-vault-pass

# Exécuter avec fichier mot de passe
ansible-playbook playbook.yml --vault-password-file ~/.vault_pass.txt

# Avec vault-id (multiple vaults)
ansible-playbook playbook.yml --vault-id prod@~/.vault_pass_prod

# Multiples vault-ids
ansible-playbook playbook.yml \
  --vault-id dev@~/.vault_pass_dev \
  --vault-id prod@~/.vault_pass_prod
```

### Vault Password File Sécurisé

```bash
# Créer un fichier de mot de passe sécurisé
echo "VotreMotDePasseVault" > ~/.vault_pass.txt
chmod 600 ~/.vault_pass.txt

# Ajouter à .gitignore
echo ".vault_pass*" >> .gitignore
echo "vault_pass*" >> .gitignore
```

```ini
# ansible.cfg - Configuration pour vault par défaut
[defaults]
vault_password_file = ~/.vault_pass.txt
```

### Chiffrement de Variables Inline

```yaml
---
# Chiffrer une seule variable au lieu du fichier entier
db_password: !vault |
  $ANSIBLE_VAULT;1.1;AES256
  66386439653637653863663731393366633334313634396532363933343630653234613233343736
  ...
```

```bash
# Créer une variable chiffrée
ansible-vault encrypt_string 'MySecretPassword' --name 'db_password'

# Résultat à copier dans votre fichier vars
db_password: !vault |
  $ANSIBLE_VAULT;1.1;AES256
  ...
```

## Principe du Moindre Privilège

### Utilisation de `become`

```yaml
---
- name: Security-conscious playbook
  hosts: servers
  become: false  # Par défaut, pas de privilèges élevés
  
  tasks:
    # Task sans privilèges
    - name: Check application status
      command: systemctl --user status myapp
      changed_when: false
    
    # Task avec privilèges uniquement quand nécessaire
    - name: Install system package
      apt:
        name: nginx
        state: present
      become: true  # Privilèges élevés uniquement pour cette task
    
    # Task avec utilisateur spécifique
    - name: Deploy application as app user
      copy:
        src: app.jar
        dest: /opt/app/
      become: true
      become_user: appuser  # Devenir appuser, pas root
```

### Créer des Utilisateurs de Service

```yaml
---
- name: Create service account with minimal privileges
  hosts: appservers
  become: true
  
  tasks:
    - name: Create application user
      user:
        name: appuser
        comment: "Application Service Account"
        system: true
        shell: /bin/false  # Pas de shell interactif
        create_home: false  # Pas de home directory
        state: present
    
    - name: Create application directories
      file:
        path: "{{ item }}"
        state: directory
        owner: appuser
        group: appuser
        mode: '0755'
      loop:
        - /opt/app
        - /var/log/app
        - /var/lib/app
    
    - name: Deploy application with restricted permissions
      copy:
        src: app.jar
        dest: /opt/app/app.jar
        owner: appuser
        group: appuser
        mode: '0550'  # Lecture + exécution, pas d'écriture
      become: true
      become_user: appuser
```

### Sudo Configuration Sécurisée

```yaml
---
- name: Configure restricted sudo access
  hosts: servers
  become: true
  
  tasks:
    - name: Create sudoers file for deployment user
      copy:
        dest: /etc/sudoers.d/deploy
        content: |
          # Deployment user - restricted sudo access
          deploy ALL=(ALL) NOPASSWD: /bin/systemctl restart myapp
          deploy ALL=(ALL) NOPASSWD: /bin/systemctl reload myapp
          deploy ALL=(ALL) NOPASSWD: /bin/systemctl status myapp
        mode: '0440'
        validate: 'visudo -cf %s'
```

## Validation et Sécurité des Entrées

### Assertions et Validations

```yaml
---
- name: Secure deployment with input validation
  hosts: production
  
  pre_tasks:
    - name: Validate critical variables are defined
      assert:
        that:
          - deployment_version is defined
          - deployment_version is version('1.0.0', '>=')
          - environment in ['staging', 'production']
          - db_password is defined
          - db_password | length >= 12
        fail_msg: "Critical variables missing or invalid"
        success_msg: "All required variables validated"
    
    - name: Validate environment-specific requirements
      assert:
        that:
          - production_approval is defined
          - production_approval | bool
        fail_msg: "Production deployment requires explicit approval"
      when: environment == 'production'
    
    - name: Validate file paths (prevent path traversal)
      assert:
        that:
          - app_install_path is match('^/opt/.*')
          - '../' not in app_install_path
        fail_msg: "Invalid installation path"
```

### Sanitization des Entrées

```yaml
---
- name: Deploy with input sanitization
  hosts: servers
  
  vars:
    # Sanitize user input
    safe_version: "{{ deployment_version | regex_replace('[^a-zA-Z0-9.-]', '') }}"
    safe_environment: "{{ environment | lower | regex_replace('[^a-z]', '') }}"
  
  tasks:
    - name: Use sanitized variables
      copy:
        src: "artifacts/app-{{ safe_version }}.jar"
        dest: "/opt/app/{{ safe_environment }}/app.jar"
```

## Sécurité des Connexions

### Configuration SSH Sécurisée

```ini
# ansible.cfg
[defaults]
host_key_checking = True  # Vérifier les clés SSH
private_key_file = ~/.ssh/ansible_ed25519

[ssh_connection]
ssh_args = -o ControlMaster=auto -o ControlPersist=60s -o ServerAliveInterval=60
pipelining = True
control_path = /tmp/ansible-ssh-%%h-%%p-%%r

# Utiliser des algorithmes sécurisés
ssh_args = -o KexAlgorithms=curve25519-sha256@libssh.org -o Ciphers=chacha20-poly1305@openssh.com
```

### Connexion avec Bastion/Jump Host

```yaml
---
- name: Connect through bastion host
  hosts: private_servers
  
  vars:
    ansible_ssh_common_args: '-o ProxyCommand="ssh -W %h:%p -q bastion.example.com"'
  
  tasks:
    - name: Deploy to servers behind bastion
      copy:
        src: app.jar
        dest: /opt/app/
```

## Sécurisation des Fichiers et Permissions

### Permissions Strictes

```yaml
---
- name: Deploy with secure file permissions
  hosts: servers
  
  tasks:
    - name: Deploy configuration file with restricted permissions
      template:
        src: config.j2
        dest: /etc/app/config.yml
        owner: appuser
        group: appuser
        mode: '0400'  # Lecture seule par le propriétaire
    
    - name: Deploy secrets file
      copy:
        content: "{{ app_secret_key }}"
        dest: /etc/app/.secret
        owner: appuser
        group: appuser
        mode: '0400'  # Lecture seule
        attributes: '+i'  # Immutable (nécessite chattr)
    
    - name: Create directory with sticky bit
      file:
        path: /var/shared/app
        state: directory
        mode: '1777'  # Sticky bit pour shared directory
```

### No_log pour les Données Sensibles

```yaml
---
- name: Handle sensitive data securely
  hosts: servers
  
  tasks:
    - name: Configure application with API key
      template:
        src: app-config.j2
        dest: /etc/app/config.yml
      no_log: true  # Ne pas logger le contenu
    
    - name: Set password in application
      command: /opt/app/bin/set-password {{ app_admin_password }}
      no_log: true  # Ne pas logger la commande
    
    - name: Query external API with credentials
      uri:
        url: "https://api.example.com/data"
        headers:
          Authorization: "Bearer {{ api_token }}"
        method: GET
      register: api_response
      no_log: true  # Ne pas logger headers avec token
```

## Audit et Logging Sécurisé

### Configuration de Logging

```ini
# ansible.cfg
[defaults]
log_path = /var/log/ansible/ansible.log
log_filter = /path/to/filter_script.py  # Filtrer les secrets des logs

# Callback plugin pour logging sécurisé
callback_whitelist = profile_tasks, timer
```

### Traçabilité des Changements

```yaml
---
- name: Auditable deployment
  hosts: production
  
  vars:
    deployment_metadata:
      timestamp: "{{ ansible_date_time.iso8601 }}"
      user: "{{ ansible_user_id }}"
      version: "{{ deployment_version }}"
      host: "{{ inventory_hostname }}"
  
  pre_tasks:
    - name: Log deployment start
      lineinfile:
        path: /var/log/deployments/audit.log
        line: "{{ deployment_metadata | to_json }}"
        create: true
        mode: '0640'
      delegate_to: log_server
  
  tasks:
    - name: Deploy application
      copy:
        src: "app-{{ deployment_version }}.jar"
        dest: /opt/app/
  
  post_tasks:
    - name: Log deployment completion
      lineinfile:
        path: /var/log/deployments/audit.log
        line: "{{ deployment_metadata | combine({'status': 'SUCCESS'}) | to_json }}"
      delegate_to: log_server
```

## Hardening du Système

### Configuration Sécurisée

```yaml
---
- name: Security hardening
  hosts: all
  become: true
  
  tasks:
    - name: Configure secure sysctl parameters
      sysctl:
        name: "{{ item.name }}"
        value: "{{ item.value }}"
        state: present
        reload: true
      loop:
        - { name: 'net.ipv4.conf.all.accept_source_route', value: '0' }
        - { name: 'net.ipv4.conf.default.accept_source_route', value: '0' }
        - { name: 'net.ipv4.conf.all.accept_redirects', value: '0' }
        - { name: 'net.ipv4.conf.default.accept_redirects', value: '0' }
        - { name: 'net.ipv4.icmp_echo_ignore_broadcasts', value: '1' }
        - { name: 'net.ipv4.tcp_syncookies', value: '1' }
    
    - name: Disable unnecessary services
      systemd:
        name: "{{ item }}"
        enabled: false
        state: stopped
      loop:
        - avahi-daemon
        - cups
      ignore_errors: true
    
    - name: Configure firewall rules
      ufw:
        rule: "{{ item.rule }}"
        port: "{{ item.port | default(omit) }}"
        proto: "{{ item.proto | default(omit) }}"
      loop:
        - { rule: 'allow', port: '22', proto: 'tcp' }
        - { rule: 'allow', port: '80', proto: 'tcp' }
        - { rule: 'allow', port: '443', proto: 'tcp' }
        - { rule: 'deny', port: '23' }  # Deny telnet
```

## Checklist Sécurité ET Testabilité

**Avant chaque déploiement :**

### Secrets et Vault

- [ ] Tous les secrets sont dans des fichiers vault chiffrés
- [ ] Préfixe `vault_` utilisé pour toutes les variables sensibles
- [ ] Fichiers vault séparés par environnement
- [ ] Mot de passe vault stocké de manière sécurisée
- [ ] `.vault_pass*` dans .gitignore
- [ ] **✅ Tests avec vault password fonctionnent**

### Privilèges et Permissions

- [ ] `become` utilisé uniquement quand nécessaire
- [ ] Utilisateurs de service avec privilèges minimaux
- [ ] `no_log: true` pour tasks manipulant des secrets
- [ ] Permissions de fichiers restrictives (mode, owner, group)
- [ ] **✅ Tests vérifient les permissions**

### Validation et Tests

- [ ] Validation des variables critiques avec `assert`
- [ ] Templates validés avant déploiement
- [ ] Connexions SSH sécurisées
- [ ] Audit logging activé
- [ ] **✅ Tests de sécurité automatisés PASSÉS**
- [ ] **✅ Scan des secrets en clair PASSÉ (aucun trouvé)**
- [ ] **✅ Test d'idempotence PASSÉ**
- [ ] **✅ Dry-run avec vault PASSÉ**
- [ ] Revue de code effectuée

**🔒 RÈGLE : La sécurité DOIT être testée automatiquement**
