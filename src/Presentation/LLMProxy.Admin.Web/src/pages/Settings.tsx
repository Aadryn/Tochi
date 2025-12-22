import {
  makeStyles,
  tokens,
  shorthands,
  Card,
  Text,
  Title3,
  Body1,
  Button,
  Input,
  Field,
  Switch,
  Divider,
  Accordion,
  AccordionItem,
  AccordionHeader,
  AccordionPanel,
} from '@fluentui/react-components'
import {
  Save24Regular,
  Settings24Regular,
  Shield24Regular,
  Database24Regular,
  Globe24Regular,
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
  settingsCard: {
    ...shorthands.padding('0'),
  },
  settingSection: {
    ...shorthands.padding('20px'),
  },
  settingRow: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    ...shorthands.padding('12px', '0'),
  },
  settingInfo: {
    display: 'flex',
    flexDirection: 'column',
    gap: '4px',
  },
  settingDescription: {
    color: tokens.colorNeutralForeground3,
    fontSize: '13px',
  },
  formGrid: {
    display: 'grid',
    gridTemplateColumns: '1fr 1fr',
    gap: '16px',
    marginTop: '16px',
  },
  formFieldFull: {
    gridColumn: '1 / -1',
  },
  sectionIcon: {
    display: 'flex',
    alignItems: 'center',
    gap: '8px',
  },
})

