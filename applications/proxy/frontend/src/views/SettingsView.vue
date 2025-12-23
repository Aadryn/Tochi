<script setup lang="ts">
import { useSettingsStore } from '@/stores'
import { AppCard } from '@/components'

const settingsStore = useSettingsStore()

const refreshIntervalOptions = [
  { value: 10, label: '10 secondes' },
  { value: 30, label: '30 secondes' },
  { value: 60, label: '1 minute' },
  { value: 120, label: '2 minutes' },
  { value: 300, label: '5 minutes' },
]

function handleRefreshIntervalChange(event: Event): void {
  const select = event.target as HTMLSelectElement
  settingsStore.setRefreshInterval(parseInt(select.value))
}

function resetSettings(): void {
  if (confirm('Êtes-vous sûr de vouloir réinitialiser tous les paramètres ?')) {
    settingsStore.resetToDefaults()
  }
}
</script>

<template>
  <div class="settings-view">
    <div class="view-header">
      <h2 class="view-title">Paramètres</h2>
      <p class="view-description">
        Configurez l'apparence et le comportement de l'interface d'administration
      </p>
    </div>

    <div class="settings-grid">
      <!-- Apparence -->
      <AppCard title="Apparence">
        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-label">Thème</span>
            <span class="setting-description">
              Choisissez entre le mode clair et sombre
            </span>
          </div>
          <div class="setting-control">
            <button
              class="theme-btn"
              :class="{ active: settingsStore.settings.theme === 'light' }"
              @click="settingsStore.updateSetting('theme', 'light')"
            >
              <i class="pi pi-sun"></i>
              Clair
            </button>
            <button
              class="theme-btn"
              :class="{ active: settingsStore.settings.theme === 'dark' }"
              @click="settingsStore.updateSetting('theme', 'dark')"
            >
              <i class="pi pi-moon"></i>
              Sombre
            </button>
          </div>
        </div>

        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-label">Mode compact</span>
            <span class="setting-description">
              Réduit l'espacement pour afficher plus d'informations
            </span>
          </div>
          <div class="setting-control">
            <label class="toggle-switch">
              <input
                type="checkbox"
                :checked="settingsStore.settings.compactMode"
                @change="settingsStore.toggleCompactMode()"
              />
              <span class="toggle-slider"></span>
            </label>
          </div>
        </div>

        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-label">Langue</span>
            <span class="setting-description">
              Langue de l'interface utilisateur
            </span>
          </div>
          <div class="setting-control">
            <select
              :value="settingsStore.settings.language"
              @change="(e) => settingsStore.setLanguage((e.target as HTMLSelectElement).value as 'fr' | 'en')"
            >
              <option value="fr">Français</option>
              <option value="en">English</option>
            </select>
          </div>
        </div>
      </AppCard>

      <!-- Comportement -->
      <AppCard title="Comportement">
        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-label">Intervalle de rafraîchissement</span>
            <span class="setting-description">
              Fréquence de mise à jour des données du dashboard
            </span>
          </div>
          <div class="setting-control">
            <select
              :value="settingsStore.settings.refreshInterval"
              @change="handleRefreshIntervalChange"
            >
              <option
                v-for="option in refreshIntervalOptions"
                :key="option.value"
                :value="option.value"
              >
                {{ option.label }}
              </option>
            </select>
          </div>
        </div>

        <div class="setting-item">
          <div class="setting-info">
            <span class="setting-label">Notifications</span>
            <span class="setting-description">
              Afficher les notifications système
            </span>
          </div>
          <div class="setting-control">
            <label class="toggle-switch">
              <input
                type="checkbox"
                :checked="settingsStore.settings.showNotifications"
                @change="settingsStore.toggleNotifications()"
              />
              <span class="toggle-slider"></span>
            </label>
          </div>
        </div>
      </AppCard>

      <!-- À propos -->
      <AppCard title="À propos">
        <div class="about-content">
          <div class="app-info">
            <div class="app-logo">
              <i class="pi pi-box"></i>
            </div>
            <div class="app-details">
              <h3>LLM Proxy Admin</h3>
              <p>Version 1.0.0</p>
            </div>
          </div>

          <div class="info-list">
            <div class="info-item">
              <span class="info-label">Framework</span>
              <span class="info-value">Vue.js 3.5</span>
            </div>
            <div class="info-item">
              <span class="info-label">UI Library</span>
              <span class="info-value">PrimeVue 3.53</span>
            </div>
            <div class="info-item">
              <span class="info-label">State Management</span>
              <span class="info-value">Pinia 2.2</span>
            </div>
          </div>

          <div class="links">
            <a href="https://github.com" target="_blank" class="link-item">
              <i class="pi pi-github"></i>
              GitHub
            </a>
            <a href="/docs" class="link-item">
              <i class="pi pi-book"></i>
              Documentation
            </a>
          </div>
        </div>
      </AppCard>

      <!-- Actions -->
      <AppCard title="Actions">
        <div class="actions-list">
          <button class="action-btn" @click="resetSettings">
            <i class="pi pi-refresh"></i>
            <span>Réinitialiser les paramètres</span>
          </button>

          <button class="action-btn">
            <i class="pi pi-download"></i>
            <span>Exporter la configuration</span>
          </button>

          <button class="action-btn">
            <i class="pi pi-upload"></i>
            <span>Importer une configuration</span>
          </button>
        </div>
      </AppCard>
    </div>
  </div>
