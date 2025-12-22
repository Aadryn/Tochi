import {
  makeStyles,
  tokens,
  shorthands,
  Card,
  Text,
  Title3,
  Body1,
  Button,
  Badge,
  Input,
  Dialog,
  DialogTrigger,
  DialogSurface,
  DialogTitle,
  DialogBody,
  DialogContent,
  DialogActions,
  Field,
  Spinner,
  Menu,
  MenuTrigger,
  MenuPopover,
  MenuList,
  MenuItem,
  Table,
  TableHeader,
  TableRow,
  TableHeaderCell,
  TableBody,
  TableCell,
  TableCellLayout,
} from '@fluentui/react-components'
import {
  Add24Regular,
  Search24Regular,
  MoreHorizontal24Regular,
  Edit24Regular,
  Delete24Regular,
  CloudFlow24Regular,
  CheckmarkCircle24Filled,
  ErrorCircle24Filled,
  Warning24Filled,
} from '@fluentui/react-icons'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { fetchProviders, createProvider, updateProvider, deleteProvider } from '@/api/providers.api'
import type { Provider, CreateProviderRequest } from '@/types/provider'

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
  statusIcon: {
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
  },
  loadingContainer: {
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    minHeight: '200px',
  },
  formGrid: {
    display: 'grid',
    gridTemplateColumns: '1fr 1fr',
    gap: '16px',
  },
  formFieldFull: {
    gridColumn: '1 / -1',
  },
})

