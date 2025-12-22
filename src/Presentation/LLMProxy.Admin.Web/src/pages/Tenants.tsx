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
  ProgressBar,
} from '@fluentui/react-components'
import {
  Add24Regular,
  Search24Regular,
  MoreHorizontal24Regular,
  Edit24Regular,
  Delete24Regular,
  People24Regular,
  Key24Regular,
} from '@fluentui/react-icons'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { fetchTenants, createTenant, updateTenant, deleteTenant } from '@/api/tenants.api'
import type { Tenant, CreateTenantRequest } from '@/types/tenant'

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
  quotaCell: {
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
    minWidth: '200px',
  },
  quotaProgress: {
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
  },
  quotaText: {
    fontSize: '12px',
    color: tokens.colorNeutralForeground3,
    whiteSpace: 'nowrap',
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

export function Tenants() {
  const styles = useStyles()
  const queryClient = useQueryClient()
  const [searchTerm, setSearchTerm] = useState('')
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [editingTenant, setEditingTenant] = useState<Tenant | null>(null)
  const [formData, setFormData] = useState<CreateTenantRequest>({
    name: '',
    slug: '',
    maxRequestsPerMonth: 10000,
    maxTokensPerMonth: 1000000,
  })

  const { data: tenants, isLoading } = useQuery<Tenant[]>({
    queryKey: ['tenants'],
    queryFn: fetchTenants,
  })

  const createMutation = useMutation({
    mutationFn: createTenant,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants'] })
      setIsCreateDialogOpen(false)
      resetForm()
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreateTenantRequest }) =>
      updateTenant(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants'] })
      setEditingTenant(null)
      resetForm()
    },
  })

  const deleteMutation = useMutation({
    mutationFn: deleteTenant,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['tenants'] })
    },
  })

  const resetForm = () => {
    setFormData({
      name: '',
      slug: '',
      maxRequestsPerMonth: 10000,
      maxTokensPerMonth: 1000000,
    })
  }

  const handleCreate = () => {
    createMutation.mutate(formData)
  }

  const handleUpdate = () => {
    if (editingTenant) {
      updateMutation.mutate({ id: editingTenant.id, data: formData })
    }
  }

  const handleEdit = (tenant: Tenant) => {
    setEditingTenant(tenant)
    setFormData({
      name: tenant.name,
      slug: tenant.slug,
      maxRequestsPerMonth: tenant.quota.maxRequestsPerMonth,
      maxTokensPerMonth: tenant.quota.maxTokensPerMonth,
    })
  }

  const handleDelete = (id: string) => {
    if (confirm('Êtes-vous sûr de vouloir supprimer ce tenant ?')) {
      deleteMutation.mutate(id)
    }
  }

  const getQuotaPercentage = (current: number, max: number) => {
    return Math.min((current / max) * 100, 100)
  }

  const getQuotaColor = (percentage: number): 'brand' | 'warning' | 'error' => {
    if (percentage >= 90) return 'error'
    if (percentage >= 75) return 'warning'
    return 'brand'
  }

  // Fallback mock data
  const mockTenants: Tenant[] = [
    {
      id: '1',
      name: 'Acme Corporation',
      slug: 'acme',
      isActive: true,
      quota: {
        maxRequestsPerMonth: 50000,
        maxTokensPerMonth: 5000000,
        currentRequests: 42350,
        currentTokens: 3850000,
      },
      apiKeysCount: 5,
      requestsThisMonth: 42350,
      createdAt: '2024-01-10T08:00:00Z',
    },
    {
      id: '2',
      name: 'TechStartup Inc',
      slug: 'techstartup',
      isActive: true,
      quota: {
        maxRequestsPerMonth: 20000,
        maxTokensPerMonth: 2000000,
        currentRequests: 8420,
        currentTokens: 720000,
      },
      apiKeysCount: 3,
      requestsThisMonth: 8420,
      createdAt: '2024-02-15T14:30:00Z',
    },
    {
      id: '3',
      name: 'Enterprise Solutions',
      slug: 'enterprise',
      isActive: true,
      quota: {
        maxRequestsPerMonth: 100000,
        maxTokensPerMonth: 10000000,
        currentRequests: 67850,
        currentTokens: 8540000,
      },
      apiKeysCount: 12,
      requestsThisMonth: 67850,
      createdAt: '2024-01-05T10:00:00Z',
    },
    {
      id: '4',
      name: 'Demo Account',
      slug: 'demo',
      isActive: false,
      quota: {
        maxRequestsPerMonth: 1000,
        maxTokensPerMonth: 100000,
        currentRequests: 0,
        currentTokens: 0,
      },
      apiKeysCount: 1,
      requestsThisMonth: 0,
      createdAt: '2024-03-01T12:00:00Z',
    },
  ]

  const displayTenants = tenants || mockTenants
  const filteredTenants = displayTenants.filter(
    (t) =>
      t.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      t.slug.toLowerCase().includes(searchTerm.toLowerCase())
  )

  if (isLoading) {
    return (
      <div className={styles.loadingContainer}>
        <Spinner size="large" label="Chargement des tenants..." />
      </div>
    )
  }

  const TenantForm = () => (
    <DialogContent>
      <div className={styles.formGrid}>
        <Field label="Nom" required>
          <Input
            value={formData.name}
            onChange={(_, data) => setFormData({ ...formData, name: data.value })}
            placeholder="Ex: Acme Corporation"
          />
        </Field>
        <Field label="Slug" required>
          <Input
            value={formData.slug}
            onChange={(_, data) => setFormData({ ...formData, slug: data.value })}
            placeholder="Ex: acme"
          />
        </Field>
        <Field label="Quota requêtes / mois" required>
          <Input
            type="number"
            value={formData.maxRequestsPerMonth.toString()}
            onChange={(_, data) =>
              setFormData({ ...formData, maxRequestsPerMonth: parseInt(data.value) || 0 })
            }
          />
        </Field>
        <Field label="Quota tokens / mois" required>
          <Input
            type="number"
            value={formData.maxTokensPerMonth.toString()}
            onChange={(_, data) =>
              setFormData({ ...formData, maxTokensPerMonth: parseInt(data.value) || 0 })
            }
          />
        </Field>
      </div>
    </DialogContent>
  )

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <div>
          <Title3>Tenants</Title3>
          <Body1>Gérez vos organisations et leurs quotas</Body1>
        </div>
        <div className={styles.toolbar}>
          <Input
            className={styles.searchInput}
            contentBefore={<Search24Regular />}
            placeholder="Rechercher un tenant..."
            value={searchTerm}
            onChange={(_, data) => setSearchTerm(data.value)}
          />
          <Dialog open={isCreateDialogOpen} onOpenChange={(_, data) => setIsCreateDialogOpen(data.open)}>
            <DialogTrigger disableButtonEnhancement>
              <Button appearance="primary" icon={<Add24Regular />}>
                Nouveau tenant
              </Button>
            </DialogTrigger>
            <DialogSurface>
              <DialogBody>
                <DialogTitle>Nouveau Tenant</DialogTitle>
                <TenantForm />
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
              <TableHeaderCell>Tenant</TableHeaderCell>
              <TableHeaderCell>Statut</TableHeaderCell>
              <TableHeaderCell>Clés API</TableHeaderCell>
              <TableHeaderCell>Quota Requêtes</TableHeaderCell>
              <TableHeaderCell>Quota Tokens</TableHeaderCell>
              <TableHeaderCell>Actions</TableHeaderCell>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filteredTenants.map((tenant) => {
              const requestPercentage = getQuotaPercentage(
                tenant.quota.currentRequests,
                tenant.quota.maxRequestsPerMonth
              )
              const tokenPercentage = getQuotaPercentage(
                tenant.quota.currentTokens,
                tenant.quota.maxTokensPerMonth
              )

              return (
                <TableRow key={tenant.id}>
                  <TableCell>
                    <TableCellLayout media={<People24Regular />}>
                      <div>
                        <Text weight="semibold">{tenant.name}</Text>
                        <Body1 style={{ color: tokens.colorNeutralForeground3 }}>
                          @{tenant.slug}
                        </Body1>
                      </div>
                    </TableCellLayout>
                  </TableCell>
                  <TableCell>
                    <Badge appearance="filled" color={tenant.isActive ? 'success' : 'danger'}>
                      {tenant.isActive ? 'Actif' : 'Inactif'}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '4px' }}>
                      <Key24Regular />
                      {tenant.apiKeysCount}
                    </div>
                  </TableCell>
                  <TableCell>
                    <div className={styles.quotaCell}>
                      <div className={styles.quotaProgress}>
                        <ProgressBar
                          value={requestPercentage / 100}
                          color={getQuotaColor(requestPercentage)}
                          style={{ flex: 1 }}
                        />
                        <Text className={styles.quotaText}>{requestPercentage.toFixed(0)}%</Text>
                      </div>
                      <Text className={styles.quotaText}>
                        {tenant.quota.currentRequests.toLocaleString()} /{' '}
                        {tenant.quota.maxRequestsPerMonth.toLocaleString()}
                      </Text>
                    </div>
                  </TableCell>
                  <TableCell>
                    <div className={styles.quotaCell}>
                      <div className={styles.quotaProgress}>
                        <ProgressBar
                          value={tokenPercentage / 100}
                          color={getQuotaColor(tokenPercentage)}
                          style={{ flex: 1 }}
                        />
                        <Text className={styles.quotaText}>{tokenPercentage.toFixed(0)}%</Text>
                      </div>
                      <Text className={styles.quotaText}>
                        {(tenant.quota.currentTokens / 1000000).toFixed(1)}M /{' '}
                        {(tenant.quota.maxTokensPerMonth / 1000000).toFixed(1)}M
                      </Text>
                    </div>
                  </TableCell>
                  <TableCell>
                    <Menu>
                      <MenuTrigger disableButtonEnhancement>
                        <Button appearance="subtle" icon={<MoreHorizontal24Regular />} />
                      </MenuTrigger>
                      <MenuPopover>
                        <MenuList>
                          <MenuItem icon={<Edit24Regular />} onClick={() => handleEdit(tenant)}>
                            Modifier
                          </MenuItem>
                          <MenuItem icon={<Key24Regular />}>Gérer les clés API</MenuItem>
                          <MenuItem
                            icon={<Delete24Regular />}
                            onClick={() => handleDelete(tenant.id)}
                          >
                            Supprimer
                          </MenuItem>
                        </MenuList>
                      </MenuPopover>
                    </Menu>
                  </TableCell>
                </TableRow>
              )
            })}
          </TableBody>
        </Table>
      </Card>

      {/* Edit Dialog */}
      <Dialog open={!!editingTenant} onOpenChange={(_, data) => !data.open && setEditingTenant(null)}>
        <DialogSurface>
          <DialogBody>
            <DialogTitle>Modifier le Tenant</DialogTitle>
            <TenantForm />
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
