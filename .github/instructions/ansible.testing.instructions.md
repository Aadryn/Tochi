---
description: Testing, validation, Molecule, ansible-lint, and CI/CD integration
name: Ansible_Testing_CICD
applyTo: "**/*.{yml,yaml}"
---

# Ansible Testing & CI/CD - Guide Expert

## 🎯 Actions Obligatoires (Mandatory)

**Tests à TOUJOURS effectuer avant tout commit/déploiement :**

1. ✅ **Validation syntaxique OBLIGATOIRE** : `ansible-playbook --syntax-check`
2. ✅ **Linting OBLIGATOIRE** : `ansible-lint` ZÉRO erreurs (warnings acceptables)
3. ✅ **Dry-run OBLIGATOIRE** : `ansible-playbook --check --diff` DOIT passer sans erreurs
4. ✅ **Test idempotence OBLIGATOIRE** : Exécuter 2 fois, la 2ème DOIT avoir changed=0
5. ✅ **Tests Molecule OBLIGATOIRES** : TOUS les tests doivent passer pour chaque rôle
6. ✅ **Validation YAML** : `yamllint` pour la cohérence du format
7. ✅ **Test en environnement isolé** : TOUJOURS tester avant production
8. ✅ **Rollback testé** : Le rollback DOIT être testé et fonctionnel

**RÈGLE ABSOLUE : Aucun code ne peut être mergé sans que TOUS ces tests soient au vert ✅**

## Validation Syntaxique

### Vérification de Base

```bash
# Valider la syntaxe d'un playbook
ansible-playbook playbook.yml --syntax-check

# Valider tous les playbooks
for file in playbooks/*.yml; do
  echo "Checking $file..."
  ansible-playbook "$file" --syntax-check
done

# Valider avec un inventaire spécifique
ansible-playbook -i inventories/production playbook.yml --syntax-check
```

### Script de Validation Automatique

```bash
#!/bin/bash
# validate.sh - Script de validation complète

set -e

echo "=== Ansible Validation Suite ==="

# 1. Syntax check
echo "1. Checking syntax..."
ansible-playbook playbooks/*.yml --syntax-check

# 2. YAML lint
echo "2. Running yamllint..."
yamllint .

# 3. Ansible lint
echo "3. Running ansible-lint..."
ansible-lint playbooks/ roles/

# 4. Inventory validation
echo "4. Validating inventory..."
ansible-inventory -i inventories/production --list > /dev/null

echo "✅ All validations passed!"
```

## Ansible-lint

### Installation et Configuration

```bash
# Installation
pip install ansible-lint

# Exécution
ansible-lint playbook.yml
ansible-lint roles/
ansible-lint .  # Tout le projet
```

### Configuration .ansible-lint

```yaml
# .ansible-lint
---
# Exclure certains fichiers/dossiers
exclude_paths:
  - .cache/
  - .github/
  - molecule/
  - venv/

# Profil de règles (production, safety, shared, moderate, min)
profile: production

# Règles à ignorer
skip_list:
  - yaml[line-length]  # Ignorer les lignes trop longues
  - name[casing]       # Ignorer la casse des noms

# Avertissements comme erreurs
strict: false

# Tags à appliquer
tags: []

# Règles personnalisées
warn_list:
  - experimental
  - role-name

# Variables Jinja autorisées
loop_var_prefix: "^(__|{molecule_|{role_)"

# Configuration YAML
yamllint_config_file: .yamllint
```

### Corriger les Erreurs Communes

```yaml
---
# ❌ Erreur: name[missing] - Task sans nom
- command: echo "hello"

# ✅ Correction
- name: Display greeting message
  command: echo "hello"
  changed_when: false

# ❌ Erreur: risky-file-permissions - Permissions non définies
- name: Create file
  copy:
    content: "data"
    dest: /tmp/file

# ✅ Correction
- name: Create file with explicit permissions
  copy:
    content: "data"
    dest: /tmp/file
    mode: '0644'
    owner: root
    group: root

# ❌ Erreur: package-latest - Utilisation de state: latest
- name: Install nginx
  apt:
    name: nginx
    state: latest

# ✅ Correction
- name: Ensure nginx is installed
  apt:
    name: nginx
    state: present

# ❌ Erreur: command-instead-of-module - Module disponible
- name: Create directory
  command: mkdir -p /opt/app

# ✅ Correction
- name: Ensure directory exists
  file:
    path: /opt/app
    state: directory
```

## YAMLlint

### Configuration .yamllint

