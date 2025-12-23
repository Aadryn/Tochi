<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useSettingsStore } from '@/stores'
import { setLocale, getCurrentLocale } from '@/locales'

const route = useRoute()
const router = useRouter()
const settingsStore = useSettingsStore()
const { t } = useI18n()

const showUserDropdown = ref(false)
const showLanguageDropdown = ref(false)
const currentLocale = ref(getCurrentLocale())

// Récupérer l'utilisateur depuis le localStorage
const user = computed(() => {
  const userStr = localStorage.getItem('auth_user')
  if (!userStr) return null
  try {
    return JSON.parse(userStr)
  } catch {
    return null
  }
})

const userName = computed(() => user.value?.name ?? t('header.user.profile'))

const userInitials = computed(() => {
  if (!user.value?.name) return 'U'
  const parts = user.value.name.split(' ')
  if (parts.length >= 2) {
    return `${parts[0][0]}${parts[1][0]}`.toUpperCase()
  }
  return parts[0].substring(0, 2).toUpperCase()
})

interface BreadcrumbItem {
  label: string
  route?: string
}

const pageTitle = computed(() => {
  const routePath = route.path
  if (routePath === '/') return t('dashboard.title')
  if (routePath === '/providers') return t('providers.title')
  if (routePath === '/tenants') return t('tenants.title')
  if (routePath === '/routes') return t('routes.title')
  if (routePath === '/monitoring') return t('monitoring.title')
  if (routePath === '/settings') return t('settings.title')
  return t('header.title')
})

const breadcrumbs = computed((): BreadcrumbItem[] => {
  const items: BreadcrumbItem[] = [{ label: t('dashboard.title'), route: '/' }]

  if (route.path !== '/') {
    const segments = route.path.split('/').filter(Boolean)
    let currentPath = ''

    for (const segment of segments) {
      currentPath += `/${segment}`
      const label = segment.charAt(0).toUpperCase() + segment.slice(1)
      items.push({ label, route: currentPath })
    }
  }

  return items
})

function toggleTheme(): void {
  settingsStore.toggleTheme()
}

function toggleUserDropdown(): void {
  showUserDropdown.value = !showUserDropdown.value
}

function closeUserDropdown(): void {
  showUserDropdown.value = false
}

function toggleLanguageDropdown(): void {
  showLanguageDropdown.value = !showLanguageDropdown.value
}

function closeLanguageDropdown(): void {
  showLanguageDropdown.value = false
}

function changeLanguage(locale: 'fr' | 'en'): void {
  setLocale(locale)
  currentLocale.value = locale
  closeLanguageDropdown()
}

function handleLogout(): void {
  localStorage.removeItem('auth_token')
  localStorage.removeItem('auth_refresh_token')
  localStorage.removeItem('auth_user')
  router.push({ name: 'login' })
}
</script>

<template>
  <header class="app-header">
    <div class="header-left">
      <h1 class="page-title">{{ pageTitle }}</h1>
      <nav class="breadcrumbs" v-if="breadcrumbs.length > 1">
        <span
          v-for="(item, index) in breadcrumbs"
          :key="item.label"
          class="breadcrumb-item"
        >
          <router-link
            v-if="item.route && index < breadcrumbs.length - 1"
            :to="item.route"
            class="breadcrumb-link"
          >
            {{ item.label }}
          </router-link>
          <span v-else class="breadcrumb-current">{{ item.label }}</span>
          <i
            v-if="index < breadcrumbs.length - 1"
            class="pi pi-angle-right breadcrumb-separator"
          ></i>
        </span>
      </nav>
    </div>

    <div class="header-right">
      <!-- Language Selector -->
      <div class="language-menu" @mouseleave="closeLanguageDropdown" data-testid="language-selector">
        <button 
          class="icon-btn" 
          @click="toggleLanguageDropdown"
          :title="t('header.language.' + (currentLocale === 'fr' ? 'french' : 'english'))"
        >
          <i :class="currentLocale === 'fr' ? 'pi pi-flag-fill' : 'pi pi-flag'"></i>
          <span class="language-label">{{ currentLocale.toUpperCase() }}</span>
        </button>
        
        <div v-if="showLanguageDropdown" class="language-dropdown">
          <button 
            class="dropdown-item" 
            :class="{ active: currentLocale === 'fr' }"
            @click="changeLanguage('fr')"
            data-testid="language-fr"
          >
            <i class="pi pi-flag-fill"></i>
            <span>{{ t('header.language.french') }}</span>
            <i v-if="currentLocale === 'fr'" class="pi pi-check"></i>
          </button>
          <button 
            class="dropdown-item"
            :class="{ active: currentLocale === 'en' }"
            @click="changeLanguage('en')"
            data-testid="language-en"
          >
            <i class="pi pi-flag"></i>
            <span>{{ t('header.language.english') }}</span>
            <i v-if="currentLocale === 'en'" class="pi pi-check"></i>
          </button>
        </div>
      </div>

      <!-- Theme Toggle -->
      <button
        data-testid="theme-toggle"
        class="icon-btn"
        @click="toggleTheme"
        :title="settingsStore.settings.theme === 'light' ? t('header.theme.dark') : t('header.theme.light')"
      >
        <i
          :class="
            settingsStore.settings.theme === 'light' ? 'pi pi-moon' : 'pi pi-sun'
          "
        ></i>
      </button>

      <button class="icon-btn" title="Notifications">
        <i class="pi pi-bell"></i>
        <span class="notification-badge">3</span>
      </button>

      <div class="user-menu" @mouseleave="closeUserDropdown">
        <button class="user-btn" @click="toggleUserDropdown">
          <span class="user-avatar">{{ userInitials }}</span>
          <span class="user-name">{{ userName }}</span>
          <i class="pi pi-angle-down"></i>
        </button>
        
        <div v-if="showUserDropdown" class="user-dropdown">
          <div class="dropdown-header">
            <span class="dropdown-name">{{ userName }}</span>
            <span class="dropdown-email">{{ user?.email }}</span>
          </div>
          <div class="dropdown-divider"></div>
          <router-link to="/settings" class="dropdown-item" @click="closeUserDropdown">
            <i class="pi pi-cog"></i>
            <span>{{ t('settings.title') }}</span>
          </router-link>
          <button class="dropdown-item logout" @click="handleLogout">
            <i class="pi pi-sign-out"></i>
            <span>{{ t('header.user.logout') }}</span>
          </button>
        </div>
      </div>
    </div>
  </header>