export function Providers() {
  const styles = useStyles()
  const queryClient = useQueryClient()
  const [searchTerm, setSearchTerm] = useState('')
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [editingProvider, setEditingProvider] = useState<Provider | null>(null)
  const [formData, setFormData] = useState<CreateProviderRequest>({
    name: '',
    type: '',
    baseUrl: '',
    apiKey: '',
    model: '',
    isEnabled: true,
  })

  const { data: providers, isLoading } = useQuery<Provider[]>({
    queryKey: ['providers'],
    queryFn: fetchProviders,
  })

  const createMutation = useMutation({
    mutationFn: createProvider,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['providers'] })
      setIsCreateDialogOpen(false)
      resetForm()
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreateProviderRequest }) =>
      updateProvider(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['providers'] })
      setEditingProvider(null)
      resetForm()
    },
  })

  const deleteMutation = useMutation({
    mutationFn: deleteProvider,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['providers'] })
    },
  })

  const resetForm = () => {
    setFormData({
      name: '',
      type: '',
      baseUrl: '',
      apiKey: '',
      model: '',
      isEnabled: true,
    })
  }

  const handleCreate = () => {
    createMutation.mutate(formData)
  }

  const handleUpdate = () => {
    if (editingProvider) {
      updateMutation.mutate({ id: editingProvider.id, data: formData })
    }
  }

  const handleEdit = (provider: Provider) => {
    setEditingProvider(provider)
    setFormData({
      name: provider.name,
      type: provider.type,
      baseUrl: provider.baseUrl,
      apiKey: '', // Don't pre-fill API key for security
      model: provider.model,
      isEnabled: provider.isEnabled,
    })
  }

  const handleDelete = (id: string) => {
    if (confirm('Êtes-vous sûr de vouloir supprimer ce provider ?')) {
      deleteMutation.mutate(id)
    }
  }

  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'healthy':
        return <CheckmarkCircle24Filled style={{ color: tokens.colorPaletteGreenForeground1 }} />
      case 'degraded':
        return <Warning24Filled style={{ color: tokens.colorPaletteYellowForeground1 }} />
      case 'unhealthy':
        return <ErrorCircle24Filled style={{ color: tokens.colorPaletteRedForeground1 }} />
      default:
        return null
    }
  }

  const getStatusBadgeColor = (status: string): 'success' | 'warning' | 'danger' => {
    switch (status) {
      case 'healthy':
        return 'success'
      case 'degraded':
        return 'warning'
      default:
        return 'danger'
    }
  }

  // Fallback mock data
  const mockProviders: Provider[] = [
    {
      id: '1',
      name: 'OpenAI GPT-4',
      type: 'openai',
      baseUrl: 'https://api.openai.com/v1',
      model: 'gpt-4-turbo',
      isEnabled: true,
      status: 'healthy',
      requestsToday: 45230,
      createdAt: '2024-01-15T10:00:00Z',
    },
    {
      id: '2',
      name: 'Anthropic Claude',
      type: 'anthropic',
      baseUrl: 'https://api.anthropic.com',
      model: 'claude-3-opus',
      isEnabled: true,
      status: 'healthy',
      requestsToday: 32150,
      createdAt: '2024-01-16T14:30:00Z',
    },
    {
      id: '3',
      name: 'Ollama Local',
      type: 'ollama',
      baseUrl: 'http://localhost:11434',
      model: 'llama3.1:70b',
      isEnabled: true,
      status: 'degraded',
      requestsToday: 8420,
      createdAt: '2024-02-01T09:15:00Z',
    },
    {
      id: '4',
      name: 'Azure OpenAI',
      type: 'azure-openai',
      baseUrl: 'https://mycompany.openai.azure.com',
      model: 'gpt-4o',
      isEnabled: true,
      status: 'healthy',
      requestsToday: 28970,
      createdAt: '2024-02-10T16:45:00Z',
    },
  ]

  const displayProviders = providers || mockProviders
  const filteredProviders = displayProviders.filter(
    (p) =>
      p.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      p.type.toLowerCase().includes(searchTerm.toLowerCase())
  )

  if (isLoading) {
    return (
      <div className={styles.loadingContainer}>
        <Spinner size="large" label="Chargement des providers..." />
      </div>
    )
  }

  const ProviderForm = () => (
    <DialogContent>
      <div className={styles.formGrid}>
        <Field label="Nom" required>
          <Input
            value={formData.name}
            onChange={(_, data) => setFormData({ ...formData, name: data.value })}
            placeholder="Ex: OpenAI Production"
          />
        </Field>
        <Field label="Type" required>
          <Input
            value={formData.type}
            onChange={(_, data) => setFormData({ ...formData, type: data.value })}
            placeholder="Ex: openai, anthropic, ollama"
          />
        </Field>
        <Field label="URL de base" required className={styles.formFieldFull}>
          <Input
            value={formData.baseUrl}
            onChange={(_, data) => setFormData({ ...formData, baseUrl: data.value })}
            placeholder="Ex: https://api.openai.com/v1"
          />
        </Field>
        <Field label="Clé API" required={!editingProvider}>
          <Input
            type="password"
            value={formData.apiKey}
            onChange={(_, data) => setFormData({ ...formData, apiKey: data.value })}
            placeholder={editingProvider ? 'Laisser vide pour conserver' : 'sk-...'}
          />
        </Field>
        <Field label="Modèle">
          <Input
            value={formData.model}
            onChange={(_, data) => setFormData({ ...formData, model: data.value })}
            placeholder="Ex: gpt-4-turbo"
          />
        </Field>
      </div>
    </DialogContent>
  )

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <div>
          <Title3>Providers LLM</Title3>
          <Body1>Gérez vos fournisseurs de modèles de langage</Body1>
        </div>
        <div className={styles.toolbar}>
          <Input
            className={styles.searchInput}
            contentBefore={<Search24Regular />}
            placeholder="Rechercher un provider..."
            value={searchTerm}
            onChange={(_, data) => setSearchTerm(data.value)}
          />
          <Dialog open={isCreateDialogOpen} onOpenChange={(_, data) => setIsCreateDialogOpen(data.open)}>
            <DialogTrigger disableButtonEnhancement>
              <Button appearance="primary" icon={<Add24Regular />}>
                Ajouter un provider
              </Button>
            </DialogTrigger>
            <DialogSurface>
              <DialogBody>
                <DialogTitle>Nouveau Provider</DialogTitle>
                <ProviderForm />
                <DialogActions>
                  <DialogTrigger disableButtonEnhancement>
                    <Button appearance="secondary">Annuler</Button>
                  </DialogTrigger>
                  <Button appearance="primary" onClick={handleCreate} disabled={createMutation.isPending}>
                    {createMutation.isPending ? <Spinner size="tiny" /> : 'Créer'}
                  </Button>
                </DialogActions>
              </DialogBody>
            </DialogSurface>
          </Dialog>
        </div>
      </div>

      <Card className={styles.tableCard}>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHeaderCell>Provider</TableHeaderCell>
              <TableHeaderCell>Type</TableHeaderCell>
              <TableHeaderCell>Modèle</TableHeaderCell>
              <TableHeaderCell>Statut</TableHeaderCell>
              <TableHeaderCell>Requêtes (24h)</TableHeaderCell>
              <TableHeaderCell>Actions</TableHeaderCell>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredProviders.map((provider) => (
              <TableRow key={provider.id}>
                <TableCell>
                  <TableCellLayout media={<CloudFlow24Regular />}>
                    <Text weight="semibold">{provider.name}</Text>
                  </TableCellLayout>
                </TableCell>
                <TableCell>
                  <Badge appearance="outline">{provider.type}</Badge>
                </TableCell>
                <TableCell>{provider.model}</TableCell>
                <TableCell>
                  <div className={styles.statusIcon}>
                    {getStatusIcon(provider.status)}
                    <Badge appearance="filled" color={getStatusBadgeColor(provider.status)}>
                      {provider.status === 'healthy'
                        ? 'Sain'
                        : provider.status === 'degraded'
                        ? 'Dégradé'
                        : 'Indisponible'}
                    </Badge>
                  </div>
                </TableCell>
                <TableCell>{provider.requestsToday.toLocaleString()}</TableCell>
                <TableCell>
                  <Menu>
                    <MenuTrigger disableButtonEnhancement>
                      <Button appearance="subtle" icon={<MoreHorizontal24Regular />} />
                    </MenuTrigger>
                    <MenuPopover>
                      <MenuList>
                        <MenuItem icon={<Edit24Regular />} onClick={() => handleEdit(provider)}>
                          Modifier
                        </MenuItem>
                        <MenuItem
                          icon={<Delete24Regular />}
                          onClick={() => handleDelete(provider.id)}
                        >
                          Supprimer
                        </MenuItem>
                      </MenuList>
                    </MenuPopover>
                  </Menu>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </Card>

      {/* Edit Dialog */}
      <Dialog open={!!editingProvider} onOpenChange={(_, data) => !data.open && setEditingProvider(null)}>
        <DialogSurface>
          <DialogBody>
            <DialogTitle>Modifier le Provider</DialogTitle>
            <ProviderForm />
            <DialogActions>
              <DialogTrigger disableButtonEnhancement>
                <Button appearance="secondary">Annuler</Button>
              </DialogTrigger>
              <Button appearance="primary" onClick={handleUpdate} disabled={updateMutation.isPending}>
                {updateMutation.isPending ? <Spinner size="tiny" /> : 'Enregistrer'}
              </Button>
            </DialogActions>
          </DialogBody>
        </DialogSurface>
      </Dialog>
    </div>
  )
}
