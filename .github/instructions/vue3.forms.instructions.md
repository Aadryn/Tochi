---
description: Vue 3 Forms - Formulaires, validation, VeeValidate, Zod, composables
name: Vue3_Forms
applyTo: "**/components/**/*Form*.vue,**/components/**/*Modal*.vue,**/composables/use*Form*.ts"
---

# Vue 3 Formulaires et Validation

Guide complet pour la gestion des formulaires avec Vue 3, VeeValidate et Zod.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** de validation côté client uniquement - valide aussi côté serveur
- **Ne stocke jamais** de données sensibles en clair dans les formulaires
- **N'ignore jamais** les états de loading et d'erreur
- **Ne valide jamais** uniquement au submit - valide au blur/change
- **N'affiche jamais** tous les champs en erreur simultanément
- **N'oublie jamais** l'attribut `autocomplete` approprié

## ✅ À FAIRE

- **Utilise toujours** des schémas de validation (Zod, Yup)
- **Affiche toujours** des messages d'erreur clairs et contextuels
- **Désactive toujours** le submit pendant le chargement
- **Valide toujours** les champs au blur ET au submit
- **Utilise toujours** les attributs `inputmode`, `autocomplete`, `type` corrects
- **Gère toujours** les erreurs serveur et les affiche aux utilisateurs

## 📦 Setup VeeValidate + Zod

### Installation

```bash
npm install vee-validate @vee-validate/zod zod
```

### Configuration Globale

```typescript
/ plugins/veeValidate.ts
import { configure, defineRule } from 'vee-validate';
import { localize, setLocale } from '@vee-validate/i18n';
import fr from '@vee-validate/i18n/dist/locale/fr.json';

/ Configuration globale
configure({
  generateMessage: localize({
    fr,
  }),
  validateOnInput: true,
  validateOnBlur: true,
  validateOnChange: true,
  validateOnModelUpdate: true,
});

setLocale('fr');

/ Règles personnalisées (optionnel)
defineRule('phone', (value: string) => {
  if (!value) return true;
  const phoneRegex = /^(?:(?:\+|00)33|0)\s*[1-9](?:[\s.-]*\d{2}){4}$/;
  return phoneRegex.test(value) || 'Numéro de téléphone invalide';
});

defineRule('password-strength', (value: string) => {
  if (!value) return true;
  const hasUpperCase = /[A-Z]/.test(value);
  const hasLowerCase = /[a-z]/.test(value);
  const hasNumbers = /\d/.test(value);
  const hasSpecialChar = /[!@#$%^&*(),.?":{}|<>]/.test(value);
  const isLongEnough = value.length >= 8;
  
  if (!isLongEnough) return 'Le mot de passe doit contenir au moins 8 caractères';
  if (!hasUpperCase) return 'Le mot de passe doit contenir au moins une majuscule';
  if (!hasLowerCase) return 'Le mot de passe doit contenir au moins une minuscule';
  if (!hasNumbers) return 'Le mot de passe doit contenir au moins un chiffre';
  if (!hasSpecialChar) return 'Le mot de passe doit contenir au moins un caractère spécial';
  
  return true;
});
```

## 📝 Schémas de Validation Zod

### Schémas de Base

