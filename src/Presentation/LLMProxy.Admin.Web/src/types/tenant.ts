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

export interface TenantQuota {
  maxRequestsPerMonth: number
  maxTokensPerMonth: number
  currentRequests: number
  currentTokens: number
}

export interface CreateTenantRequest {
  name: string
  slug: string
  maxRequestsPerMonth: number
  maxTokensPerMonth: number
}
