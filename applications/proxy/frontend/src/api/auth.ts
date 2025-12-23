import { apiClient } from './client'
import type { 
  LoginRequest, 
  LoginResponse, 
  RefreshTokenRequest, 
  User,
  ChangePasswordRequest 
} from '@/types'

/**
 * Effectue la connexion utilisateur.
 */
export async function login(credentials: LoginRequest): Promise<LoginResponse> {
  const response = await apiClient.post<LoginResponse>('/auth/login', credentials)
  return response.data
}

/**
 * Déconnecte l'utilisateur.
 */
export async function logout(): Promise<void> {
  await apiClient.post('/auth/logout')
}

/**
 * Rafraîchit le token d'authentification.
 */
export async function refreshToken(request: RefreshTokenRequest): Promise<LoginResponse> {
  const response = await apiClient.post<LoginResponse>('/auth/refresh', request)
  return response.data
}

/**
 * Récupère les informations de l'utilisateur courant.
 */
export async function getCurrentUser(): Promise<User> {
  const response = await apiClient.get<User>('/auth/me')
  return response.data
}

/**
 * Change le mot de passe de l'utilisateur courant.
 */
export async function changePassword(request: ChangePasswordRequest): Promise<void> {
  await apiClient.post('/auth/change-password', request)
}

/**
 * Demande la réinitialisation du mot de passe.
 */
export async function requestPasswordReset(email: string): Promise<void> {
  await apiClient.post('/auth/reset-password', { email })
}

/**
 * Retourne des données mock pour le développement.
 */
export function getMockLoginResponse(email: string): LoginResponse {
  return {
    token: 'mock-jwt-token-' + Date.now(),
    refreshToken: 'mock-refresh-token-' + Date.now(),
    expiresIn: 3600,
    user: {
      id: '1',
      email,
      name: email.split('@')[0].replace('.', ' '),
      role: email.includes('admin') ? 'admin' : 'tenant-admin',
      tenantId: email.includes('admin') ? undefined : 'tenant-1',
      tenantName: email.includes('admin') ? undefined : 'Acme Corp',
      createdAt: '2024-01-01T00:00:00Z',
      lastLoginAt: new Date().toISOString(),
    },
  }
}

/**
 * Version mock de la fonction login pour le développement.
 */
export async function mockLogin(credentials: LoginRequest): Promise<LoginResponse> {
  // Simuler un délai réseau
  await new Promise(resolve => setTimeout(resolve, 800))
  
  // Validation simple
  if (!credentials.email || !credentials.password) {
    throw new Error('Email et mot de passe requis')
  }
  
  if (credentials.password.length < 4) {
    throw new Error('Identifiants invalides')
  }
  
  return getMockLoginResponse(credentials.email)
}