```typescript
/ schemas/auth.schema.ts
import { z } from 'zod';

/**
 * Schéma de validation pour le login.
 */
export const loginSchema = z.object({
  email: z
    .string({ required_error: 'L\'email est requis' })
    .min(1, 'L\'email est requis')
    .email('Email invalide'),
  password: z
    .string({ required_error: 'Le mot de passe est requis' })
    .min(1, 'Le mot de passe est requis'),
  rememberMe: z.boolean().optional().default(false),
});

export type LoginFormData = z.infer<typeof loginSchema>;

/**
 * Schéma de validation pour l'inscription.
 */
export const registerSchema = z.object({
  firstName: z
    .string({ required_error: 'Le prénom est requis' })
    .min(2, 'Le prénom doit contenir au moins 2 caractères')
    .max(50, 'Le prénom ne peut pas dépasser 50 caractères'),
  lastName: z
    .string({ required_error: 'Le nom est requis' })
    .min(2, 'Le nom doit contenir au moins 2 caractères')
    .max(50, 'Le nom ne peut pas dépasser 50 caractères'),
  email: z
    .string({ required_error: 'L\'email est requis' })
    .email('Email invalide'),
  password: z
    .string({ required_error: 'Le mot de passe est requis' })
    .min(8, 'Le mot de passe doit contenir au moins 8 caractères')
    .regex(/[A-Z]/, 'Le mot de passe doit contenir au moins une majuscule')
    .regex(/[a-z]/, 'Le mot de passe doit contenir au moins une minuscule')
    .regex(/\d/, 'Le mot de passe doit contenir au moins un chiffre')
    .regex(/[!@#$%^&*(),.?":{}|<>]/, 'Le mot de passe doit contenir au moins un caractère spécial'),
  confirmPassword: z.string({ required_error: 'La confirmation est requise' }),
  acceptTerms: z
    .boolean()
    .refine((val) => val === true, 'Vous devez accepter les conditions'),
}).refine((data) => data.password === data.confirmPassword, {
  message: 'Les mots de passe ne correspondent pas',
  path: ['confirmPassword'],
});

export type RegisterFormData = z.infer<typeof registerSchema>;
```

### Schémas Complexes

```typescript
/ schemas/user.schema.ts
import { z } from 'zod';

/**
 * Schéma pour l'adresse.
 */
export const addressSchema = z.object({
  street: z.string().min(1, 'L\'adresse est requise'),
  city: z.string().min(1, 'La ville est requise'),
  postalCode: z
    .string()
    .min(1, 'Le code postal est requis')
    .regex(/^\d{5}$/, 'Code postal invalide (5 chiffres)'),
  country: z.string().min(1, 'Le pays est requis'),
});

/**
 * Schéma pour le profil utilisateur.
 */
export const userProfileSchema = z.object({
  firstName: z.string().min(2, 'Minimum 2 caractères'),
  lastName: z.string().min(2, 'Minimum 2 caractères'),
  email: z.string().email('Email invalide'),
  phone: z
    .string()
    .optional()
    .refine(
      (val) => !val || /^(?:(?:\+|00)33|0)\s*[1-9](?:[\s.-]*\d{2}){4}$/.test(val),
      'Numéro de téléphone invalide'
    ),
  birthDate: z
    .string()
    .optional()
    .refine(
      (val) => {
        if (!val) return true;
        const date = new Date(val);
        const now = new Date();
        const age = Math.floor((now.getTime() - date.getTime()) / (365.25 * 24 * 60 * 60 * 1000));
        return age >= 18;
      },
      'Vous devez avoir au moins 18 ans'
    ),
  bio: z.string().max(500, 'Maximum 500 caractères').optional(),
  avatar: z
    .instanceof(File)
    .optional()
    .refine(
      (file) => !file || file.size <= 5 * 1024 * 1024,
      'L\'image ne doit pas dépasser 5 Mo'
    )
    .refine(
      (file) => !file || ['image/jpeg', 'image/png', 'image/webp'].includes(file.type),
      'Format accepté : JPG, PNG ou WebP'
    ),
  address: addressSchema.optional(),
  notifications: z.object({
    email: z.boolean().default(true),
    sms: z.boolean().default(false),
    push: z.boolean().default(true),
  }),
});

export type UserProfileFormData = z.infer<typeof userProfileSchema>;

/**
 * Schéma pour les filtres de recherche.
 */
export const searchFiltersSchema = z.object({
  query: z.string().optional(),
  category: z.string().optional(),
  minPrice: z.number().min(0).optional(),
  maxPrice: z.number().min(0).optional(),
  sortBy: z.enum(['name', 'price', 'date', 'popularity']).default('date'),
  sortOrder: z.enum(['asc', 'desc']).default('desc'),
  page: z.number().min(1).default(1),
  limit: z.number().min(1).max(100).default(20),
}).refine(
  (data) => !data.minPrice || !data.maxPrice || data.minPrice <= data.maxPrice,
  { message: 'Le prix minimum doit être inférieur au prix maximum', path: ['minPrice'] }
);

export type SearchFiltersFormData = z.infer<typeof searchFiltersSchema>;
```

