<script setup lang="ts">
import { computed, ref, watch, onMounted } from 'vue'
import { Doughnut } from 'vue-chartjs'
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend,
  type ChartData,
  type ChartOptions,
} from 'chart.js'

// Enregistrer les composants Chart.js
ChartJS.register(ArcElement, Tooltip, Legend)

interface ProviderData {
  name: string
  requests: number
  color: string
}

const props = defineProps<{
  providers: ProviderData[]
}>()

const chartRef = ref()

const chartData = computed<ChartData<'doughnut'>>(() => ({
  labels: props.providers.map((p) => p.name),
  datasets: [
    {
      data: props.providers.map((p) => p.requests),
      backgroundColor: props.providers.map((p) => p.color),
      borderColor: 'rgba(255, 255, 255, 0.8)',
      borderWidth: 2,
      hoverOffset: 8,
    },
  ],
}))

const chartOptions = computed<ChartOptions<'doughnut'>>(() => ({
  responsive: true,
  maintainAspectRatio: false,
  cutout: '60%',
  plugins: {
    legend: {
      position: 'right' as const,
      labels: {
        padding: 16,
        usePointStyle: true,
        pointStyle: 'circle',
        font: {
          size: 12,
        },
      },
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
      displayColors: true,
      callbacks: {
        label: (context) => {
          const total = context.dataset.data.reduce((a: number, b: number) => a + b, 0)
          const value = context.parsed as number
          const percentage = ((value / total) * 100).toFixed(1)
          return `${context.label}: ${value.toLocaleString()} (${percentage}%)`
        },
      },
    },
  },
  animation: {
    animateRotate: true,
    animateScale: true,
  },
}))

// Mettre à jour le graphique quand les données changent
watch(
  () => props.providers,
  () => {
    if (chartRef.value?.chart) {
      chartRef.value.chart.update()
    }
  },
  { deep: true }
)
</script>

<template>
  <div class="doughnut-chart">
    <Doughnut
      ref="chartRef"
      :data="chartData"
      :options="chartOptions"
    />
  </div>
</template>

<style scoped>
.doughnut-chart {
  position: relative;
  height: 280px;
  width: 100%;
}
</style>
