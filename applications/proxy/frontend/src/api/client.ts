import axios from 'axios'
import { API_CONFIG } from './config'

/**
 * Interface pour les réponses API avec Result pattern (backend C#)
 */
export interface ApiResult<T> {
  value?: T
  isSuccess: boolean
  error?: {
    code: string
    message: string
  }
}

/**
 * Client Axios configuré pour l'API Admin de LLMProxy.
 * Le proxy Vite redirige les appels /api vers http://localhost:5001
 */
export const apiClient = axios.create({
  baseURL: API_CONFIG.BASE_URL,
  timeout: API_CONFIG.TIMEOUT,
  headers: API_CONFIG.DEFAULT_HEADERS,
})

// Intercepteur pour ajouter le token d'authentification
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('auth_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Intercepteur pour gérer les erreurs globalement
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Rediriger vers la page de connexion si non authentifié
      localStorage.removeItem('auth_token')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  },
)
