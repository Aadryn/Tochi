/**
 * Types pour l'authentification et la gestion des utilisateurs.
 */

/**
 * Représente un utilisateur authentifié.
 */
export interface User {
  id: string
  email: string
  name: string
  role: UserRole
  tenantId?: string
  tenantName?: string
  createdAt: string
  lastLoginAt?: string
}

/**
 * Rôles utilisateur possibles.
 */
export type UserRole = 'admin' | 'tenant-admin' | 'user'

/**
 * Requête de connexion.
 */
export interface LoginRequest {
  email: string
  password: string
}

/**
 * Réponse de connexion réussie.
 */
export interface LoginResponse {
  token: string
  refreshToken: string
  expiresIn: number
  user: User
}

/**
 * Requête de rafraîchissement du token.
 */
export interface RefreshTokenRequest {
  refreshToken: string
}

/**
 * Requête de changement de mot de passe.
 */
export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}

/**
 * Requête de réinitialisation de mot de passe.
 */
export interface ResetPasswordRequest {
  email: string
}

/**
 * État d'authentification du store.
 */
export interface AuthState {
  user: User | null
  token: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
}
