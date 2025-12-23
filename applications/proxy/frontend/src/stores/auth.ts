import { ref, computed } from 'vue'
import { defineStore } from 'pinia'
import type { User, LoginRequest, LoginResponse } from '@/types'
import { login as apiLogin, logout as apiLogout, refreshToken as apiRefreshToken, getCurrentUser } from '@/api/auth'

const TOKEN_KEY = 'auth_token'
const REFRESH_TOKEN_KEY = 'auth_refresh_token'
const USER_KEY = 'auth_user'

/**
 * Store Pinia pour la gestion de l'authentification.
 */
export const useAuthStore = defineStore('auth', () => {
  // État
  const user = ref<User | null>(loadUserFromStorage())
  const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
  const refreshTokenValue = ref<string | null>(localStorage.getItem(REFRESH_TOKEN_KEY))
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // Getters calculés
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  
  const isAdmin = computed(() => user.value?.role === 'admin')
  
  const isTenantAdmin = computed(() => 
    user.value?.role === 'admin' || user.value?.role === 'tenant-admin'
  )

  const userName = computed(() => user.value?.name ?? '')

  const userInitials = computed(() => {
    if (!user.value?.name) return ''
    const parts = user.value.name.split(' ')
    if (parts.length >= 2) {
      return `${parts[0][0]}${parts[1][0]}`.toUpperCase()
    }
    return parts[0].substring(0, 2).toUpperCase()
  })

  // Fonctions utilitaires
  function loadUserFromStorage(): User | null {
    const stored = localStorage.getItem(USER_KEY)
    if (!stored) return null
    try {
      return JSON.parse(stored) as User
    } catch {
      return null
    }
  }

  function saveToStorage(loginResponse: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, loginResponse.token)
    localStorage.setItem(REFRESH_TOKEN_KEY, loginResponse.refreshToken)
    localStorage.setItem(USER_KEY, JSON.stringify(loginResponse.user))
  }

  function clearStorage(): void {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  }

  // Actions
  async function login(credentials: LoginRequest): Promise<boolean> {
    isLoading.value = true
    error.value = null

    try {
      const response = await apiLogin(credentials)
      
      // Sauvegarder les données
      saveToStorage(response)
      
      // Mettre à jour l'état
      token.value = response.token
      refreshTokenValue.value = response.refreshToken
      user.value = response.user
      
      return true
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors de la connexion'
      console.error('Erreur login:', err)
      return false
    } finally {
      isLoading.value = false
    }
  }

  async function logout(): Promise<void> {
    isLoading.value = true
    error.value = null

    try {
      await apiLogout()
    } catch (err) {
      console.error('Erreur logout:', err)
    } finally {
      // Nettoyer l'état même en cas d'erreur
      clearStorage()
      token.value = null
      refreshTokenValue.value = null
      user.value = null
      isLoading.value = false
    }
  }

  async function refreshToken(): Promise<boolean> {
    if (!refreshTokenValue.value) {
      return false
    }

    try {
      const response = await apiRefreshToken({ refreshToken: refreshTokenValue.value })
      
      saveToStorage(response)
      token.value = response.token
      refreshTokenValue.value = response.refreshToken
      user.value = response.user
      
      return true
    } catch (err) {
      console.error('Erreur refresh token:', err)
      // En cas d'erreur, déconnecter
      await logout()
      return false
    }
  }

  async function fetchCurrentUser(): Promise<void> {
    if (!token.value) return

    isLoading.value = true
    error.value = null

    try {
      user.value = await getCurrentUser()
      localStorage.setItem(USER_KEY, JSON.stringify(user.value))
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Erreur lors de la récupération du profil'
      console.error('Erreur profil:', err)
    } finally {
      isLoading.value = false
    }
  }

  function clearError(): void {
    error.value = null
  }

  // Vérifier si le token est expiré (simplifié)
  function isTokenExpired(): boolean {
    if (!token.value) return true
    
    try {
      // Décoder le payload JWT (partie 2)
      const payload = JSON.parse(atob(token.value.split('.')[1]))
      const expirationTime = payload.exp * 1000 // Convertir en ms
      return Date.now() >= expirationTime
    } catch {
      return true
    }
  }

  return {
    // État
    user,
    token,
    isLoading,
    error,
    
    // Getters
    isAuthenticated,
    isAdmin,
    isTenantAdmin,
    userName,
    userInitials,
    
    // Actions
    login,
    logout,
    refreshToken,
    fetchCurrentUser,
    clearError,
    isTokenExpired,
  }
})
