<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores'
import { mockLogin } from '@/api/auth'

const router = useRouter()
const authStore = useAuthStore()

const email = ref('')
const password = ref('')
const showPassword = ref(false)
const rememberMe = ref(false)
const isSubmitting = ref(false)
const errorMessage = ref('')

const isFormValid = computed(() => {
  return email.value.includes('@') && password.value.length >= 4
})

async function handleSubmit(): Promise<void> {
  if (!isFormValid.value || isSubmitting.value) return
  
  isSubmitting.value = true
  errorMessage.value = ''
  
  try {
    // En développement, utiliser le mock
    const response = await mockLogin({
      email: email.value,
      password: password.value,
    })
    
    // Sauvegarder manuellement pour le mock
    localStorage.setItem('auth_token', response.token)
    localStorage.setItem('auth_refresh_token', response.refreshToken)
    localStorage.setItem('auth_user', JSON.stringify(response.user))
    
    // Rediriger vers le dashboard
    router.push({ name: 'dashboard' })
  } catch (err) {
    errorMessage.value = err instanceof Error ? err.message : 'Erreur lors de la connexion'
  } finally {
    isSubmitting.value = false
  }
}

function togglePasswordVisibility(): void {
  showPassword.value = !showPassword.value
}
</script>

<template>
  <div class="login-page">
    <div class="login-container">
      <!-- Logo et titre -->
      <div class="login-header">
        <div class="logo">
          <i class="pi pi-box"></i>
        </div>
        <h1 class="title">LLMProxy Admin</h1>
        <p class="subtitle">Connectez-vous pour accéder au panneau d'administration</p>
      </div>
      
      <!-- Formulaire -->
      <form class="login-form" @submit.prevent="handleSubmit">
        <!-- Message d'erreur -->
        <div v-if="errorMessage" class="error-alert">
          <i class="pi pi-exclamation-circle"></i>
          <span>{{ errorMessage }}</span>
        </div>
        
        <!-- Email -->
        <div class="form-field">
          <label for="email" class="field-label">Adresse email</label>
          <div class="input-wrapper">
            <i class="pi pi-envelope input-icon"></i>
            <input
              id="email"
              v-model="email"
              type="email"
              placeholder="votre@email.com"
              autocomplete="email"
              class="form-input"
              :class="{ 'has-error': errorMessage && !email }"
            />
          </div>
        </div>
        
        <!-- Mot de passe -->
        <div class="form-field">
          <label for="password" class="field-label">Mot de passe</label>
          <div class="input-wrapper">
            <i class="pi pi-lock input-icon"></i>
            <input
              id="password"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="Entrez votre mot de passe"
              autocomplete="current-password"
              class="form-input"
              :class="{ 'has-error': errorMessage && !password }"
            />
            <button
              type="button"
              class="toggle-password"
              @click="togglePasswordVisibility"
              tabindex="-1"
            >
              <i :class="showPassword ? 'pi pi-eye-slash' : 'pi pi-eye'"></i>
            </button>
          </div>
        </div>
        
        <!-- Options -->
        <div class="form-options">
          <label class="checkbox-label">
            <input v-model="rememberMe" type="checkbox" class="checkbox" />
            <span>Se souvenir de moi</span>
          </label>
          <a href="#" class="forgot-link">Mot de passe oublié ?</a>
        </div>
        
        <!-- Bouton de connexion -->
        <button
          type="submit"
          class="submit-btn"
          :disabled="!isFormValid || isSubmitting"
        >
          <i v-if="isSubmitting" class="pi pi-spinner pi-spin"></i>
          <span v-else>Se connecter</span>
        </button>
      </form>
      
      <!-- Info développement -->
      <div class="dev-info">
        <p>Mode développement - Identifiants de test :</p>
        <code>admin@llmproxy.io / test1234</code>
      </div>
    </div>
    
    <!-- Fond décoratif -->
    <div class="login-background">
      <div class="bg-shape bg-shape-1"></div>
      <div class="bg-shape bg-shape-2"></div>
      <div class="bg-shape bg-shape-3"></div>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, var(--surface-ground) 0%, var(--surface-section) 100%);
  position: relative;
  overflow: hidden;
}

