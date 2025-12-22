import { useLocation, useNavigate } from 'react-router-dom'
import {
  makeStyles,
  tokens,
  shorthands,
  Tab,
  TabList,
  Tooltip,
} from '@fluentui/react-components'
import {
  Home24Regular,
  Home24Filled,
  CloudFlow24Regular,
  CloudFlow24Filled,
  People24Regular,
  People24Filled,
  ArrowRouting24Regular,
  ArrowRouting24Filled,
  ChartMultiple24Regular,
  ChartMultiple24Filled,
  Settings24Regular,
  Settings24Filled,
  bundleIcon,
} from '@fluentui/react-icons'

const HomeIcon = bundleIcon(Home24Filled, Home24Regular)
const ProvidersIcon = bundleIcon(CloudFlow24Filled, CloudFlow24Regular)
const TenantsIcon = bundleIcon(People24Filled, People24Regular)
const RoutesIcon = bundleIcon(ArrowRouting24Filled, ArrowRouting24Regular)
const MonitoringIcon = bundleIcon(ChartMultiple24Filled, ChartMultiple24Regular)
const SettingsIcon = bundleIcon(Settings24Filled, Settings24Regular)

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    width: '68px',
    minWidth: '68px',
    backgroundColor: tokens.colorNeutralBackground1,
    borderRight: `1px solid ${tokens.colorNeutralStroke2}`,
    ...shorthands.padding('12px', '8px'),
    alignItems: 'center',
  },
  tabList: {
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
  },
  tab: {
    width: '48px',
    height: '48px',
    minHeight: '48px',
    justifyContent: 'center',
    ...shorthands.borderRadius(tokens.borderRadiusMedium),
  },
  spacer: {
    flex: 1,
  },
})

const navItems = [
  { path: '/', icon: HomeIcon, label: 'Dashboard' },
  { path: '/providers', icon: ProvidersIcon, label: 'Providers' },
  { path: '/tenants', icon: TenantsIcon, label: 'Tenants' },
  { path: '/routes', icon: RoutesIcon, label: 'Routes' },
  { path: '/monitoring', icon: MonitoringIcon, label: 'Monitoring' },
]

export function NavRail() {
  const styles = useStyles()
  const location = useLocation()
  const navigate = useNavigate()

  const handleSelect = (_: unknown, data: { value: unknown }) => {
    navigate(data.value as string)
  }

  return (
    <nav className={styles.root}>
      <TabList
        className={styles.tabList}
        vertical
        selectedValue={location.pathname}
        onTabSelect={handleSelect}
        size="large"
      >
        {navItems.map((item) => (
          <Tooltip key={item.path} content={item.label} relationship="label" positioning="after">
            <Tab
              className={styles.tab}
              value={item.path}
              icon={<item.icon />}
            />
          </Tooltip>
        ))}
      </TabList>
      <div className={styles.spacer} />
      <TabList
        className={styles.tabList}
        vertical
        selectedValue={location.pathname}
        onTabSelect={handleSelect}
        size="large"
      >
        <Tooltip content="Settings" relationship="label" positioning="after">
          <Tab
            className={styles.tab}
            value="/settings"
            icon={<SettingsIcon />}
          />
        </Tooltip>
      </TabList>
    </nav>
  )
}
