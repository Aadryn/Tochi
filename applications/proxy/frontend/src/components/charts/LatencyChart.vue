<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { Line } from 'vue-chartjs'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler,
  type ChartData,
  type ChartOptions,
} from 'chart.js'

// Enregistrer les composants Chart.js
ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
)

interface LatencyPoint {
  time: string
  value: number
}

const props = defineProps<{
  data: LatencyPoint[]
}>()

const chartRef = ref()

const chartData = computed<ChartData<'line'>>(() => ({
  labels: props.data.map((p) => p.time),
  datasets: [
    {
      label: 'Latence (ms)',
      data: props.data.map((p) => p.value),
      borderColor: '#3b82f6',
      backgroundColor: 'rgba(59, 130, 246, 0.1)',
      borderWidth: 2,
      fill: true,
      tension: 0.4,
      pointRadius: 0,
      pointHoverRadius: 6,
      pointBackgroundColor: '#3b82f6',
      pointHoverBackgroundColor: '#3b82f6',
      pointBorderColor: '#ffffff',
      pointHoverBorderColor: '#ffffff',
      pointBorderWidth: 2,
    },
  ],
}))

const chartOptions = computed<ChartOptions<'line'>>(() => ({
  responsive: true,
  maintainAspectRatio: false,
  interaction: {
    mode: 'index' as const,
    intersect: false,
  },
  plugins: {
    legend: {
      display: false,
    },
    tooltip: {
      backgroundColor: 'rgba(30, 41, 59, 0.95)',
      titleFont: {
        size: 13,
        weight: 'bold' as const,
      },
      bodyFont: {
        size: 12,
      },
      padding: 12,
      cornerRadius: 8,
      displayColors: false,
      callbacks: {
        label: (context) => {
          return `Latence: ${context.parsed.y}ms`
        },
      },
    },
  },
  scales: {
    x: {
      display: true,
      grid: {
        display: false,
      },
      ticks: {
        font: {
          size: 11,
        },
        color: '#94a3b8',
        maxTicksLimit: 8,
      },
    },
    y: {
      display: true,
      beginAtZero: false,
      grid: {
        color: 'rgba(148, 163, 184, 0.1)',
      },
      ticks: {
        font: {
          size: 11,
        },
        color: '#94a3b8',
        callback: (value) => `${value}ms`,
      },
    },
  },
  animation: {
    duration: 300,
  },
}))

// Mettre à jour le graphique quand les données changent
watch(
  () => props.data,
  () => {
    if (chartRef.value?.chart) {
      chartRef.value.chart.update('none')
    }
  },
  { deep: true }
)
</script>

<template>
  <div class="latency-chart">
    <Line
      ref="chartRef"
      :data="chartData"
      :options="chartOptions"
    />
  </div>
</template>

<style scoped>
.latency-chart {
  position: relative;
  height: 280px;
  width: 100%;
}
</style>
