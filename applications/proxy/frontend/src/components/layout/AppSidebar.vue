<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useSettingsStore } from '@/stores'

const router = useRouter()
const route = useRoute()
const settingsStore = useSettingsStore()

const sidebarCollapsed = ref(false)

interface NavItem {
  label: string
  icon: string
  route: string
}

const navItems: NavItem[] = [
  { label: 'Tableau de bord', icon: 'pi pi-home', route: '/' },
  { label: 'Providers', icon: 'pi pi-server', route: '/providers' },
  { label: 'Tenants', icon: 'pi pi-users', route: '/tenants' },
  { label: 'Routes', icon: 'pi pi-directions', route: '/routes' },
  { label: 'Monitoring', icon: 'pi pi-chart-line', route: '/monitoring' },
  { label: 'Paramètres', icon: 'pi pi-cog', route: '/settings' },
]

const isActiveRoute = (itemRoute: string): boolean => {
  if (itemRoute === '/') {
    return route.path === '/'
  }
  return route.path.startsWith(itemRoute)
}

const sidebarWidth = computed(() => (sidebarCollapsed.value ? '60px' : '240px'))

function navigateTo(itemRoute: string): void {
  router.push(itemRoute)
}

function toggleSidebar(): void {
  sidebarCollapsed.value = !sidebarCollapsed.value
}
</script>

<template>
  <aside
    class="sidebar"
    :class="{ collapsed: sidebarCollapsed }"
    :style="{ width: sidebarWidth }"
  >
    <!-- Logo -->
    <div class="sidebar-header">
      <div class="logo" v-if="!sidebarCollapsed">
        <i class="pi pi-box logo-icon"></i>
        <span class="logo-text">LLM Proxy</span>
      </div>
      <button
        class="toggle-btn"
        @click="toggleSidebar"
        :title="sidebarCollapsed ? 'Développer' : 'Réduire'"
      >
        <i :class="sidebarCollapsed ? 'pi pi-angle-right' : 'pi pi-angle-left'"></i>
      </button>
    </div>

    <!-- Navigation -->
    <nav class="sidebar-nav">
      <ul>
        <li
          v-for="item in navItems"
          :key="item.route"
          :class="{ active: isActiveRoute(item.route) }"
          @click="navigateTo(item.route)"
          :title="sidebarCollapsed ? item.label : ''"
        >
          <i :class="item.icon"></i>
          <span v-if="!sidebarCollapsed" class="nav-label">{{ item.label }}</span>
        </li>
      </ul>
    </nav>

    <!-- Footer -->
    <div class="sidebar-footer" v-if="!sidebarCollapsed">
      <div class="version">v1.0.0</div>
    </div>
  </aside>
</template>

<style scoped>
.sidebar {
  position: fixed;
  top: 0;
  left: 0;
  height: 100vh;
  background: var(--surface-card);
  border-right: 1px solid var(--surface-border);
  display: flex;
  flex-direction: column;
  transition: width 0.3s ease;
  z-index: 1000;
  overflow: hidden;
}

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem;
  border-bottom: 1px solid var(--surface-border);
  min-height: 60px;
}

.logo {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.logo-icon {
  font-size: 1.5rem;
  color: var(--primary-color);
}

.logo-text {
  font-size: 1.25rem;
  font-weight: 600;
  color: var(--text-color);
  white-space: nowrap;
}

.toggle-btn {
  background: transparent;
  border: none;
  color: var(--text-color-secondary);
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.toggle-btn:hover {
  background: var(--surface-hover);
  color: var(--text-color);
}

.sidebar.collapsed .sidebar-header {
  justify-content: center;
  padding: 1rem 0.5rem;
}

.sidebar-nav {
  flex: 1;
  overflow-y: auto;
  padding: 0.5rem;
}

.sidebar-nav ul {
  list-style: none;
  margin: 0;
  padding: 0;
}

.sidebar-nav li {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.75rem 1rem;
  margin-bottom: 0.25rem;
  border-radius: 6px;
  cursor: pointer;
  color: var(--text-color-secondary);
  transition: all 0.2s;
}

.sidebar-nav li:hover {
  background: var(--surface-hover);
  color: var(--text-color);
}

.sidebar-nav li.active {
  background: var(--primary-color);
  color: white;
}

.sidebar-nav li i {
  font-size: 1.1rem;
  width: 1.5rem;
  text-align: center;
}

.nav-label {
  white-space: nowrap;
  font-weight: 500;
}

.sidebar.collapsed .sidebar-nav li {
  justify-content: center;
  padding: 0.75rem;
}

.sidebar-footer {
  padding: 1rem;
  border-top: 1px solid var(--surface-border);
  text-align: center;
}

.version {
  font-size: 0.75rem;
  color: var(--text-color-secondary);
}
</style>
