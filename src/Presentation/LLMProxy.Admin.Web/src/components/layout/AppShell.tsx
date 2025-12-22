import { ReactNode } from 'react'
import { makeStyles, tokens, shorthands } from '@fluentui/react-components'
import { NavRail } from './NavRail'
import { Header } from './Header'

const useStyles = makeStyles({
  root: {
    display: 'flex',
    flexDirection: 'column',
    height: '100vh',
    backgroundColor: tokens.colorNeutralBackground2,
  },
  body: {
    display: 'flex',
    flex: 1,
    overflow: 'hidden',
  },
  main: {
    flex: 1,
    overflow: 'auto',
    ...shorthands.padding('24px'),
  },
})

interface AppShellProps {
  children: ReactNode
  isDarkMode: boolean
  onToggleTheme: () => void
}

export function AppShell({ children, isDarkMode, onToggleTheme }: AppShellProps) {
  const styles = useStyles()

  return (
    <div className={styles.root}>
      <Header isDarkMode={isDarkMode} onToggleTheme={onToggleTheme} />
      <div className={styles.body}>
        <NavRail />
        <main className={styles.main}>{children}</main>
      </div>
    </div>
  )
}
