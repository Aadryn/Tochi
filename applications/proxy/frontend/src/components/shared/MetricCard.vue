<script setup lang="ts">
interface Props {
  title: string
  value: string | number
  icon?: string
  trend?: number
  trendLabel?: string
  color?: 'primary' | 'success' | 'warning' | 'danger' | 'info'
}

withDefaults(defineProps<Props>(), {
  color: 'primary',
})
</script>

<template>
  <div class="metric-card" :class="`color-${color}`">
    <div class="metric-icon" v-if="icon">
      <i :class="icon"></i>
    </div>
    <div class="metric-content">
      <span class="metric-title">{{ title }}</span>
      <span class="metric-value">{{ value }}</span>
      <div v-if="trend !== undefined" class="metric-trend" :class="{ positive: trend >= 0 }">
        <i :class="trend >= 0 ? 'pi pi-arrow-up' : 'pi pi-arrow-down'"></i>
        <span>{{ Math.abs(trend) }}%</span>
        <span v-if="trendLabel" class="trend-label">{{ trendLabel }}</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
.metric-card {
  display: flex;
  align-items: flex-start;
  gap: 1rem;
  padding: 1.25rem;
  background: var(--surface-card);
  border: 1px solid var(--surface-border);
  border-radius: 8px;
}

.metric-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: 10px;
  font-size: 1.25rem;
}

.color-primary .metric-icon {
  background: var(--primary-100);
  color: var(--primary-color);
}

.color-success .metric-icon {
  background: var(--green-100);
  color: var(--green-600);
}

.color-warning .metric-icon {
  background: var(--yellow-100);
  color: var(--yellow-600);
}

.color-danger .metric-icon {
  background: var(--red-100);
  color: var(--red-600);
}

.color-info .metric-icon {
  background: var(--blue-100);
  color: var(--blue-600);
}

.metric-content {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.metric-title {
  font-size: 0.875rem;
  color: var(--text-color-secondary);
}

.metric-value {
  font-size: 1.75rem;
  font-weight: 700;
  color: var(--text-color);
  line-height: 1.2;
}

.metric-trend {
  display: flex;
  align-items: center;
  gap: 0.25rem;
  font-size: 0.75rem;
  color: var(--red-500);
}

.metric-trend.positive {
  color: var(--green-500);
}

.metric-trend i {
  font-size: 0.625rem;
}

.trend-label {
  color: var(--text-color-secondary);
  margin-left: 0.25rem;
}
</style>
