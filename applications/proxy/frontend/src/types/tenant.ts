/**
 * Représente un tenant (client) du système.
 */
export interface Tenant {
  id: string
  name: string
  slug: string
  isActive: boolean
  quota: TenantQuota
  apiKeysCount: number
  requestsThisMonth: number
  createdAt: string
}

/**
 * Quota d'utilisation d'un tenant.
 */
export interface TenantQuota {
  maxRequestsPerMonth: number
  maxTokensPerMonth: number
  currentRequests: number
  currentTokens: number
}

/**
 * Requête de création d'un tenant.
 */
export interface CreateTenantRequest {
  name: string
  slug: string
  maxRequestsPerMonth: number
  maxTokensPerMonth: number
  isActive?: boolean
  quota?: {
    maxRequestsPerMonth: number
    maxTokensPerMonth: number
  }
}

/**
 * Requête de mise à jour d'un tenant.
 */
export interface UpdateTenantRequest extends Partial<CreateTenantRequest> {
  id: string
  isActive?: boolean
}

/**
 * Clé API d'un tenant.
 */
export interface TenantApiKey {
  id: string
  name: string
  prefix: string
  keyPreview?: string
  createdAt: string
  lastUsedAt?: string
}

/**
 * Résultat de création d'une clé API.
 */
export interface CreateApiKeyResult {
  id: string
  key: string
}