.login-container {
  width: 100%;
  max-width: 420px;
  padding: 2.5rem;
  background: var(--surface-card);
  border-radius: 16px;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.08);
  position: relative;
  z-index: 1;
}

.login-header {
  text-align: center;
  margin-bottom: 2rem;
}

.logo {
  width: 64px;
  height: 64px;
  background: var(--primary-color);
  border-radius: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1rem;
}

.logo i {
  font-size: 2rem;
  color: white;
}

.title {
  font-size: 1.5rem;
  font-weight: 700;
  color: var(--text-color);
  margin: 0 0 0.5rem;
}

.subtitle {
  font-size: 0.875rem;
  color: var(--text-color-secondary);
  margin: 0;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.error-alert {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.75rem 1rem;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 8px;
  color: #dc2626;
  font-size: 0.875rem;
}

.error-alert i {
  font-size: 1rem;
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.field-label {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--text-color);
}

.input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.input-icon {
  position: absolute;
  left: 1rem;
  color: var(--text-color-secondary);
  font-size: 1rem;
  pointer-events: none;
}

.form-input {
  width: 100%;
  padding: 0.875rem 1rem 0.875rem 2.75rem;
  font-size: 0.9375rem;
  border: 1px solid var(--surface-border);
  border-radius: 8px;
  background: var(--surface-ground);
  color: var(--text-color);
  transition: border-color 0.2s, box-shadow 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 3px rgba(var(--primary-color-rgb, 99, 102, 241), 0.1);
}

.form-input.has-error {
  border-color: #dc2626;
}

.form-input::placeholder {
  color: var(--text-color-secondary);
}

.toggle-password {
  position: absolute;
  right: 0.75rem;
  padding: 0.5rem;
  border: none;
  background: transparent;
  color: var(--text-color-secondary);
  cursor: pointer;
  border-radius: 4px;
  transition: color 0.2s;
}

.toggle-password:hover {
  color: var(--text-color);
}

.form-options {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.checkbox-label {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-size: 0.875rem;
  color: var(--text-color);
  cursor: pointer;
}

.checkbox {
  width: 1rem;
  height: 1rem;
  accent-color: var(--primary-color);
}

.forgot-link {
  font-size: 0.875rem;
  color: var(--primary-color);
  text-decoration: none;
}

.forgot-link:hover {
  text-decoration: underline;
}

.submit-btn {
  width: 100%;
  padding: 0.875rem;
  font-size: 1rem;
  font-weight: 600;
  color: white;
  background: var(--primary-color);
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: background-color 0.2s, transform 0.1s;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.submit-btn:hover:not(:disabled) {
  background: var(--primary-600, #4f46e5);
}

.submit-btn:active:not(:disabled) {
  transform: scale(0.98);
}

.submit-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.dev-info {
  margin-top: 1.5rem;
  padding: 1rem;
  background: var(--surface-ground);
  border-radius: 8px;
  text-align: center;
  font-size: 0.8125rem;
  color: var(--text-color-secondary);
}

.dev-info code {
  display: block;
  margin-top: 0.5rem;
  font-family: 'Fira Code', monospace;
  color: var(--primary-color);
}

/* Fond décoratif */
.login-background {
  position: absolute;
  inset: 0;
  overflow: hidden;
  pointer-events: none;
}

.bg-shape {
  position: absolute;
  border-radius: 50%;
  background: var(--primary-color);
  opacity: 0.05;
}

.bg-shape-1 {
  width: 600px;
  height: 600px;
  top: -200px;
  right: -200px;
}

.bg-shape-2 {
  width: 400px;
  height: 400px;
  bottom: -100px;
  left: -100px;
}

.bg-shape-3 {
  width: 200px;
  height: 200px;
  top: 50%;
  left: 20%;
  opacity: 0.03;
}

/* Responsive */
@media (max-width: 480px) {
  .login-container {
    margin: 1rem;
    padding: 1.5rem;
  }
  
  .title {
    font-size: 1.25rem;
  }
}
</style>
