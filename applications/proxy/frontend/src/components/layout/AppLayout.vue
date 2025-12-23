<script setup lang="ts">
import AppSidebar from './AppSidebar.vue'
import AppHeader from './AppHeader.vue'
</script>

<template>
  <div class="app-layout">
    <AppSidebar />
    <div class="main-container">
      <AppHeader />
      <main class="main-content">
        <slot></slot>
      </main>
    </div>
  </div>
</template>

<style scoped>
.app-layout {
  display: flex;
  min-height: 100vh;
  background: var(--surface-ground);
}

.main-container {
  flex: 1;
  margin-left: 240px;
  display: flex;
  flex-direction: column;
  transition: margin-left 0.3s ease;
}

.main-content {
  flex: 1;
  padding: 1.5rem;
  overflow-y: auto;
}

/* Support du sidebar réduit */
@media (max-width: 768px) {
  .main-container {
    margin-left: 60px;
  }
}
</style>
