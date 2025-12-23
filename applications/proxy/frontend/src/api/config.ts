/**
 * Configuration de l'API client
 * Centralise les URLs et paramètres de connexion backend
 */

export const API_CONFIG = {
  // Base URL de l'Admin API (via proxy Vite /api -> http://localhost:5001)
  BASE_URL: import.meta.env.VITE_API_BASE_URL || '/api',
  
  // Version de l'API (détectée par namespace dans le backend)
  API_VERSION: import.meta.env.VITE_API_VERSION || 'v2025-12-22',
  
  // Timeout des requêtes (ms)
  TIMEOUT: 30000,
  
  // Mode mock pour développement
  USE_MOCK_DATA: import.meta.env.VITE_USE_MOCK_DATA === 'true',
  
  // Headers par défaut
  DEFAULT_HEADERS: {
    'Content-Type': 'application/json',
    'Accept': 'application/json'
  }
} as const

/**
 * Clés de stockage localStorage
 */
export const STORAGE_KEYS = {
  AUTH_TOKEN: 'llmproxy_auth_token',
  USER_INFO: 'llmproxy_user_info',
  REFRESH_TOKEN: 'llmproxy_refresh_token'
} as const

/**
 * Endpoints de l'API
 */
export const API_ENDPOINTS = {
  // Authentication
  AUTH: {
    LOGIN: '/auth/login',
    LOGOUT: '/auth/logout',
    REFRESH: '/auth/refresh',
    ME: '/auth/me'
  },
  
  // Tenants
  TENANTS: {
    BASE: `/api/${API_CONFIG.API_VERSION}/tenants`,
    BY_ID: (id: string) => `/api/${API_CONFIG.API_VERSION}/tenants/${id}`,
    ACTIVATE: (id: string) => `/api/${API_CONFIG.API_VERSION}/tenants/${id}/activate`,
    DEACTIVATE: (id: string) => `/api/${API_CONFIG.API_VERSION}/tenants/${id}/deactivate`,
    SETTINGS: (id: string) => `/api/${API_CONFIG.API_VERSION}/tenants/${id}/settings`
  },
  
  // Providers
  PROVIDERS: {
    BASE: `/api/${API_CONFIG.API_VERSION}/providers`,
    BY_ID: (id: string) => `/api/${API_CONFIG.API_VERSION}/providers/${id}`,
    BY_TENANT: (tenantId: string) => `/api/${API_CONFIG.API_VERSION}/providers/tenant/${tenantId}`
  },
  
  // Users
  USERS: {
    BASE: `/api/${API_CONFIG.API_VERSION}/users`,
    BY_ID: (id: string) => `/api/${API_CONFIG.API_VERSION}/users/${id}`,
    BY_TENANT: (tenantId: string) => `/api/${API_CONFIG.API_VERSION}/users/tenant/${tenantId}`
  },
  
  // API Keys
  API_KEYS: {
    BASE: `/api/${API_CONFIG.API_VERSION}/apikeys`,
    BY_USER: (userId: string) => `/api/${API_CONFIG.API_VERSION}/apikeys/user/${userId}`,
    BY_TENANT: (tenantId: string) => `/api/${API_CONFIG.API_VERSION}/apikeys/tenant/${tenantId}`,
    REVOKE: (id: string) => `/api/${API_CONFIG.API_VERSION}/apikeys/${id}/revoke`
  }
} as const
