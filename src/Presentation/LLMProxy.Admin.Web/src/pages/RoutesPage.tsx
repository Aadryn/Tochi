import {
  makeStyles,
  tokens,
  shorthands,
  Card,
  Title3,
  Body1,
  Button,
  Badge,
  Input,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  TableCellLayout,
  Switch,
  Menu,
  MenuTrigger,
  MenuPopover,
  MenuList,
  MenuItem,
} from '@fluentui/react-components'
import {
  Add24Regular,
  Search24Regular,
  MoreHorizontal24Regular,
  Edit24Regular,
  Delete24Regular,
  ArrowRouting24Regular,
  CloudFlow24Regular,
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
  },
  searchInput: {
    width: '300px',
  },
  tableCard: {
    ...shorthands.overflow('hidden'),
  },
  pathCell: {
    fontFamily: 'monospace',
    backgroundColor: tokens.colorNeutralBackground3,
    ...shorthands.padding('4px', '8px'),
    ...shorthands.borderRadius(tokens.borderRadiusSmall),
  },
})

interface Route {
  id: string
  path: string
  provider: string
  model: string
  isEnabled: boolean
  priority: number
  rateLimit: number
}

export function RoutesPage() {
  const styles = useStyles()
  const [searchTerm, setSearchTerm] = useState('')

  // Mock data
  const routes: Route[] = [
    {
      id: '1',
      path: '/v1/chat/completions',
      provider: 'OpenAI',
      model: 'gpt-4-turbo',
      isEnabled: true,
      priority: 1,
      rateLimit: 1000,
    },
    {
      id: '2',
      path: '/v1/completions',
      provider: 'OpenAI',
      model: 'gpt-3.5-turbo-instruct',
      isEnabled: true,
      priority: 2,
      rateLimit: 2000,
    },
    {
      id: '3',
      path: '/v1/embeddings',
      provider: 'OpenAI',
      model: 'text-embedding-3-small',
      isEnabled: true,
      priority: 1,
      rateLimit: 3000,
    },
    {
      id: '4',
      path: '/anthropic/messages',
      provider: 'Anthropic',
      model: 'claude-3-opus',
      isEnabled: true,
      priority: 1,
      rateLimit: 500,
    },
    {
      id: '5',
      path: '/ollama/chat',
      provider: 'Ollama Local',
      model: 'llama3.1:70b',
      isEnabled: false,
      priority: 3,
      rateLimit: 100,
    },
  ]

  const filteredRoutes = routes.filter(
    (r) =>
      r.path.toLowerCase().includes(searchTerm.toLowerCase()) ||
      r.provider.toLowerCase().includes(searchTerm.toLowerCase())
  )

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <div>
          <Title3>Routes</Title3>
          <Body1>Configuration du routage YARP vers les providers</Body1>
        </div>
        <div className={styles.toolbar}>
          <Input
            className={styles.searchInput}
            contentBefore={<Search24Regular />}
            placeholder="Rechercher une route..."
            value={searchTerm}
            onChange={(_, data) => setSearchTerm(data.value)}
          />
          <Button appearance="primary" icon={<Add24Regular />}>
            Nouvelle route
          </Button>
        </div>
      </div>

      <Card className={styles.tableCard}>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHeaderCell>Path</TableHeaderCell>
              <TableHeaderCell>Provider</TableHeaderCell>
              <TableHeaderCell>Modèle</TableHeaderCell>
              <TableHeaderCell>Rate Limit</TableHeaderCell>
              <TableHeaderCell>Priorité</TableHeaderCell>
              <TableHeaderCell>Actif</TableHeaderCell>
              <TableHeaderCell>Actions</TableHeaderCell>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredRoutes.map((route) => (
              <TableRow key={route.id}>
                <TableCell>
                  <TableCellLayout media={<ArrowRouting24Regular />}>
                    <code className={styles.pathCell}>{route.path}</code>
                  </TableCellLayout>
                </TableCell>
                <TableCell>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <CloudFlow24Regular />
                    {route.provider}
                  </div>
                </TableCell>
                <TableCell>
                  <Badge appearance="outline">{route.model}</Badge>
                </TableCell>
                <TableCell>{route.rateLimit.toLocaleString()} req/min</TableCell>
                <TableCell>
                  <Badge appearance="filled" color={route.priority === 1 ? 'success' : 'informative'}>
                    P{route.priority}
                  </Badge>
                </TableCell>
                <TableCell>
                  <Switch checked={route.isEnabled} />
                </TableCell>
                <TableCell>
                  <Menu>
                    <MenuTrigger disableButtonEnhancement>
                      <Button appearance="subtle" icon={<MoreHorizontal24Regular />} />
                    </MenuTrigger>
                    <MenuPopover>
                      <MenuList>
                        <MenuItem icon={<Edit24Regular />}>Modifier</MenuItem>
                        <MenuItem icon={<Delete24Regular />}>Supprimer</MenuItem>
                      </MenuList>
                    </MenuPopover>
                  </Menu>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>
    </div>
  )
}