```yaml
# .yamllint
---
extends: default

rules:
  line-length:
    max: 120
    level: warning
  
  indentation:
    spaces: 2
    indent-sequences: true
  
  comments:
    min-spaces-from-content: 1
  
  comments-indentation: {}
  
  truthy:
    allowed-values: ['true', 'false', 'yes', 'no']
    check-keys: false
  
  braces:
    max-spaces-inside: 1
  
  brackets:
    max-spaces-inside: 1

ignore: |
  .cache/
  .github/
  venv/
  *.pyc
```

## Dry-Run et Check Mode

### Exécution en Mode Check

```bash
# Mode check (dry-run) - Aucune modification
ansible-playbook playbook.yml --check

# Mode check avec diff
ansible-playbook playbook.yml --check --diff

# Mode check sur un groupe spécifique
ansible-playbook -i inventories/production playbook.yml --check --limit webservers

# Mode check avec verbose
ansible-playbook playbook.yml --check --diff -vv
```

### Workflow de Test STRICT - OBLIGATOIRE

**CE workflow DOIT être suivi pour CHAQUE changement :**

```bash
#!/bin/bash
# strict-test-workflow.sh
# Workflow de test COMPLET - AUCUNE étape ne peut être sautée

set -e  # Arrêter au premier échec

PLAYBOOK="$1"
INVENTORY="${2:-inventories/test}"

if [ -z "$PLAYBOOK" ]; then
  echo "Usage: $0 <playbook.yml> [inventory]"
  exit 1
fi

echo "======================================================="
echo "   STRICT TEST WORKFLOW - ALL TESTS MANDATORY"
echo "======================================================="

# ÉTAPE 1 : Validation syntaxique (OBLIGATOIRE)
echo ""
echo "[1/8] ✓ Syntax validation..."
ansible-playbook "$PLAYBOOK" --syntax-check || {
  echo "❌ FAILED: Syntax errors detected"
  exit 1
}
echo "✅ PASSED: Syntax is valid"

# ÉTAPE 2 : YAML Lint (OBLIGATOIRE)
echo ""
echo "[2/8] ✓ YAML linting..."
yamllint "$PLAYBOOK" || {
  echo "❌ FAILED: YAML format errors"
  exit 1
}
echo "✅ PASSED: YAML format is valid"

# ÉTAPE 3 : Ansible Lint (OBLIGATOIRE)
echo ""
echo "[3/8] ✓ Ansible linting..."
ansible-lint "$PLAYBOOK" || {
  echo "❌ FAILED: Ansible lint errors detected"
  exit 1
}
echo "✅ PASSED: Ansible lint passed"

# ÉTAPE 4 : Check Mode / Dry-Run (OBLIGATOIRE)
echo ""
echo "[4/8] ✓ Dry-run (check mode)..."
ansible-playbook -i "$INVENTORY" "$PLAYBOOK" --check --diff > /tmp/check_run.log 2>&1 || {
  echo "❌ FAILED: Dry-run encountered errors"
  cat /tmp/check_run.log
  exit 1
}
echo "✅ PASSED: Dry-run successful"

# ÉTAPE 5 : Première Exécution (OBLIGATOIRE)
echo ""
echo "[5/8] ✓ First execution..."
ansible-playbook -i "$INVENTORY" "$PLAYBOOK" | tee /tmp/first_run.log || {
  echo "❌ FAILED: First execution failed"
  exit 1
}
echo "✅ PASSED: First execution successful"

# ÉTAPE 6 : Test d'Idempotence (OBLIGATOIRE - CRITIQUE)
echo ""
echo "[6/8] ✓ Idempotence test (CRITICAL)..."
ansible-playbook -i "$INVENTORY" "$PLAYBOOK" | tee /tmp/second_run.log

if grep -q "changed=0" /tmp/second_run.log && ! grep -q "failed=[1-9]" /tmp/second_run.log; then
  echo "✅ PASSED: Playbook is IDEMPOTENT (0 changes on second run)"
else
  echo "❌ FAILED: Playbook is NOT IDEMPOTENT"
  echo "Changes detected on second run - THIS IS CRITICAL!"
  echo ""
  echo "Comparing first and second run:"
  diff /tmp/first_run.log /tmp/second_run.log || true
  exit 1
fi

# ÉTAPE 7 : Vérification Fonctionnelle (OBLIGATOIRE)
echo ""
echo "[7/8] ✓ Functional verification..."
# Ajouter vos tests fonctionnels ici
# Exemple: vérifier que le service répond
if command -v curl &> /dev/null; then
  echo "Checking service health..."
  # curl -f http://localhost:8080/health || exit 1
fi
echo "✅ PASSED: Functional tests passed"

# ÉTAPE 8 : Cleanup et Validation Finale (OBLIGATOIRE)
echo ""
echo "[8/8] ✓ Final validation..."
rm -f /tmp/check_run.log /tmp/first_run.log /tmp/second_run.log
echo "✅ PASSED: Cleanup complete"

echo ""
echo "======================================================="
echo "   ✅✅✅ ALL TESTS PASSED ✅✅✅"
echo "   Playbook is ready for production deployment"
echo "======================================================="
```