</template>

<style scoped>
.settings-view {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.view-header {
  margin-bottom: 0.5rem;
}

.view-title {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 600;
}

.view-description {
  margin: 0.25rem 0 0;
  color: var(--text-color-secondary);
}

.settings-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 1rem;
}

/* Setting Items */
.setting-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 0;
  border-bottom: 1px solid var(--surface-border);
}

.setting-item:last-child {
  border-bottom: none;
  padding-bottom: 0;
}

.setting-item:first-child {
  padding-top: 0;
}

.setting-info {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.setting-label {
  font-weight: 500;
}

.setting-description {
  font-size: 0.875rem;
  color: var(--text-color-secondary);
}

.setting-control {
  display: flex;
  gap: 0.5rem;
}

.setting-control select {
  padding: 0.5rem 0.75rem;
  border: 1px solid var(--surface-border);
  border-radius: 6px;
  background: var(--surface-card);
  color: var(--text-color);
  font-size: 0.875rem;
  min-width: 150px;
}

/* Theme Buttons */
.theme-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 1rem;
  background: var(--surface-100);
  border: 1px solid var(--surface-border);
  border-radius: 6px;
  color: var(--text-color-secondary);
  font-size: 0.875rem;
  cursor: pointer;
  transition: all 0.2s;
}

.theme-btn:hover {
  background: var(--surface-200);
}

.theme-btn.active {
  background: var(--primary-color);
  border-color: var(--primary-color);
  color: white;
}

/* Toggle Switch */
.toggle-switch {
  position: relative;
  display: inline-block;
  width: 48px;
  height: 26px;
}

.toggle-switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.toggle-slider {
  position: absolute;
  cursor: pointer;
  inset: 0;
  background: var(--surface-300);
  border-radius: 26px;
  transition: 0.3s;
}

.toggle-slider::before {
  position: absolute;
  content: '';
  height: 20px;
  width: 20px;
  left: 3px;
  bottom: 3px;
  background: white;
  border-radius: 50%;
  transition: 0.3s;
}

.toggle-switch input:checked + .toggle-slider {
  background: var(--primary-color);
}

.toggle-switch input:checked + .toggle-slider::before {
  transform: translateX(22px);
}

/* About Section */
.about-content {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.app-info {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.app-logo {
  width: 64px;
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--primary-100);
  border-radius: 12px;
}

.app-logo i {
  font-size: 2rem;
  color: var(--primary-color);
}

.app-details h3 {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 600;
}

.app-details p {
  margin: 0.25rem 0 0;
  color: var(--text-color-secondary);
  font-size: 0.875rem;
}

.info-list {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.info-item {
  display: flex;
  justify-content: space-between;
  font-size: 0.875rem;
}

.info-label {
  color: var(--text-color-secondary);
}

.info-value {
  font-weight: 500;
}

.links {
  display: flex;
  gap: 1rem;
}

.link-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  color: var(--primary-color);
  text-decoration: none;
  font-size: 0.875rem;
}

.link-item:hover {
  text-decoration: underline;
}

/* Actions */
.actions-list {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  width: 100%;
  padding: 0.75rem 1rem;
  background: var(--surface-ground);
  border: 1px solid var(--surface-border);
  border-radius: 6px;
  color: var(--text-color);
  font-size: 0.875rem;
  text-align: left;
  cursor: pointer;
  transition: all 0.2s;
}

.action-btn:hover {
  background: var(--surface-hover);
  border-color: var(--primary-color);
}

.action-btn i {
  color: var(--text-color-secondary);
}
</style>
