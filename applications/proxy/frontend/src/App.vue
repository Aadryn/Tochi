<script setup lang="ts">
import { computed, onMounted, watch } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import AppLayout from '@/components/layout/AppLayout.vue'
import { useSettingsStore } from '@/stores'

const route = useRoute()
const settingsStore = useSettingsStore()

// Ne pas afficher le layout sur les pages publiques (login)
const showLayout = computed(() => route.meta.public !== true)

/**
 * Applique le thème au document.
 */
function applyTheme(theme: 'light' | 'dark'): void {
  const root = document.documentElement
  const body = document.body

  if (theme === 'dark') {
    root.classList.add('dark-theme')
    body.classList.add('dark-theme')
  } else {
    root.classList.remove('dark-theme')
    body.classList.remove('dark-theme')
  }
}

// Observer les changements de thème
watch(
  () => settingsStore.settings.theme,
  (newTheme) => {
    applyTheme(newTheme)
  }
)

// Appliquer le thème au montage
onMounted(() => {
  applyTheme(settingsStore.settings.theme)
})
</script>

<template>
  <AppLayout v-if="showLayout">
    <RouterView />
  </AppLayout>
  <RouterView v-else />
</template>

<style scoped>
/* Styles spécifiques à App */
</style>