## Test d'Idempotence - RÈGLE D'OR

**RÈGLE ABSOLUE : Si le test d'idempotence échoue, le code est INACCEPTABLE**

```bash
#!/bin/bash
# idempotence-test.sh - Version stricte

PLAYBOOK="$1"
INVENTORY="${2:-inventories/test}"

if [ -z "$PLAYBOOK" ]; then
  echo "Usage: $0 <playbook.yml> [inventory]"
  exit 1
fi

echo "======================================"
echo "   IDEMPOTENCE TEST - CRITICAL"
echo "======================================"

# Première exécution
echo ""
echo "First run..."
ansible-playbook -i "$INVENTORY" "$PLAYBOOK" | tee /tmp/first_run.log
FIRST_RC=${PIPESTATUS[0]}

if [ $FIRST_RC -ne 0 ]; then
  echo "❌ FAILED: First execution failed"
  exit 1
fi

# Deuxième exécution (TEST CRITIQUE)
echo ""
echo "Second run (MUST have 0 changes)..."
ansible-playbook -i "$INVENTORY" "$PLAYBOOK" | tee /tmp/second_run.log
SECOND_RC=${PIPESTATUS[0]}

if [ $SECOND_RC -ne 0 ]; then
  echo "❌ FAILED: Second execution failed"
  exit 1
fi

# Vérification STRICTE
echo ""
echo "Analyzing results..."

CHANGED=$(grep -oP 'changed=\K[0-9]+' /tmp/second_run.log | head -1)
FAILED=$(grep -oP 'failed=\K[0-9]+' /tmp/second_run.log | head -1)

if [ "$CHANGED" = "0" ] && [ "$FAILED" = "0" ]; then
  echo ""
  echo "✅✅✅ IDEMPOTENCE TEST PASSED ✅✅✅"
  echo "Second run: changed=0, failed=0"
  echo "Playbook is IDEMPOTENT and PRODUCTION-READY"
  exit 0
else
  echo ""
  echo "❌❌❌ IDEMPOTENCE TEST FAILED ❌❌❌"
  echo "Second run: changed=$CHANGED, failed=$FAILED"
  echo ""
  echo "THIS IS CRITICAL! The playbook is NOT idempotent."
  echo "This MUST be fixed before any production deployment."
  echo ""
  echo "Differences between first and second run:"
  diff /tmp/first_run.log /tmp/second_run.log || true
  exit 1
fi
```

## Molecule - Framework de Test

### Installation

```bash
# Installation de Molecule avec Docker
pip install molecule molecule-plugins[docker]

# Ou avec Podman
pip install molecule molecule-plugins[podman]

# Ou avec Vagrant
pip install molecule molecule-plugins[vagrant]
```

### Initialiser un Rôle avec Molecule

```bash
# Créer un nouveau rôle avec Molecule
molecule init role my_role --driver-name docker

# Structure créée:
# my_role/
# ├── defaults/
# ├── handlers/
# ├── meta/
# ├── molecule/
# │   └── default/
# │       ├── converge.yml
# │       ├── molecule.yml
# │       └── verify.yml
# ├── tasks/
# ├── templates/
# └── vars/

# Ajouter Molecule à un rôle existant
cd roles/existing_role
molecule init scenario --driver-name docker
```

### Configuration Molecule

