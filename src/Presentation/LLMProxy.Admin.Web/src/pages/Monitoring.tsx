import {
  makeStyles,
  tokens,
  shorthands,
  Card,
  CardHeader,
  Text,
  Title3,
  Body1,
  Button,
  Badge,
  Input,
  Dropdown,
  Option,
  Divider,
} from '@fluentui/react-components'
import {
  Search24Regular,
  ArrowClockwise24Regular,
  Info24Regular,
  Warning24Regular,
  ErrorCircle24Regular,
  ChartMultiple24Regular,
} from '@fluentui/react-icons'
import { useState } from 'react'

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
  toolbar: {
    display: 'flex',
    gap: '12px',
    alignItems: 'center',
    flexWrap: 'wrap',
  },
  searchInput: {
    width: '300px',
  },
  filterDropdown: {
    minWidth: '150px',
  },
  logCard: {
    ...shorthands.overflow('hidden'),
  },
  logEntry: {
    display: 'flex',
    alignItems: 'flex-start',
    gap: '12px',
    ...shorthands.padding('12px', '16px'),
    ...shorthands.borderBottom('1px', 'solid', tokens.colorNeutralStroke2),
    '&:hover': {
      backgroundColor: tokens.colorNeutralBackground2,
    },
  },
  logIcon: {
    marginTop: '2px',
  },
  logContent: {
    flex: 1,
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
  },
  logMessage: {
    fontFamily: 'monospace',
    fontSize: '13px',
    wordBreak: 'break-all',
  },
  logMeta: {
    display: 'flex',
    gap: '16px',
    fontSize: '12px',
    color: tokens.colorNeutralForeground3,
  },
  metricsGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))',
    gap: '16px',
  },
  metricCard: {
    ...shorthands.padding('16px'),
  },
  metricValue: {
    fontSize: '32px',
    fontWeight: tokens.fontWeightSemibold,
    color: tokens.colorBrandForeground1,
  },
})

interface LogEntry {
  id: string
  timestamp: string
  level: 'info' | 'warning' | 'error'
  message: string
  source: string
  traceId?: string
}

export function Monitoring() {
  const styles = useStyles()
  const [searchTerm, setSearchTerm] = useState('')
  const [levelFilter, setLevelFilter] = useState<string>('all')

  // Mock logs
  const logs: LogEntry[] = [
    {
      id: '1',
      timestamp: '2024-12-06T10:45:32.123Z',
      level: 'info',
      message: 'Request completed successfully for tenant acme - 245ms',
      source: 'LLMProxy.Gateway',
      traceId: 'abc123def456',
    },
    {
      id: '2',
      timestamp: '2024-12-06T10:45:30.456Z',
      level: 'warning',
      message: 'Provider ollama-local responding slowly - latency 2340ms',
      source: 'LLMProxy.Gateway',
      traceId: 'xyz789ghi012',
    },
    {
      id: '3',
      timestamp: '2024-12-06T10:45:28.789Z',
      level: 'error',
      message: 'Rate limit exceeded for tenant techstartup - 429 returned',
      source: 'LLMProxy.Gateway',
      traceId: 'mno345pqr678',
    },
    {
      id: '4',
      timestamp: '2024-12-06T10:45:25.012Z',
      level: 'info',
      message: 'Provider health check passed for openai-production',
      source: 'LLMProxy.HealthChecks',
    },
    {
      id: '5',
      timestamp: '2024-12-06T10:45:20.345Z',
      level: 'info',
      message: 'New API key created for tenant enterprise - key_id: api_k8s2f3',
      source: 'LLMProxy.Admin',
      traceId: 'stu901vwx234',
    },
  ]

  const getLevelIcon = (level: string) => {
    switch (level) {
      case 'info':
        return <Info24Regular style={{ color: tokens.colorBrandForeground1 }} />
      case 'warning':
        return <Warning24Regular style={{ color: tokens.colorPaletteYellowForeground1 }} />
      case 'error':
        return <ErrorCircle24Regular style={{ color: tokens.colorPaletteRedForeground1 }} />
      default:
        return <Info24Regular />
    }
  }

  const getLevelBadgeColor = (level: string): 'informative' | 'warning' | 'danger' => {
    switch (level) {
      case 'warning':
        return 'warning'
      case 'error':
        return 'danger'
      default:
        return 'informative'
    }
  }

  const filteredLogs = logs.filter((log) => {
    const matchesSearch =
      log.message.toLowerCase().includes(searchTerm.toLowerCase()) ||
      log.source.toLowerCase().includes(searchTerm.toLowerCase())
    const matchesLevel = levelFilter === 'all' || log.level === levelFilter
    return matchesSearch && matchesLevel
  })

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <div>
          <Title3>Monitoring</Title3>
          <Body1>Logs et métriques en temps réel</Body1>
        </div>
        <Button appearance="secondary" icon={<ArrowClockwise24Regular />}>
          Actualiser
        </Button>
      </div>

      <div className={styles.metricsGrid}>
        <Card className={styles.metricCard}>
          <Body1>Requêtes / min</Body1>
          <Text className={styles.metricValue}>847</Text>
        </Card>
        <Card className={styles.metricCard}>
          <Body1>Latence P95</Body1>
          <Text className={styles.metricValue}>312ms</Text>
        </Card>
        <Card className={styles.metricCard}>
          <Body1>Erreurs (1h)</Body1>
          <Text className={styles.metricValue} style={{ color: tokens.colorPaletteRedForeground1 }}>
            23
          </Text>
        </Card>
        <Card className={styles.metricCard}>
          <Body1>Uptime</Body1>
          <Text className={styles.metricValue} style={{ color: tokens.colorPaletteGreenForeground1 }}>
            99.9%
          </Text>
        </Card>
      </div>

      <Card className={styles.logCard}>
        <CardHeader
          image={<ChartMultiple24Regular />}
          header={<Text weight="semibold">Logs récents</Text>}
          action={
            <div className={styles.toolbar}>
              <Input
                className={styles.searchInput}
                contentBefore={<Search24Regular />}
                placeholder="Rechercher dans les logs..."
                value={searchTerm}
                onChange={(_, data) => setSearchTerm(data.value)}
              />
              <Dropdown
                className={styles.filterDropdown}
                placeholder="Niveau"
                value={levelFilter === 'all' ? 'Tous les niveaux' : levelFilter}
                onOptionSelect={(_, data) => setLevelFilter(data.optionValue || 'all')}
              >
                <Option value="all">Tous les niveaux</Option>
                <Option value="info">Info</Option>
                <Option value="warning">Warning</Option>
                <Option value="error">Error</Option>
              </Dropdown>
            </div>
          }
        />
        <Divider />
        <div>
          {filteredLogs.map((log) => (
            <div key={log.id} className={styles.logEntry}>
              <div className={styles.logIcon}>{getLevelIcon(log.level)}</div>
              <div className={styles.logContent}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <Badge appearance="outline" color={getLevelBadgeColor(log.level)}>
                    {log.level.toUpperCase()}
                  </Badge>
                  <Badge appearance="outline">{log.source}</Badge>
                </div>
                <Text className={styles.logMessage}>{log.message}</Text>
                <div className={styles.logMeta}>
                  <span>{new Date(log.timestamp).toLocaleString('fr-FR')}</span>
                  {log.traceId && <span>TraceId: {log.traceId}</span>}
                </div>
              </div>
            </div>
          ))}
        </div>
      </Card>
    </div>
  )
}