export function Settings() {
  const styles = useStyles()
  const [settings, setSettings] = useState({
    // General
    instanceName: 'LLMProxy Production',
    adminEmail: 'admin@company.com',
    
    // Security
    requireApiKey: true,
    enableRateLimiting: true,
    maxRequestsPerMinute: 1000,
    enableAuditLog: true,
    
    // Database
    connectionString: 'Host=localhost;Port=15432;Database=development',
    maxPoolSize: 100,
    
    // CORS
    allowedOrigins: '*',
    allowCredentials: false,
  })

  const handleSave = () => {
    console.log('Saving settings:', settings)
    // API call to save settings
  }

  return (
    <div className={styles.root}>
      <div className={styles.header}>
        <div>
          <Title3>Paramètres</Title3>
          <Body1>Configuration globale de LLMProxy</Body1>
        </div>
        <Button appearance="primary" icon={<Save24Regular />} onClick={handleSave}>
          Enregistrer
        </Button>
      </div>

      <Card className={styles.settingsCard}>
        <Accordion multiple defaultOpenItems={['general', 'security']}>
          <AccordionItem value="general">
            <AccordionHeader>
              <div className={styles.sectionIcon}>
                <Settings24Regular />
                <Text weight="semibold">Général</Text>
              </div>
            </AccordionHeader>
            <AccordionPanel>
              <div className={styles.settingSection}>
                <div className={styles.formGrid}>
                  <Field label="Nom de l'instance">
                    <Input
                      value={settings.instanceName}
                      onChange={(_, data) =>
                        setSettings({ ...settings, instanceName: data.value })
                      }
                    />
                  </Field>
                  <Field label="Email administrateur">
                    <Input
                      type="email"
                      value={settings.adminEmail}
                      onChange={(_, data) =>
                        setSettings({ ...settings, adminEmail: data.value })
                      }
                    />
                  </Field>
                </div>
              </div>
            </AccordionPanel>
          </AccordionItem>

          <AccordionItem value="security">
            <AccordionHeader>
              <div className={styles.sectionIcon}>
                <Shield24Regular />
                <Text weight="semibold">Sécurité</Text>
              </div>
            </AccordionHeader>
            <AccordionPanel>
              <div className={styles.settingSection}>
                <div className={styles.settingRow}>
                  <div className={styles.settingInfo}>
                    <Text weight="semibold">Authentification par clé API</Text>
                    <Text className={styles.settingDescription}>
                      Exiger une clé API valide pour toutes les requêtes
                    </Text>
                  </div>
                  <Switch
                    checked={settings.requireApiKey}
                    onChange={(_, data) =>
                      setSettings({ ...settings, requireApiKey: data.checked })
                    }
                  />
                </div>
                <Divider />
                <div className={styles.settingRow}>
                  <div className={styles.settingInfo}>
                    <Text weight="semibold">Rate Limiting</Text>
                    <Text className={styles.settingDescription}>
                      Limiter le nombre de requêtes par tenant
                    </Text>
                  </div>
                  <Switch
                    checked={settings.enableRateLimiting}
                    onChange={(_, data) =>
                      setSettings({ ...settings, enableRateLimiting: data.checked })
                    }
                  />
                </div>
                {settings.enableRateLimiting && (
                  <div style={{ marginTop: '12px' }}>
                    <Field label="Requêtes max par minute (global)">
                      <Input
                        type="number"
                        value={settings.maxRequestsPerMinute.toString()}
                        onChange={(_, data) =>
                          setSettings({
                            ...settings,
                            maxRequestsPerMinute: parseInt(data.value) || 0,
                          })
                        }
                      />
                    </Field>
                  </div>
                )}
                <Divider />
                <div className={styles.settingRow}>
                  <div className={styles.settingInfo}>
                    <Text weight="semibold">Journal d'audit</Text>
                    <Text className={styles.settingDescription}>
                      Enregistrer toutes les opérations administratives
                    </Text>
                  </div>
                  <Switch
                    checked={settings.enableAuditLog}
                    onChange={(_, data) =>
                      setSettings({ ...settings, enableAuditLog: data.checked })
                    }
                  />
                </div>
              </div>
            </AccordionPanel>
          </AccordionItem>

          <AccordionItem value="database">
            <AccordionHeader>
              <div className={styles.sectionIcon}>
                <Database24Regular />
                <Text weight="semibold">Base de données</Text>
              </div>
            </AccordionHeader>
            <AccordionPanel>
              <div className={styles.settingSection}>
                <div className={styles.formGrid}>
                  <Field label="Chaîne de connexion" className={styles.formFieldFull}>
                    <Input
                      value={settings.connectionString}
                      onChange={(_, data) =>
                        setSettings({ ...settings, connectionString: data.value })
                      }
                    />
                  </Field>
                  <Field label="Taille max du pool">
                    <Input
                      type="number"
                      value={settings.maxPoolSize.toString()}
                      onChange={(_, data) =>
                        setSettings({
                          ...settings,
                          maxPoolSize: parseInt(data.value) || 0,
                        })
                      }
                    />
                  </Field>
                </div>
              </div>
            </AccordionPanel>
          </AccordionItem>

          <AccordionItem value="cors">
            <AccordionHeader>
              <div className={styles.sectionIcon}>
                <Globe24Regular />
                <Text weight="semibold">CORS</Text>
              </div>
            </AccordionHeader>
            <AccordionPanel>
              <div className={styles.settingSection}>
                <div className={styles.formGrid}>
                  <Field label="Origines autorisées" className={styles.formFieldFull}>
                    <Input
                      value={settings.allowedOrigins}
                      onChange={(_, data) =>
                        setSettings({ ...settings, allowedOrigins: data.value })
                      }
                      placeholder="* ou liste d'origines séparées par des virgules"
                    />
                  </Field>
                </div>
                <div className={styles.settingRow}>
                  <div className={styles.settingInfo}>
                    <Text weight="semibold">Autoriser les credentials</Text>
                    <Text className={styles.settingDescription}>
                      Inclure les cookies et headers d'authentification dans les requêtes CORS
                    </Text>
                  </div>
                  <Switch
                    checked={settings.allowCredentials}
                    onChange={(_, data) =>
                      setSettings({ ...settings, allowCredentials: data.checked })
                    }
                  />
                </div>
              </div>
            </AccordionPanel>
          </AccordionItem>
        </Accordion>
      </Card>
    </div>
  )
}