```yaml
# molecule/default/molecule.yml
---
dependency:
  name: galaxy
  options:
    requirements-file: requirements.yml

driver:
  name: docker

platforms:
  - name: ubuntu-22.04
    image: ubuntu:22.04
    pre_build_image: true
    privileged: true
    command: /lib/systemd/systemd
    volumes:
      - /sys/fs/cgroup:/sys/fs/cgroup:rw
    cgroupns_mode: host
    capabilities:
      - SYS_ADMIN
  
  - name: debian-12
    image: debian:12
    pre_build_image: true
    privileged: true
    command: /lib/systemd/systemd
    volumes:
      - /sys/fs/cgroup:/sys/fs/cgroup:rw
    cgroupns_mode: host
  
  - name: centos-stream-9
    image: quay.io/centos/centos:stream9
    pre_build_image: true
    privileged: true
    command: /usr/sbin/init
    volumes:
      - /sys/fs/cgroup:/sys/fs/cgroup:rw

provisioner:
  name: ansible
  config_options:
    defaults:
      callbacks_enabled: profile_tasks
      fact_caching: jsonfile
      fact_caching_connection: /tmp/molecule_facts
  inventory:
    group_vars:
      all:
        nginx_port: 8080
        nginx_worker_processes: 2
  playbooks:
    converge: converge.yml
    verify: verify.yml

verifier:
  name: ansible

scenario:
  test_sequence:
    - dependency
    - cleanup
    - destroy
    - syntax
    - create
    - prepare
    - converge
    - idempotence
    - side_effect
    - verify
    - cleanup
    - destroy
```

### Playbook de Convergence

```yaml
# molecule/default/converge.yml
---
- name: Converge
  hosts: all
  become: true
  
  pre_tasks:
    - name: Update apt cache (Debian/Ubuntu)
      apt:
        update_cache: true
        cache_valid_time: 3600
      when: ansible_os_family == 'Debian'
  
  roles:
    - role: my_role
      vars:
        my_role_port: 8080
        my_role_enable_ssl: false
```

### Tests de Vérification

```yaml
# molecule/default/verify.yml
---
- name: Verify
  hosts: all
  gather_facts: false
  
  tasks:
    - name: Check if service is running
      systemd:
        name: nginx
      register: service_status
      failed_when: service_status.status.ActiveState != 'active'
    
    - name: Check if service is enabled
      systemd:
        name: nginx
      register: service_status
      failed_when: service_status.status.UnitFileState != 'enabled'
    
    - name: Verify port is listening
      wait_for:
        port: 8080
        timeout: 5
      register: port_check
    
    - name: Verify application endpoint responds
      uri:
        url: http://localhost:8080/
        status_code: 200
        timeout: 5
      register: http_check
    
    - name: Check configuration file exists
      stat:
        path: /etc/nginx/nginx.conf
      register: config_file
      failed_when: not config_file.stat.exists
    
    - name: Verify configuration file permissions
      stat:
        path: /etc/nginx/nginx.conf
      register: config_file
      failed_when: config_file.stat.mode != '0644'
    
    - name: Check log directory exists
      stat:
        path: /var/log/nginx
      register: log_dir
      failed_when: not log_dir.stat.exists or not log_dir.stat.isdir
```

### Playbook de Préparation

```yaml
# molecule/default/prepare.yml
---
- name: Prepare
  hosts: all
  become: true
  
  tasks:
    - name: Install Python dependencies
      package:
        name:
          - python3
          - python3-pip
        state: present
    
    - name: Create test user
      user:
        name: testuser
        state: present
    
    - name: Create required directories
      file:
        path: "{{ item }}"
        state: directory
        mode: '0755'
      loop:
        - /opt/test
        - /var/test
```

### Commandes Molecule

```bash
# Cycle complet de test
molecule test

# Étapes individuelles
molecule create         # Créer les conteneurs
molecule converge       # Appliquer le rôle
molecule verify         # Exécuter les tests
molecule destroy        # Détruire les conteneurs

# Test d'idempotence
molecule idempotence

# Se connecter au conteneur
molecule login
molecule login -h ubuntu-22.04

# Voir les logs
molecule --debug test

# Tester un scenario spécifique
molecule test -s custom_scenario

# Lister les scenarios
molecule list
```

### Scenarios Multiples

```yaml
# molecule/production/molecule.yml
---
# Scenario spécifique pour tester la configuration production
dependency:
  name: galaxy

driver:
  name: docker

platforms:
  - name: prod-ubuntu
    image: ubuntu:22.04
    pre_build_image: true

provisioner:
  name: ansible
  inventory:
    group_vars:
      all:
        environment: production
        nginx_worker_processes: 8
        nginx_worker_connections: 4096

verifier:
  name: ansible
```

```bash
# Tester le scenario production
molecule test -s production
```

## CI/CD Integration

### GitHub Actions

