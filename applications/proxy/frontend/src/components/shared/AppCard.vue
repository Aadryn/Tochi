<script setup lang="ts">
interface Props {
  title?: string
  subtitle?: string
  padding?: boolean
}

withDefaults(defineProps<Props>(), {
  padding: true,
})
</script>

<template>
  <div class="card" :class="{ 'no-padding': !padding }">
    <div v-if="title || $slots.header" class="card-header">
      <div class="card-header-content" v-if="title">
        <h3 class="card-title">{{ title }}</h3>
        <p v-if="subtitle" class="card-subtitle">{{ subtitle }}</p>
      </div>
      <slot name="header"></slot>
      <div class="card-header-actions" v-if="$slots.actions">
        <slot name="actions"></slot>
      </div>
    </div>
    <div class="card-body" :class="{ 'has-header': title || $slots.header }">
      <slot></slot>
    </div>
    <div v-if="$slots.footer" class="card-footer">
      <slot name="footer"></slot>
    </div>
  </div>
</template>

<style scoped>
.card {
  background: var(--surface-card);
  border: 1px solid var(--surface-border);
  border-radius: 8px;
  overflow: hidden;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1.25rem;
  border-bottom: 1px solid var(--surface-border);
}

.card-header-content {
  flex: 1;
}

.card-title {
  margin: 0;
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--text-color);
}

.card-subtitle {
  margin: 0.25rem 0 0;
  font-size: 0.875rem;
  color: var(--text-color-secondary);
}

.card-header-actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.card-body {
  padding: 1.25rem;
}

.card-body.has-header {
  padding-top: 1rem;
}

.no-padding .card-body {
  padding: 0;
}

.card-footer {
  padding: 1rem 1.25rem;
  border-top: 1px solid var(--surface-border);
  background: var(--surface-ground);
}
</style>