## 🎯 Composant de Formulaire

### Formulaire avec VeeValidate

```vue
<!-- views/auth/LoginView.vue -->
<script setup lang="ts">
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import { loginSchema, type LoginFormData } from '@/schemas/auth.schema';
import { useAuthStore } from '@/stores/auth';
import { useAppRouter } from '@/composables/useAppRouter';
import FormField from '@/components/forms/FormField.vue';
import FormInput from '@/components/forms/FormInput.vue';
import FormCheckbox from '@/components/forms/FormCheckbox.vue';

const authStore = useAuthStore();
const { navigateTo, route } = useAppRouter();

/ Configuration du formulaire avec schéma Zod
const { handleSubmit, errors, isSubmitting, resetForm, setErrors } = useForm<LoginFormData>({
  validationSchema: toTypedSchema(loginSchema),
  initialValues: {
    email: '',
    password: '',
    rememberMe: false,
  },
});

/ Soumission du formulaire
const onSubmit = handleSubmit(async (values) => {
  try {
    await authStore.login(values.email, values.password, values.rememberMe);
    
    / Redirection après login
    const redirect = route.query.redirect as string;
    await navigateTo(redirect || 'Home');
  } catch (error) {
    / Gérer les erreurs serveur
    if (error instanceof Error) {
      if (error.message.includes('email')) {
        setErrors({ email: error.message });
      } else if (error.message.includes('password')) {
        setErrors({ password: error.message });
      } else {
        / Erreur générale
        setErrors({ email: 'Identifiants incorrects' });
      }
    }
  }
});
</script>

<template>
  <div class="login-page">
    <h1>Connexion</h1>
    
    <form @submit="onSubmit" novalidate>
      <FormField name="email" label="Email" :error="errors.email">
        <FormInput
          name="email"
          type="email"
          placeholder="votre@email.com"
          autocomplete="email"
          inputmode="email"
        />
      </FormField>

      <FormField name="password" label="Mot de passe" :error="errors.password">
        <FormInput
          name="password"
          type="password"
          placeholder="Votre mot de passe"
          autocomplete="current-password"
        />
      </FormField>

      <FormCheckbox name="rememberMe" label="Se souvenir de moi" />

      <div class="form-actions">
        <button 
          type="submit" 
          :disabled="isSubmitting"
          class="btn btn-primary"
        >
          <span v-if="isSubmitting">Connexion...</span>
          <span v-else>Se connecter</span>
        </button>

        <RouterLink :to="{ name: 'ForgotPassword' }" class="link">
          Mot de passe oublié ?
        </RouterLink>
      </div>
    </form>

    <p class="register-link">
      Pas encore de compte ?
      <RouterLink :to="{ name: 'Register' }">S'inscrire</RouterLink>
    </p>
  </div>
</template>
```

### Composants de Formulaire Réutilisables