```yaml
# .github/workflows/ansible-ci.yml
---
name: Ansible CI

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  lint:
    name: Lint
    runs-on: ubuntu-latest
    
    steps:
      - name: Check out code
        uses: actions/checkout@v3
      
      - name: Set up Python
        uses: actions/setup-python@v4
        with:
          python-version: '3.11'
      
      - name: Install dependencies
        run: |
          python -m pip install --upgrade pip
          pip install ansible ansible-lint yamllint
      
      - name: Run yamllint
        run: yamllint .
      
      - name: Run ansible-lint
        run: ansible-lint
      
      - name: Syntax check
        run: |
          for playbook in playbooks/*.yml; do
            ansible-playbook "$playbook" --syntax-check
          done
  
  molecule:
    name: Molecule Test
    runs-on: ubuntu-latest
    strategy:
      matrix:
        role:
          - nginx
          - postgresql
          - application
    
    steps:
      - name: Check out code
        uses: actions/checkout@v3
      
      - name: Set up Python
        uses: actions/setup-python@v4
        with:
          python-version: '3.11'
      
      - name: Install dependencies
        run: |
          python -m pip install --upgrade pip
          pip install molecule molecule-plugins[docker] ansible
      
      - name: Run Molecule tests
        run: |
          cd roles/${{ matrix.role }}
          molecule test
        env:
          MOLECULE_NO_LOG: false
  
  deploy-staging:
    name: Deploy to Staging
    needs: [lint, molecule]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    
    steps:
      - name: Check out code
        uses: actions/checkout@v3
      
      - name: Set up Python
        uses: actions/setup-python@v4
        with:
          python-version: '3.11'
      
      - name: Install Ansible
        run: |
          python -m pip install --upgrade pip
          pip install ansible
      
      - name: Configure SSH
        run: |
          mkdir -p ~/.ssh
          echo "${{ secrets.SSH_PRIVATE_KEY }}" > ~/.ssh/id_rsa
          chmod 600 ~/.ssh/id_rsa
      
      - name: Deploy to staging
        run: |
          ansible-playbook \
            -i inventories/staging \
            playbooks/deploy.yml \
            --vault-password-file <(echo "${{ secrets.VAULT_PASSWORD }}")
        env:
          ANSIBLE_HOST_KEY_CHECKING: False
  
  deploy-production:
    name: Deploy to Production
    needs: [lint, molecule]
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    environment: production
    
    steps:
      - name: Check out code
        uses: actions/checkout@v3
      
      - name: Deploy to production
        run: |
          # Déploiement production avec approbation manuelle
          ansible-playbook \
            -i inventories/production \
            playbooks/deploy.yml \
            --vault-password-file <(echo "${{ secrets.VAULT_PASSWORD }}")
```

### GitLab CI

```yaml
# .gitlab-ci.yml
---
stages:
  - validate
  - test
  - deploy

variables:
  ANSIBLE_FORCE_COLOR: "true"
  PIP_CACHE_DIR: "$CI_PROJECT_DIR/.cache/pip"

cache:
  paths:
    - .cache/pip

# =============================================================================
# VALIDATION
# =============================================================================
syntax-check:
  stage: validate
  image: python:3.11
  before_script:
    - pip install ansible
  script:
    - |
      for playbook in playbooks/*.yml; do
        ansible-playbook "$playbook" --syntax-check
      done
  only:
    - branches
    - merge_requests

yaml-lint:
  stage: validate
  image: python:3.11
  before_script:
    - pip install yamllint
  script:
    - yamllint .
  only:
    - branches
    - merge_requests

ansible-lint:
  stage: validate
  image: python:3.11
  before_script:
    - pip install ansible ansible-lint
  script:
    - ansible-lint
  allow_failure: true
  only:
    - branches
    - merge_requests

# =============================================================================
# TESTS
# =============================================================================
molecule-test:
  stage: test
  image: python:3.11
  services:
    - docker:dind
  variables:
    DOCKER_HOST: tcp://docker:2375
    DOCKER_TLS_CERTDIR: ""
  before_script:
    - pip install molecule molecule-plugins[docker] ansible
  script:
    - |
      for role in roles/*; do
        if [ -d "$role/molecule" ]; then
          echo "Testing role: $(basename $role)"
          cd "$role"
          molecule test
          cd -
        fi
      done
  only:
    - branches
    - merge_requests

# =============================================================================
# DÉPLOIEMENT
# =============================================================================
deploy-staging:
  stage: deploy
  image: python:3.11
  before_script:
    - pip install ansible
    - mkdir -p ~/.ssh
    - echo "$SSH_PRIVATE_KEY" > ~/.ssh/id_rsa
    - chmod 600 ~/.ssh/id_rsa
    - echo "$VAULT_PASSWORD" > .vault_pass
    - chmod 600 .vault_pass
  script:
    - |
      ansible-playbook \
        -i inventories/staging \
        playbooks/deploy.yml \
        --vault-password-file .vault_pass
  after_script:
    - rm -f .vault_pass
  only:
    - develop
  environment:
    name: staging
    url: https://staging.example.com

deploy-production:
  stage: deploy
  image: python:3.11
  before_script:
    - pip install ansible
    - mkdir -p ~/.ssh
    - echo "$SSH_PRIVATE_KEY" > ~/.ssh/id_rsa
    - chmod 600 ~/.ssh/id_rsa
    - echo "$VAULT_PASSWORD" > .vault_pass
    - chmod 600 .vault_pass
  script:
    - |
      ansible-playbook \
        -i inventories/production \
        playbooks/deploy.yml \
        --vault-password-file .vault_pass \
        --check --diff  # Dry-run d'abord
      
      # Attendre confirmation
      read -p "Continue with actual deployment? (yes/no) " confirm
      if [ "$confirm" = "yes" ]; then
        ansible-playbook \
          -i inventories/production \
          playbooks/deploy.yml \
          --vault-password-file .vault_pass
      fi
  after_script:
    - rm -f .vault_pass
  only:
    - main
  when: manual
  environment:
    name: production
    url: https://production.example.com
```

