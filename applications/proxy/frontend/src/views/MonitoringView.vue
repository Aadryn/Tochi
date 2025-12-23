<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { AppCard, MetricCard, LoadingSpinner, RequestsByProviderChart, LatencyChart } from '@/components'

interface RequestLog {
  id: string
  timestamp: string
  tenant: string
  provider: string
  endpoint: string
  status: number
  latencyMs: number
  tokensUsed: number
}

interface ProviderStats {
  name: string
  requests: number
  color: string
}

interface LatencyPoint {
  time: string
  value: number
}

const isLoading = ref(true)
const realtimeEnabled = ref(true)
const requestLogs = ref<RequestLog[]>([])
const latencyHistory = ref<LatencyPoint[]>([])
let refreshInterval: ReturnType<typeof setInterval> | null = null

// Métriques temps réel
const metrics = ref({
  requestsPerSecond: 45.2,
  avgLatency: 234,
  errorRate: 0.8,
  activeConnections: 128,
})

// Statistiques par provider
const providerStats = ref<ProviderStats[]>([
  { name: 'OpenAI GPT-4', requests: 12500, color: '#10b981' },
  { name: 'Anthropic Claude', requests: 8700, color: '#8b5cf6' },
  { name: 'Ollama Local', requests: 3200, color: '#f59e0b' },
  { name: 'Azure OpenAI', requests: 5600, color: '#3b82f6' },
])

// Générer des logs mock
function generateMockLog(): RequestLog {
  const tenants = ['Acme Corp', 'Tech Startup', 'Research Lab']
  const providers = ['OpenAI GPT-4', 'Anthropic Claude', 'Ollama Local']
  const endpoints = ['/v1/chat/completions', '/v1/embeddings', '/v1/messages']
  const statuses = [200, 200, 200, 200, 200, 400, 500]

  return {
    id: Math.random().toString(36).substr(2, 9),
    timestamp: new Date().toISOString(),
    tenant: tenants[Math.floor(Math.random() * tenants.length)],
    provider: providers[Math.floor(Math.random() * providers.length)],
    endpoint: endpoints[Math.floor(Math.random() * endpoints.length)],
    status: statuses[Math.floor(Math.random() * statuses.length)],
    latencyMs: Math.floor(Math.random() * 500) + 50,
    tokensUsed: Math.floor(Math.random() * 2000) + 100,
  }
}

function generateInitialLogs(): RequestLog[] {
  return Array.from({ length: 20 }, generateMockLog).sort(
    (a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime(),
  )
}

function generateInitialLatencyHistory(): LatencyPoint[] {
  const now = new Date()
  const points: LatencyPoint[] = []
  for (let i = 29; i >= 0; i--) {
    const time = new Date(now.getTime() - i * 2000)
    points.push({
      time: time.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit', second: '2-digit' }),
      value: Math.floor(Math.random() * 200) + 150,
    })
  }
  return points
}

function startRealtime(): void {
  if (refreshInterval) return

  refreshInterval = setInterval(() => {
    if (!realtimeEnabled.value) return

    // Ajouter un nouveau log
    const newLog = generateMockLog()
    requestLogs.value = [newLog, ...requestLogs.value.slice(0, 49)]

    // Mettre à jour les métriques avec une légère variation
    metrics.value = {
      requestsPerSecond: Math.max(0, metrics.value.requestsPerSecond + (Math.random() - 0.5) * 5),
      avgLatency: Math.max(50, metrics.value.avgLatency + (Math.random() - 0.5) * 20),
      errorRate: Math.max(0, Math.min(5, metrics.value.errorRate + (Math.random() - 0.5) * 0.2)),
      activeConnections: Math.max(50, metrics.value.activeConnections + Math.floor((Math.random() - 0.5) * 10)),
    }

    // Mettre à jour l'historique de latence
    const now = new Date()
    latencyHistory.value = [
      ...latencyHistory.value.slice(1),
      {
        time: now.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit', second: '2-digit' }),
        value: Math.round(metrics.value.avgLatency + (Math.random() - 0.5) * 40),
      },
    ]

    // Mettre à jour les stats par provider (légère variation)
    providerStats.value = providerStats.value.map((p) => ({
      ...p,
      requests: p.requests + Math.floor(Math.random() * 50),
    }))
  }, 2000)
}

function stopRealtime(): void {
  if (refreshInterval) {
    clearInterval(refreshInterval)
    refreshInterval = null
  }
}

function toggleRealtime(): void {
  realtimeEnabled.value = !realtimeEnabled.value
  if (realtimeEnabled.value) {
    startRealtime()
  } else {
    stopRealtime()
  }
}

onMounted(async () => {
  // Simuler un chargement initial
  await new Promise((resolve) => setTimeout(resolve, 800))
  requestLogs.value = generateInitialLogs()
  latencyHistory.value = generateInitialLatencyHistory()
  isLoading.value = false
  startRealtime()
})

onUnmounted(() => {
  stopRealtime()
})

function formatTime(isoString: string): string {
  return new Date(isoString).toLocaleTimeString('fr-FR', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  })
}

function getStatusClass(status: number): string {
  if (status >= 200 && status < 300) return 'success'
  if (status >= 400 && status < 500) return 'warning'
  return 'danger'
}
</script>

