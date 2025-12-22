import {
  makeStyles,
  tokens,
  shorthands,
  Text,
  Button,
  Avatar,
  Menu,
  MenuTrigger,
  MenuPopover,
  MenuList,
  MenuItem,
} from '@fluentui/react-components'
import {
  WeatherMoon24Regular,
  WeatherSunny24Regular,
  PersonCircle24Regular,
  SignOut24Regular,
} from '@fluentui/react-icons'

const useStyles = makeStyles({
  root: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    height: '56px',
    minHeight: '56px',
    backgroundColor: tokens.colorNeutralBackground1,
    borderBottom: `1px solid ${tokens.colorNeutralStroke2}`,
    ...shorthands.padding('0', '24px'),
  },
  brand: {
    display: 'flex',
    alignItems: 'center',
    gap: '12px',
  },
  logo: {
    width: '32px',
    height: '32px',
    backgroundColor: tokens.colorBrandBackground,
    ...shorthands.borderRadius('8px'),
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    color: tokens.colorNeutralForegroundOnBrand,
    fontWeight: tokens.fontWeightSemibold,
    fontSize: '14px',
  },
  actions: {
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
  },
})

interface HeaderProps {
  isDarkMode: boolean
  onToggleTheme: () => void
}

export function Header({ isDarkMode, onToggleTheme }: HeaderProps) {
  const styles = useStyles()

  return (
    <header className={styles.root}>
      <div className={styles.brand}>
        <div className={styles.logo}>LP</div>
        <Text weight="semibold" size={400}>
          LLMProxy Admin
        </Text>
      </div>
      <div className={styles.actions}>
        <Button
          appearance="subtle"
          icon={isDarkMode ? <WeatherSunny24Regular /> : <WeatherMoon24Regular />}
          onClick={onToggleTheme}
          title={isDarkMode ? 'Mode clair' : 'Mode sombre'}
        />
        <Menu>
          <MenuTrigger disableButtonEnhancement>
            <Button
              appearance="subtle"
              icon={<Avatar name="Admin User" size={28} />}
            />
          </MenuTrigger>
          <MenuPopover>
            <MenuList>
              <MenuItem icon={<PersonCircle24Regular />}>Profil</MenuItem>
              <MenuItem icon={<SignOut24Regular />}>Déconnexion</MenuItem>
            </MenuList>
          </MenuPopover>
        </Menu>
      </div>
    </header>
  )
}