```vue
<!-- components/forms/FormField.vue -->
<script setup lang="ts">
import { computed } from 'vue';

interface Props {
  name: string;
  label?: string;
  error?: string;
  hint?: string;
  required?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  required: false,
});

const fieldId = computed(() => `field-${props.name}`);
const errorId = computed(() => `${fieldId.value}-error`);
const hintId = computed(() => `${fieldId.value}-hint`);

const describedBy = computed(() => {
  const ids: string[] = [];
  if (props.error) ids.push(errorId.value);
  if (props.hint) ids.push(hintId.value);
  return ids.length > 0 ? ids.join(' ') : undefined;
});
</script>

<template>
  <div 
    class="form-field" 
    :class="{ 'has-error': error }"
  >
    <label 
      v-if="label" 
      :for="fieldId" 
      class="form-label"
    >
      {{ label }}
      <span v-if="required" class="required-indicator" aria-hidden="true">*</span>
    </label>

    <div class="form-control">
      <slot :id="fieldId" :described-by="describedBy" />
    </div>

    <p 
      v-if="hint && !error" 
      :id="hintId" 
      class="form-hint"
    >
      {{ hint }}
    </p>

    <p 
      v-if="error" 
      :id="errorId" 
      class="form-error" 
      role="alert"
    >
      {{ error }}
    </p>
  </div>
</template>

<style scoped>
.form-field {
  margin-bottom: var(--spacing-4);
}

.form-label {
  display: block;
  margin-bottom: var(--spacing-1);
  font-weight: var(--font-weight-medium);
  color: var(--color-text-primary);
}

.required-indicator {
  color: var(--color-error);
  margin-left: var(--spacing-1);
}

.form-hint {
  margin-top: var(--spacing-1);
  font-size: var(--font-size-sm);
  color: var(--color-text-secondary);
}

.form-error {
  margin-top: var(--spacing-1);
  font-size: var(--font-size-sm);
  color: var(--color-error);
}

.has-error :deep(input),
.has-error :deep(textarea),
.has-error :deep(select) {
  border-color: var(--color-error);
}

.has-error :deep(input:focus),
.has-error :deep(textarea:focus),
.has-error :deep(select:focus) {
  box-shadow: 0 0 0 3px var(--color-error-subtle);
}
</style>
```

```vue
<!-- components/forms/FormInput.vue -->
<script setup lang="ts">
import { useField } from 'vee-validate';
import { computed, ref } from 'vue';

interface Props {
  name: string;
  type?: string;
  placeholder?: string;
  autocomplete?: string;
  inputmode?: string;
  disabled?: boolean;
  readonly?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  type: 'text',
  disabled: false,
  readonly: false,
});

const { value, errorMessage, handleBlur, handleChange } = useField<string>(
  () => props.name
);

/ Gestion du type password avec toggle
const showPassword = ref(false);
const inputType = computed(() => {
  if (props.type === 'password') {
    return showPassword.value ? 'text' : 'password';
  }
  return props.type;
});

function togglePassword(): void {
  showPassword.value = !showPassword.value;
}
</script>

<template>
  <div class="input-wrapper">
    <input
      :id="`field-${name}`"
      v-model="value"
      :type="inputType"
      :name="name"
      :placeholder="placeholder"
      :autocomplete="autocomplete"
      :inputmode="inputmode"
      :disabled="disabled"
      :readonly="readonly"
      :aria-invalid="!!errorMessage"
      class="form-input"
      @blur="handleBlur"
      @input="handleChange"
    />
    
    <button
      v-if="type === 'password'"
      type="button"
      class="password-toggle"
      :aria-label="showPassword ? 'Masquer le mot de passe' : 'Afficher le mot de passe'"
      @click="togglePassword"
    >
      <EyeIcon v-if="!showPassword" />
      <EyeOffIcon v-else />
    </button>
  </div>
</template>

<style scoped>
.input-wrapper {
  position: relative;
}

.form-input {
  width: 100%;
  padding: var(--spacing-2) var(--spacing-3);
  font-size: var(--font-size-base);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background-color: var(--color-surface);
  transition: border-color 0.2s, box-shadow 0.2s;
}

.form-input:focus {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px var(--color-primary-subtle);
}

.form-input:disabled {
  background-color: var(--color-gray-100);
  cursor: not-allowed;
}

.form-input::placeholder {
  color: var(--color-text-disabled);
}

.password-toggle {
  position: absolute;
  right: var(--spacing-3);
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: var(--color-text-secondary);
  cursor: pointer;
  padding: var(--spacing-1);
}

.password-toggle:hover {
  color: var(--color-text-primary);
}
</style>
```

## 🔧 Composables de Formulaire

