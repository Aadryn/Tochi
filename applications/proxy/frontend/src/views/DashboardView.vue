<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useDashboardStore, useProvidersStore, useTenantsStore } from '@/stores'
import { MetricCard, AppCard, StatusBadge, LoadingSpinner } from '@/components'

const dashboardStore = useDashboardStore()
const providersStore = useProvidersStore()
const tenantsStore = useTenantsStore()

onMounted(async () => {
  // Charger les données avec mock pour le développement
  await Promise.all([
    dashboardStore.loadMetrics(true),
    providersStore.loadProviders(true),
    tenantsStore.loadTenants(true),
  ])
})

const isLoading = computed(
  () =>
    dashboardStore.isLoading || providersStore.isLoading || tenantsStore.isLoading,
)

const metrics = computed(() => dashboardStore.metrics)

function formatNumber(value: number): string {
  if (value >= 1000000) {
    return `${(value / 1000000).toFixed(1)}M`
  }
  if (value >= 1000) {
    return `${(value / 1000).toFixed(1)}K`
  }
  return value.toLocaleString('fr-FR')
}
</script>

<template>
  <div class="dashboard-view">
    <LoadingSpinner v-if="isLoading" message="Chargement des métriques..." />

    <template v-else-if="metrics">
      <!-- Métriques principales -->
      <section class="metrics-grid">
        <MetricCard
          title="Total Requêtes"
          :value="formatNumber(metrics.totalRequests)"
          icon="pi pi-send"
          :trend="12"
          trend-label="vs mois dernier"
          color="primary"
        />
        <MetricCard
          title="Providers Actifs"
          :value="metrics.activeProviders"
          icon="pi pi-server"
          color="success"
        />
        <MetricCard
          title="Tenants Actifs"
          :value="metrics.activeTenants"
          icon="pi pi-users"
          color="info"
        />
        <MetricCard
          title="Latence Moyenne"
          :value="`${metrics.avgLatencyMs}ms`"
          icon="pi pi-clock"
          :trend="-5"
          trend-label="amélioration"
          color="warning"
        />
      </section>

      <!-- Taux de succès -->
      <section class="success-rate-section">
        <AppCard title="Taux de Succès Global">
          <div class="success-rate">
            <div class="rate-value">{{ metrics.successRate }}%</div>
            <div class="rate-bar">
              <div
                class="rate-fill"
                :style="{ width: `${metrics.successRate}%` }"
              ></div>
            </div>
            <div class="rate-details">
              <span class="rate-label">Requêtes réussies sur les dernières 24h</span>
            </div>
          </div>
        </AppCard>
      </section>

      <!-- Liste des providers -->
      <section class="providers-section">
        <AppCard title="État des Providers" :padding="false">
          <template #actions>
            <router-link to="/providers" class="view-all-link">
              Voir tout
              <i class="pi pi-arrow-right"></i>
            </router-link>
          </template>

          <table class="providers-table">
            <thead>
              <tr>
                <th>Provider</th>
                <th>Statut</th>
                <th>Requêtes Aujourd'hui</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="provider in metrics.providers" :key="provider.name">
                <td class="provider-name">{{ provider.name }}</td>
                <td>
                  <StatusBadge :status="provider.status" size="small" />
                </td>
                <td class="provider-requests">
                  {{ formatNumber(provider.requestsToday) }}
                </td>
              </tr>
            </tbody>
          </table>
        </AppCard>
      </section>

      <!-- Activité récente -->
      <section class="activity-section">
        <div class="activity-grid">
          <AppCard title="Tenants Récents">
            <template #actions>
              <router-link to="/tenants" class="view-all-link">
                Voir tout
                <i class="pi pi-arrow-right"></i>
              </router-link>
            </template>

            <ul class="tenant-list">
              <li v-for="tenant in tenantsStore.tenants.slice(0, 5)" :key="tenant.id">
                <div class="tenant-info">
                  <span class="tenant-name">{{ tenant.name }}</span>
                  <span class="tenant-slug">{{ tenant.slug }}</span>
                </div>
                <span class="tenant-requests">
                  {{ formatNumber(tenant.requestsThisMonth) }} req/mois
                </span>
              </li>
            </ul>
          </AppCard>

          <AppCard title="Actions Rapides">
            <div class="quick-actions">
              <router-link to="/providers" class="quick-action">
                <i class="pi pi-plus"></i>
                <span>Nouveau Provider</span>
              </router-link>
              <router-link to="/tenants" class="quick-action">
                <i class="pi pi-user-plus"></i>
                <span>Nouveau Tenant</span>
              </router-link>
              <router-link to="/routes" class="quick-action">
                <i class="pi pi-directions"></i>
                <span>Configurer Route</span>
              </router-link>
              <router-link to="/monitoring" class="quick-action">
                <i class="pi pi-chart-line"></i>
                <span>Voir Monitoring</span>
              </router-link>
            </div>
          </AppCard>
        </div>
      </section>
    </template>
  </div>