## Tests Unitaires avec Pytest

```python
# tests/test_inventory.py
import pytest
import yaml

def test_inventory_structure():
    """Test que l'inventaire a la structure requise"""
    with open('inventories/production/hosts') as f:
        inventory = f.read()
    
    assert '[webservers]' in inventory
    assert '[dbservers]' in inventory
    assert '[production:children]' in inventory

def test_group_vars_exist():
    """Test que les group_vars existent"""
    import os
    assert os.path.exists('inventories/production/group_vars/all.yml')
    assert os.path.exists('inventories/production/group_vars/webservers.yml')

def test_vault_files_encrypted():
    """Test que les fichiers vault sont chiffrés"""
    with open('inventories/production/group_vars/all/vault.yml') as f:
        content = f.read()
    
    assert '$ANSIBLE_VAULT' in content

def test_no_secrets_in_clear():
    """Test qu'il n'y a pas de secrets en clair"""
    import os
    for root, dirs, files in os.walk('inventories'):
        for file in files:
            if file.endswith(('.yml', '.yaml')) and 'vault' not in file:
                path = os.path.join(root, file)
                with open(path) as f:
                    content = f.read().lower()
                    assert 'password:' not in content or '{{' in content
```

## Checklist Testing - AUCUNE EXCEPTION

### Tests OBLIGATOIRES (Bloquants pour Merge)

- [ ] **✅ `ansible-playbook --syntax-check` PASSÉ**
- [ ] **✅ `ansible-lint` ZÉRO erreurs (warnings OK)**
- [ ] **✅ `yamllint` PASSÉ**
- [ ] **✅ `ansible-playbook --check --diff` PASSÉ sans erreurs**
- [ ] **✅ Test d'idempotence PASSÉ (2ème run = changed=0)**
- [ ] **✅ Tests Molecule créés et TOUS PASSANTS**
- [ ] **✅ Testé en environnement isolé/test**
- [ ] **✅ Rollback testé et fonctionnel**

### CI/CD (Obligatoire)

- [ ] Pipeline CI/CD configuré
- [ ] Tests automatiques sur CHAQUE PR
- [ ] Merge bloqué si tests échouent
- [ ] Test d'idempotence dans la CI
- [ ] Notifications en cas d'échec

### Sécurité (Obligatoire)

- [ ] Validation des secrets (vault chiffrés)
- [ ] Aucun secret en clair détecté
- [ ] Scan de sécurité passé

### Documentation (Obligatoire)

- [ ] Documentation des tests à jour
- [ ] README avec instructions de test
- [ ] Procédure de rollback documentée

### Validation Finale

- [ ] **✅ TOUS les tests ci-dessus sont au vert**
- [ ] **✅ Code review approuvé**
- [ ] **✅ Tests d'idempotence : changed=0 sur 2ème run**
- [ ] **✅ Prêt pour production**

**🚨 RAPPEL : Un seul test échoué = Code NON MERGEABLE 🚨**