### useFormState

```typescript
/ composables/useFormState.ts
import { ref, computed, type Ref } from 'vue';

export interface FormState<T> {
  data: Ref<T>;
  isDirty: Ref<boolean>;
  isSubmitting: Ref<boolean>;
  submitCount: Ref<number>;
  errors: Ref<Record<string, string>>;
}

/**
 * Composable pour gérer l'état d'un formulaire.
 */
export function useFormState<T extends Record<string, unknown>>(
  initialValues: T
): FormState<T> & {
  reset: () => void;
  setError: (field: keyof T, message: string) => void;
  clearErrors: () => void;
} {
  const data = ref<T>({ ...initialValues }) as Ref<T>;
  const originalData = ref<T>({ ...initialValues }) as Ref<T>;
  const isSubmitting = ref(false);
  const submitCount = ref(0);
  const errors = ref<Record<string, string>>({});

  const isDirty = computed(() => {
    return JSON.stringify(data.value) !== JSON.stringify(originalData.value);
  });

  function reset(): void {
    data.value = { ...originalData.value };
    errors.value = {};
    submitCount.value = 0;
  }

  function setError(field: keyof T, message: string): void {
    errors.value[field as string] = message;
  }

  function clearErrors(): void {
    errors.value = {};
  }

  return {
    data,
    isDirty,
    isSubmitting,
    submitCount,
    errors,
    reset,
    setError,
    clearErrors,
  };
}
```

### useAsyncValidation

```typescript
/ composables/useAsyncValidation.ts
import { ref, type Ref } from 'vue';
import { debounce } from 'lodash-es';

interface AsyncValidationOptions<T> {
  /** Fonction de validation asynchrone */
  validate: (value: T) => Promise<string | null>;
  /** Délai de debounce en ms */
  debounceMs?: number;
}

/**
 * Composable pour la validation asynchrone (ex: vérifier email unique).
 */
export function useAsyncValidation<T>(options: AsyncValidationOptions<T>): {
  isValidating: Ref<boolean>;
  error: Ref<string | null>;
  validate: (value: T) => Promise<void>;
} {
  const { validate: validateFn, debounceMs = 300 } = options;
  
  const isValidating = ref(false);
  const error = ref<string | null>(null);

  const debouncedValidate = debounce(async (value: T) => {
    isValidating.value = true;
    try {
      error.value = await validateFn(value);
    } catch (e) {
      error.value = 'Erreur de validation';
    } finally {
      isValidating.value = false;
    }
  }, debounceMs);

  async function validate(value: T): Promise<void> {
    await debouncedValidate(value);
  }

  return {
    isValidating,
    error,
    validate,
  };
}

/ Exemple d'utilisation
export function useEmailValidation() {
  return useAsyncValidation({
    validate: async (email: string) => {
      const response = await fetch(`/api/users/check-email?email=${email}`);
      const { exists } = await response.json();
      return exists ? 'Cet email est déjà utilisé' : null;
    },
  });
}
```

### useFormPersistence

```typescript
/ composables/useFormPersistence.ts
import { watch, onMounted, type Ref } from 'vue';

interface FormPersistenceOptions<T> {
  /** Clé de stockage */
  key: string;
  /** Données du formulaire */
  data: Ref<T>;
  /** Exclure certains champs */
  exclude?: (keyof T)[];
  /** Durée de vie en minutes */
  ttlMinutes?: number;
}

/**
 * Composable pour persister les données de formulaire.
 */
export function useFormPersistence<T extends Record<string, unknown>>(
  options: FormPersistenceOptions<T>
): {
  save: () => void;
  restore: () => boolean;
  clear: () => void;
} {
  const { key, data, exclude = [], ttlMinutes = 30 } = options;
  const storageKey = `form_${key}`;

  function save(): void {
    const toSave = { ...data.value };
    
    / Exclure les champs sensibles
    for (const field of exclude) {
      delete toSave[field];
    }

    const entry = {
      data: toSave,
      timestamp: Date.now(),
    };

    localStorage.setItem(storageKey, JSON.stringify(entry));
  }

  function restore(): boolean {
    const stored = localStorage.getItem(storageKey);
    if (!stored) return false;

    try {
      const { data: savedData, timestamp } = JSON.parse(stored);
      
      / Vérifier la validité
      const age = (Date.now() - timestamp) / (1000 * 60);
      if (age > ttlMinutes) {
        clear();
        return false;
      }

      / Restaurer les données
      Object.assign(data.value, savedData);
      return true;
    } catch {
      clear();
      return false;
    }
  }

  function clear(): void {
    localStorage.removeItem(storageKey);
  }

  / Auto-save sur changement
  watch(
    data,
    () => save(),
    { deep: true }
  );

  / Restaurer au montage
  onMounted(() => {
    restore();
  });

  return { save, restore, clear };
}
```

