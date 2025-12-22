import {
  makeStyles,
  tokens,
  shorthands,
  Card,
  CardHeader,
  Text,
  Title3,
  Body1,
  Badge,
  Spinner,
} from '@fluentui/react-components'
import {
  ArrowTrending24Regular,
  CloudFlow24Regular,
  People24Regular,
  Timer24Regular,
  CheckmarkCircle24Regular,
  ErrorCircle24Regular,
} from '@fluentui/react-icons'
import { useQuery } from '@tanstack/react-query'
import { fetchDashboardMetrics } from '@/api/dashboard.api'

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    gap: '24px',
  },
  header: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  statsGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))',
    gap: '16px',
  },
  statCard: {
    ...shorthands.padding('20px'),
  },
  statContent: {
    display: 'flex',
    alignItems: 'center',
    gap: '16px',
  },
  statIcon: {
    width: '48px',
    height: '48px',
    ...shorthands.borderRadius('12px'),
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    fontSize: '24px',
  },
  statIconPrimary: {
    backgroundColor: tokens.colorBrandBackground2,
    color: tokens.colorBrandForeground1,
  },
  statIconSuccess: {
    backgroundColor: tokens.colorPaletteGreenBackground2,
    color: tokens.colorPaletteGreenForeground1,
  },
  statIconWarning: {
    backgroundColor: tokens.colorPaletteYellowBackground2,
    color: tokens.colorPaletteYellowForeground1,
  },
  statIconDanger: {
    backgroundColor: tokens.colorPaletteRedBackground2,
    color: tokens.colorPaletteRedForeground1,
  },
  statInfo: {
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
  },
  statValue: {
    fontSize: '28px',
    fontWeight: tokens.fontWeightSemibold,
    lineHeight: 1,
  },
  chartsGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(400px, 1fr))',
    gap: '16px',
  },
  chartCard: {
    minHeight: '300px',
  },
  providersList: {
    display: 'flex',
    flexDirection: 'column',
    gap: '12px',
    ...shorthands.padding('16px'),
  },
  providerItem: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    ...shorthands.padding('12px'),
    backgroundColor: tokens.colorNeutralBackground2,
    ...shorthands.borderRadius(tokens.borderRadiusMedium),
  },
  providerInfo: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
  },
  loadingContainer: {
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    minHeight: '200px',
  },
})

interface DashboardMetrics {
  totalRequests: number
  activeProviders: number
  activeTenants: number
  avgLatencyMs: number
  successRate: number
  providers: Array<{
    name: string
    status: 'healthy' | 'degraded' | 'unhealthy'
    requestsToday: number
  }>
}

export function Dashboard() {
  const styles = useStyles()

  const { data: metrics, isLoading, error } = useQuery<DashboardMetrics>({
    queryKey: ['dashboard-metrics'],
    queryFn: fetchDashboardMetrics,
    refetchInterval: 30000, // Refresh every 30 seconds
  })

  if (isLoading) {
    return (
      <div className={styles.loadingContainer}>
        <Spinner size="large" label="Chargement des métriques..." />
      </div>
    )
  }

  if (error) {
    return (
      <div className={styles.root}>
        <Card>
          <CardHeader
            image={<ErrorCircle24Regular />}
            header={<Text weight="semibold">Erreur de chargement</Text>}
            description="Impossible de charger les métriques du dashboard"
          />
        </Card>
      </div>
    )
  }

  // Fallback to mock data if API not available
  const data: DashboardMetrics = metrics || {
    totalRequests: 125847,
    activeProviders: 4,
    activeTenants: 12,
    avgLatencyMs: 245,
    successRate: 99.2,
    providers: [
      { name: 'OpenAI', status: 'healthy', requestsToday: 45230 },
      { name: 'Anthropic', status: 'healthy', requestsToday: 32150 },
      { name: 'Ollama Local', status: 'degraded', requestsToday: 8420 },
      { name: 'Azure OpenAI', status: 'healthy', requestsToday: 28970 },
    ],
  }

  const stats = [
    {
      label: 'Requêtes totales',
      value: data.totalRequests.toLocaleString(),
      icon: ArrowTrending24Regular,
      colorClass: styles.statIconPrimary,
    },
    {
      label: 'Providers actifs',
      value: data.activeProviders.toString(),
      icon: CloudFlow24Regular,
      colorClass: styles.statIconSuccess,
    },
    {
      label: 'Tenants actifs',
      value: data.activeTenants.toString(),
      icon: People24Regular,
      colorClass: styles.statIconWarning,
    },
    {
      label: 'Latence moyenne',
      value: `${data.avgLatencyMs}ms`,
      icon: Timer24Regular,
      colorClass: styles.statIconPrimary,
    },
  ]

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <div>
          <Title3>Dashboard</Title3>
          <Body1>Vue d'ensemble de LLMProxy</Body1>
        </div>
        <Badge appearance="filled" color="success">
          <CheckmarkCircle24Regular />
          Système opérationnel
        </Badge>
      </div>

      <div className={styles.statsGrid}>
        {stats.map((stat, index) => (
          <Card key={index} className={styles.statCard}>
            <div className={styles.statContent}>
              <div className={`${styles.statIcon} ${stat.colorClass}`}>
                <stat.icon />
              </div>
              <div className={styles.statInfo}>
                <Text className={styles.statValue}>{stat.value}</Text>
                <Body1>{stat.label}</Body1>
              </div>
            </div>
          </Card>
        ))}
      </div>

      <div className={styles.chartsGrid}>
        <Card className={styles.chartCard}>
          <CardHeader
            header={<Text weight="semibold">Statut des Providers</Text>}
            description="Santé et activité des providers LLM"
          />
          <div className={styles.providersList}>
            {data.providers.map((provider) => (
              <div key={provider.name} className={styles.providerItem}>
                <div className={styles.providerInfo}>
                  <CloudFlow24Regular />
                  <div>
                    <Text weight="semibold">{provider.name}</Text>
                    <Body1>{provider.requestsToday.toLocaleString()} requêtes aujourd'hui</Body1>
                  </div>
                </div>
                <Badge
                  appearance="filled"
                  color={
                    provider.status === 'healthy'
                      ? 'success'
                      : provider.status === 'degraded'
                      ? 'warning'
                      : 'danger'
                  }
                >
                  {provider.status === 'healthy'
                    ? 'Sain'
                    : provider.status === 'degraded'
                    ? 'Dégradé'
                    : 'Indisponible'}
                </Badge>
              </div>
            ))}
          </div>
        </Card>

        <Card className={styles.chartCard}>
          <CardHeader
            header={<Text weight="semibold">Taux de succès</Text>}
            description="Performance globale du proxy"
          />
          <div className={styles.providersList}>
            <div className={styles.providerItem}>
              <div className={styles.providerInfo}>
                <CheckmarkCircle24Regular />
                <div>
                  <Text weight="semibold">Taux de réussite global</Text>
                  <Body1>Sur les dernières 24 heures</Body1>
                </div>
              </div>
              <Text
                style={{ fontSize: '24px', fontWeight: 600 }}
                color={data.successRate >= 99 ? 'success' : data.successRate >= 95 ? 'warning' : 'danger'}
              >
                {data.successRate}%
              </Text>
            </div>
          </div>
        </Card>
      </div>
    </div>
  )
}