<template>
  <div class="monitoring-view">
    <div class="view-header">
      <div class="header-info">
        <h2 class="view-title" data-testid="page-title">Monitoring</h2>
        <p class="view-description">
          Surveillance en temps réel des requêtes et performances
        </p>
      </div>
      <button
        class="btn-toggle"
        :class="{ active: realtimeEnabled }"
        @click="toggleRealtime"
      >
        <i :class="realtimeEnabled ? 'pi pi-pause' : 'pi pi-play'"></i>
        {{ realtimeEnabled ? 'Pause' : 'Reprendre' }}
      </button>
    </div>

    <LoadingSpinner v-if="isLoading" message="Connexion au flux en temps réel..." />

    <template v-else>
      <!-- Métriques temps réel -->
      <section class="metrics-grid">
        <MetricCard
          title="Requêtes/sec"
          :value="metrics.requestsPerSecond.toFixed(1)"
          icon="pi pi-bolt"
          color="primary"
        />
        <MetricCard
          title="Latence Moyenne"
          :value="`${Math.round(metrics.avgLatency)}ms`"
          icon="pi pi-clock"
          color="info"
        />
        <MetricCard
          title="Taux d'Erreur"
          :value="`${metrics.errorRate.toFixed(1)}%`"
          icon="pi pi-exclamation-triangle"
          :color="metrics.errorRate > 2 ? 'danger' : 'success'"
        />
        <MetricCard
          title="Connexions Actives"
          :value="metrics.activeConnections"
          icon="pi pi-link"
          color="warning"
        />
      </section>

      <!-- Log des requêtes en temps réel -->
      <AppCard title="Requêtes en Temps Réel" :padding="false">
        <template #actions>
          <div class="realtime-indicator" :class="{ active: realtimeEnabled }">
            <span class="indicator-dot"></span>
            {{ realtimeEnabled ? 'En direct' : 'En pause' }}
          </div>
        </template>

        <div class="logs-container">
          <table class="logs-table">
            <thead>
              <tr>
                <th>Heure</th>
                <th>Tenant</th>
                <th>Provider</th>
                <th>Endpoint</th>
                <th>Statut</th>
                <th>Latence</th>
                <th>Tokens</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="log in requestLogs"
                :key="log.id"
                class="log-row"
              >
                <td class="log-time">{{ formatTime(log.timestamp) }}</td>
                <td>{{ log.tenant }}</td>
                <td>{{ log.provider }}</td>
                <td class="log-endpoint">{{ log.endpoint }}</td>
                <td>
                  <span class="status-code" :class="getStatusClass(log.status)">
                    {{ log.status }}
                  </span>
                </td>
                <td class="log-latency">{{ log.latencyMs }}ms</td>
                <td class="log-tokens">{{ log.tokensUsed }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </AppCard>

      <!-- Graphiques de performance -->
      <div class="charts-grid">
        <AppCard title="Requêtes par Provider">
          <RequestsByProviderChart :providers="providerStats" />
        </AppCard>

        <AppCard title="Latence en Temps Réel">
          <LatencyChart :data="latencyHistory" />
        </AppCard>
      </div>
    </template>
  </div>
</template>

<style scoped>
.monitoring-view {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.view-header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
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

.btn-toggle {
  display: inline-flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.625rem 1rem;
  background: var(--surface-100);
  color: var(--text-color);
  border: 1px solid var(--surface-border);
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-toggle:hover {
  background: var(--surface-200);
}

.btn-toggle.active {
  background: var(--primary-color);
  color: white;
  border-color: var(--primary-color);
}

/* Métriques */
.metrics-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 1rem;
}

/* Indicateur temps réel */
.realtime-indicator {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: var(--text-color-secondary);
}

.realtime-indicator.active {
  color: var(--green-600);
}

.indicator-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--text-color-secondary);
}

.realtime-indicator.active .indicator-dot {
  background: var(--green-500);
  animation: pulse 2s infinite;
}

@keyframes pulse {
  0%, 100% {
    opacity: 1;
  }
  50% {
    opacity: 0.5;
  }
}

/* Logs */
.logs-container {
  max-height: 400px;
  overflow-y: auto;
}

.logs-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.logs-table th,
.logs-table td {
  padding: 0.75rem 1rem;
  text-align: left;
  border-bottom: 1px solid var(--surface-border);
}

.logs-table th {
  font-weight: 600;
  color: var(--text-color-secondary);
  background: var(--surface-ground);
  position: sticky;
  top: 0;
  z-index: 1;
}

.log-row {
  animation: fadeIn 0.3s ease;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    background: var(--primary-50);
  }
  to {
    opacity: 1;
    background: transparent;
  }
}

.log-time {
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.8125rem;
  color: var(--text-color-secondary);
}

.log-endpoint {
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.8125rem;
}

.log-latency,
.log-tokens {
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.8125rem;
}

.status-code {
  display: inline-block;
  padding: 0.125rem 0.5rem;
  font-family: 'JetBrains Mono', monospace;
  font-size: 0.75rem;
  font-weight: 600;
  border-radius: 4px;
}

.status-code.success {
  background: var(--green-100);
  color: var(--green-700);
}

.status-code.warning {
  background: var(--yellow-100);
  color: var(--yellow-700);
}

.status-code.danger {
  background: var(--red-100);
  color: var(--red-700);
}

/* Graphiques */
.charts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(400px, 1fr));
  gap: 1rem;
}

.chart-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 200px;
  color: var(--text-color-secondary);
}

.chart-placeholder i {
  font-size: 3rem;
  opacity: 0.3;
  margin-bottom: 1rem;
}

.chart-placeholder p {
  font-size: 0.875rem;
}
</style>