## 📋 Patterns Avancés

### Formulaire Multi-étapes

```vue
<!-- components/forms/MultiStepForm.vue -->
<script setup lang="ts">
import { ref, computed, provide } from 'vue';
import { useForm } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import { z } from 'zod';

/ Schémas par étape
const stepSchemas = {
  1: z.object({
    firstName: z.string().min(2),
    lastName: z.string().min(2),
  }),
  2: z.object({
    email: z.string().email(),
    phone: z.string().optional(),
  }),
  3: z.object({
    password: z.string().min(8),
    confirmPassword: z.string(),
  }).refine(data => data.password === data.confirmPassword, {
    message: 'Les mots de passe ne correspondent pas',
    path: ['confirmPassword'],
  }),
};

const currentStep = ref(1);
const totalSteps = 3;

/ Formulaire avec schéma dynamique
const currentSchema = computed(() => stepSchemas[currentStep.value as keyof typeof stepSchemas]);

const { handleSubmit, errors, values, validate } = useForm({
  validationSchema: computed(() => toTypedSchema(currentSchema.value)),
  keepValuesOnUnmount: true,
});

const canGoBack = computed(() => currentStep.value > 1);
const canGoNext = computed(() => currentStep.value < totalSteps);
const isLastStep = computed(() => currentStep.value === totalSteps);

async function nextStep(): Promise<void> {
  const { valid } = await validate();
  if (valid) {
    currentStep.value++;
  }
}

function prevStep(): void {
  if (canGoBack.value) {
    currentStep.value--;
  }
}

const onSubmit = handleSubmit(async (values) => {
  console.log('Form submitted:', values);
  / Submit final
});

/ Fournir le contexte aux enfants
provide('multiStepForm', {
  currentStep,
  totalSteps,
  values,
  errors,
});
</script>

<template>
  <form @submit="onSubmit">
    <!-- Progress indicator -->
    <div class="step-progress">
      <div 
        v-for="step in totalSteps" 
        :key="step"
        class="step-indicator"
        :class="{
          'is-active': step === currentStep,
          'is-completed': step < currentStep,
        }"
      >
        <span class="step-number">{{ step }}</span>
      </div>
    </div>

    <!-- Step content -->
    <div class="step-content">
      <Transition name="slide-fade" mode="out-in">
        <div :key="currentStep">
          <slot :name="`step-${currentStep}`" />
        </div>
      </Transition>
    </div>

    <!-- Navigation -->
    <div class="step-navigation">
      <button 
        v-if="canGoBack"
        type="button" 
        class="btn btn-secondary"
        @click="prevStep"
      >
        Précédent
      </button>

      <button 
        v-if="canGoNext"
        type="button" 
        class="btn btn-primary"
        @click="nextStep"
      >
        Suivant
      </button>

      <button 
        v-if="isLastStep"
        type="submit" 
        class="btn btn-primary"
      >
        Terminer
      </button>
    </div>
  </form>
</template>
```

### Formulaire Dynamique