</template>

<style scoped>
.dashboard-view {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

/* Grille des métriques */
.metrics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  gap: 1rem;
}

/* Taux de succès */
.success-rate {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 1rem;
  padding: 1rem 0;
}

.rate-value {
  font-size: 3rem;
  font-weight: 700;
  color: var(--green-600);
}

.rate-bar {
  width: 100%;
  max-width: 400px;
  height: 12px;
  background: var(--surface-200);
  border-radius: 6px;
  overflow: hidden;
}

.rate-fill {
  height: 100%;
  background: var(--green-500);
  border-radius: 6px;
  transition: width 0.5s ease;
}

.rate-label {
  font-size: 0.875rem;
  color: var(--text-color-secondary);
}

/* Table des providers */
.providers-table {
  width: 100%;
  border-collapse: collapse;
}

.providers-table th,
.providers-table td {
  padding: 1rem 1.25rem;
  text-align: left;
  border-bottom: 1px solid var(--surface-border);
}

.providers-table th {
  font-weight: 600;
  font-size: 0.875rem;
  color: var(--text-color-secondary);
  background: var(--surface-ground);
}

.providers-table tr:last-child td {
  border-bottom: none;
}

.provider-name {
  font-weight: 500;
}

.provider-requests {
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.875rem;
}

/* Lien voir tout */
.view-all-link {
  display: inline-flex;
  align-items: center;
  gap: 0.25rem;
  color: var(--primary-color);
  text-decoration: none;
  font-size: 0.875rem;
  font-weight: 500;
}

.view-all-link:hover {
  text-decoration: underline;
}

/* Grille d'activité */
.activity-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
  gap: 1rem;
}

/* Liste des tenants */
.tenant-list {
  list-style: none;
  margin: 0;
  padding: 0;
}

.tenant-list li {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0.75rem 0;
  border-bottom: 1px solid var(--surface-border);
}

.tenant-list li:last-child {
  border-bottom: none;
}

.tenant-info {
  display: flex;
  flex-direction: column;
}

.tenant-name {
  font-weight: 500;
}

.tenant-slug {
  font-size: 0.75rem;
  color: var(--text-color-secondary);
}

.tenant-requests {
  font-size: 0.875rem;
  color: var(--text-color-secondary);
}

/* Actions rapides */
.quick-actions {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 0.75rem;
}

.quick-action {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 1rem;
  background: var(--surface-ground);
  border-radius: 8px;
  text-decoration: none;
  color: var(--text-color);
  transition: all 0.2s;
}

.quick-action:hover {
  background: var(--surface-hover);
  transform: translateY(-2px);
}

.quick-action i {
  font-size: 1.25rem;
  color: var(--primary-color);
}

.quick-action span {
  font-size: 0.875rem;
  font-weight: 500;
  text-align: center;
}
</style>