</template>

<style scoped>
.app-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.5rem;
  background: var(--surface-card);
  border-bottom: 1px solid var(--surface-border);
  min-height: 60px;
}

.header-left {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.page-title {
  margin: 0;
  font-size: 1.5rem;
  font-weight: 600;
  color: var(--text-color);
}

.breadcrumbs {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.875rem;
}

.breadcrumb-item {
  display: flex;
  align-items: center;
  gap: 0.25rem;
}

.breadcrumb-link {
  color: var(--text-color-secondary);
  text-decoration: none;
  transition: color 0.2s;
}

.breadcrumb-link:hover {
  color: var(--primary-color);
}

.breadcrumb-current {
  color: var(--text-color);
  font-weight: 500;
}

.breadcrumb-separator {
  font-size: 0.75rem;
  color: var(--text-color-secondary);
}

.header-right {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.icon-btn {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  background: transparent;
  border: none;
  border-radius: 8px;
  color: var(--text-color-secondary);
  cursor: pointer;
  transition: all 0.2s;
}

.icon-btn:hover {
  background: var(--surface-hover);
  color: var(--text-color);
}

.icon-btn i {
  font-size: 1.1rem;
}

.notification-badge {
  position: absolute;
  top: 6px;
  right: 6px;
  min-width: 16px;
  height: 16px;
  padding: 0 4px;
  background: var(--red-500);
  color: white;
  font-size: 0.625rem;
  font-weight: 600;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.user-menu {
  margin-left: 0.5rem;
  position: relative;
}

.user-btn {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.5rem 0.75rem;
  background: transparent;
  border: 1px solid var(--surface-border);
  border-radius: 8px;
  color: var(--text-color);
  cursor: pointer;
  transition: all 0.2s;
}

.user-btn:hover {
  background: var(--surface-hover);
  border-color: var(--surface-hover);
}

.user-avatar {
  width: 28px;
  height: 28px;
  background: var(--primary-color);
  color: white;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.75rem;
  font-weight: 600;
}

.user-name {
  font-weight: 500;
}

.user-btn i:last-child {
  font-size: 0.75rem;
  color: var(--text-color-secondary);
}

.user-dropdown {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  min-width: 220px;
  background: var(--surface-card);
  border: 1px solid var(--surface-border);
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  z-index: 100;
  overflow: hidden;
}

.dropdown-header {
  padding: 0.75rem 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.125rem;
}

.dropdown-name {
  font-weight: 600;
  color: var(--text-color);
}

.dropdown-email {
  font-size: 0.8125rem;
  color: var(--text-color-secondary);
}

.dropdown-divider {
  height: 1px;
  background: var(--surface-border);
  margin: 0;
}

.dropdown-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  color: var(--text-color);
  text-decoration: none;
  transition: background 0.15s;
  border: none;
  background: none;
  width: 100%;
  cursor: pointer;
  font-size: 0.875rem;
}

.dropdown-item:hover {
  background: var(--surface-hover);
}

.dropdown-item i {
  font-size: 1rem;
  color: var(--text-color-secondary);
}

.dropdown-item.logout {
  color: var(--red-500);
}

.dropdown-item.logout i {
  color: var(--red-500);
}

/* Language Selector Styles */
.language-menu {
  position: relative;
}

.language-label {
  margin-left: 0.25rem;
  font-size: 0.75rem;
  font-weight: 600;
}

.icon-btn:has(.language-label) {
  width: auto;
  padding: 0 0.75rem;
  gap: 0.25rem;
}

.language-dropdown {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  min-width: 160px;
  background: var(--surface-card);
  border: 1px solid var(--surface-border);
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  z-index: 100;
  overflow: hidden;
}

.language-dropdown .dropdown-item {
  justify-content: space-between;
}

.language-dropdown .dropdown-item.active {
  background: var(--surface-hover);
  color: var(--primary-color);
}

.language-dropdown .dropdown-item.active i:first-of-type {
  color: var(--primary-color);
}

.language-dropdown .dropdown-item .pi-check {
  font-size: 0.875rem;
  color: var(--primary-color);
}
</style>