```vue
<!-- components/forms/DynamicForm.vue -->
<script setup lang="ts">
import { computed } from 'vue';
import { useForm, useFieldArray } from 'vee-validate';
import { toTypedSchema } from '@vee-validate/zod';
import { z } from 'zod';

/ Schéma avec tableau dynamique
const schema = z.object({
  title: z.string().min(1, 'Titre requis'),
  items: z.array(z.object({
    name: z.string().min(1, 'Nom requis'),
    quantity: z.number().min(1, 'Quantité minimum: 1'),
    price: z.number().min(0, 'Prix minimum: 0'),
  })).min(1, 'Au moins un élément requis'),
});

type FormData = z.infer<typeof schema>;

const { handleSubmit, errors } = useForm<FormData>({
  validationSchema: toTypedSchema(schema),
  initialValues: {
    title: '',
    items: [{ name: '', quantity: 1, price: 0 }],
  },
});

/ Gestion du tableau dynamique
const { fields, push, remove, move } = useFieldArray<FormData['items'][0]>('items');

function addItem(): void {
  push({ name: '', quantity: 1, price: 0 });
}

function removeItem(index: number): void {
  if (fields.value.length > 1) {
    remove(index);
  }
}

const total = computed(() => {
  return fields.value.reduce((sum, field) => {
    return sum + (field.value.quantity * field.value.price);
  }, 0);
});

const onSubmit = handleSubmit((values) => {
  console.log('Submitted:', values);
});
</script>

<template>
  <form @submit="onSubmit">
    <FormField name="title" label="Titre de la commande" :error="errors.title">
      <FormInput name="title" />
    </FormField>

    <div class="items-list">
      <h3>Articles</h3>
      
      <TransitionGroup name="list" tag="div">
        <div 
          v-for="(field, index) in fields" 
          :key="field.key"
          class="item-row"
        >
          <FormInput 
            :name="`items[${index}].name`" 
            placeholder="Nom de l'article"
          />
          
          <FormInput 
            :name="`items[${index}].quantity`" 
            type="number"
            placeholder="Qté"
          />
          
          <FormInput 
            :name="`items[${index}].price`" 
            type="number"
            step="0.01"
            placeholder="Prix"
          />
          
          <button 
            type="button"
            class="btn-icon"
            :disabled="fields.length <= 1"
            @click="removeItem(index)"
          >
            <TrashIcon />
          </button>
        </div>
      </TransitionGroup>

      <button 
        type="button" 
        class="btn btn-secondary"
        @click="addItem"
      >
        Ajouter un article
      </button>
    </div>

    <div class="total">
      <strong>Total: {{ total.toFixed(2) }} €</strong>
    </div>

    <button type="submit" class="btn btn-primary">
      Valider la commande
    </button>
  </form>
</template>
```

## ⚠️ Bonnes Pratiques

### Accessibilité

```vue
<template>
  <!-- Labels liés aux inputs -->
  <label for="email">Email</label>
  <input id="email" type="email" aria-describedby="email-error email-hint" />
  <span id="email-hint">Votre adresse email professionnelle</span>
  <span id="email-error" role="alert">Email invalide</span>

  <!-- Champs requis -->
  <label for="name">
    Nom <span aria-hidden="true">*</span>
    <span class="sr-only">(requis)</span>
  </label>

  <!-- Messages d'erreur accessibles -->
  <div role="alert" aria-live="polite">
    <p v-if="formError">{{ formError }}</p>
  </div>

  <!-- Groupes de champs -->
  <fieldset>
    <legend>Informations personnelles</legend>
    <!-- champs -->
  </fieldset>
</template>
```

### Sécurité

```typescript
/ Ne jamais stocker de mots de passe dans le state
const formData = ref({
  email: '',
  / password: '', / ❌ Éviter de garder en state
});

/ Utiliser autocomplete approprié
/ autocomplete="new-password" pour création
/ autocomplete="current-password" pour connexion

/ Nettoyer les données avant soumission
function sanitizeInput(input: string): string {
  return input.trim().replace(/[<>]/g, '');
}
```
