<script setup lang="ts">
import type { ProviderStatus } from '@/types'

interface Props {
  status: ProviderStatus | string
  size?: 'small' | 'medium' | 'large'
}

const props = withDefaults(defineProps<Props>(), {
  size: 'medium',
})

const statusConfig: Record<
  string,
  { label: string; severity: 'success' | 'warning' | 'danger' | 'info' }
> = {
  healthy: { label: 'Opérationnel', severity: 'success' },
  degraded: { label: 'Dégradé', severity: 'warning' },
  unhealthy: { label: 'Hors service', severity: 'danger' },
  active: { label: 'Actif', severity: 'success' },
  inactive: { label: 'Inactif', severity: 'danger' },
  enabled: { label: 'Activé', severity: 'success' },
  disabled: { label: 'Désactivé', severity: 'danger' },
  pending: { label: 'En attente', severity: 'info' },
}

const config = statusConfig[props.status] || { label: props.status, severity: 'info' }
</script>

<template>
  <span class="status-badge" :class="[`severity-${config.severity}`, `size-${size}`]">
    <span class="status-dot"></span>
    <span class="status-label">{{ config.label }}</span>
  </span>
</template>

<style scoped>
.status-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
  padding: 0.25rem 0.75rem;
  border-radius: 9999px;
  font-weight: 500;
}

.status-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
}

/* Sizes */
.size-small {
  font-size: 0.75rem;
  padding: 0.125rem 0.5rem;
}

.size-small .status-dot {
  width: 6px;
  height: 6px;
}

.size-medium {
  font-size: 0.875rem;
}

.size-large {
  font-size: 1rem;
  padding: 0.375rem 1rem;
}

.size-large .status-dot {
  width: 10px;
  height: 10px;
}

/* Severities */
.severity-success {
  background: var(--green-50);
  color: var(--green-700);
}

.severity-success .status-dot {
  background: var(--green-500);
}

.severity-warning {
  background: var(--yellow-50);
  color: var(--yellow-700);
}

.severity-warning .status-dot {
  background: var(--yellow-500);
}

.severity-danger {
  background: var(--red-50);
  color: var(--red-700);
}

.severity-danger .status-dot {
  background: var(--red-500);
}

.severity-info {
  background: var(--blue-50);
  color: var(--blue-700);
}

.severity-info .status-dot {
  background: var(--blue-500);
}
</style>
