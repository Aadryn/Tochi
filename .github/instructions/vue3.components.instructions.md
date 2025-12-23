---
description: Composants Vue 3 - Structure SFC, Props, Emits, Slots, PrimeVue integration
name: Vue3_Components
applyTo: "**/components/**/*.vue,**/views/**/*.vue,**/App.vue"
---

# Vue 3 - Guide des Composants

Guide complet pour créer des composants Vue 3 de qualité avec TypeScript.

## ⛔ À NE PAS FAIRE

- **N'écris jamais** de composant sans `<script setup lang="ts">`
- **Ne définis jamais** les props sans `defineProps<T>()` typé
- **N'omets jamais** `defineEmits<T>()` pour les événements
- **Ne crée jamais** de composant >200 lignes sans découper
- **N'utilise jamais** `$refs` directement (utilise `ref<T>()` typé)
- **Ne passe jamais** >5 props à un composant (grouper en objet)
- **N'oublie jamais** les slots nommés pour la flexibilité

## ✅ À FAIRE

- **Structure toujours** les SFC dans l'ordre : script → template → style
- **Type toujours** les props avec `defineProps<{ prop: Type }>()`
- **Type toujours** les emits avec `defineEmits<{ event: [payload: Type] }>()`
- **Utilise toujours** `withDefaults()` pour les valeurs par défaut
- **Découpe toujours** les gros composants en sous-composants
- **Utilise toujours** les composants PrimeVue pour l'UI
- **Expose toujours** explicitement avec `defineExpose()` si nécessaire

## 🎯 Actions Obligatoires (Mandatory)

### Structure SFC (Single File Component)

**TOUJOURS structurer les fichiers `.vue` dans cet ordre :**

```vue
<script setup lang="ts">
/ 1. Imports
/ 2. Props et Emits
/ 3. État réactif
/ 4. Computed
/ 5. Watchers
/ 6. Lifecycle hooks
/ 7. Méthodes
</script>

<template>
  <!-- Template HTML avec PrimeVue -->
</template>

<style scoped>
/* Styles scopés au composant */
</style>
```

### Props Typées OBLIGATOIRES

```vue
<script setup lang="ts">
/ ✅ BON : Props avec interface TypeScript
interface Props {
  /** Identifiant unique de l'utilisateur */
  userId: string
  /** Titre affiché dans l'en-tête */
  title?: string
  /** Mode lecture seule */
  readonly?: boolean
  /** Liste des éléments à afficher */
  items: Item[]
}

const props = withDefaults(defineProps<Props>(), {
  title: 'Default Title',
  readonly: false,
})

/ ❌ MAUVAIS : Props sans typage
/ const props = defineProps(['userId', 'title'])
</script>
```

### Emits Typés OBLIGATOIRES

```vue
<script setup lang="ts">
/ ✅ BON : Emits avec types
interface Emits {
  /** Émis lors de la soumission du formulaire */
  (e: 'submit', data: FormData): void
  /** Émis lors de l'annulation */
  (e: 'cancel'): void
  /** Émis lors de la mise à jour */
  (e: 'update:modelValue', value: string): void
}

const emit = defineEmits<Emits>()

/ Utilisation
function handleSubmit(data: FormData) {
  emit('submit', data)
}

/ ❌ MAUVAIS : Emits non typés
/ const emit = defineEmits(['submit', 'cancel'])
</script>
```

## 📐 Patterns de Composants

### Composant de Base (Base Component)

```vue
<!-- components/shared/BaseButton.vue -->
<script setup lang="ts">
import Button from 'primevue/button'

interface Props {
  /** Libellé du bouton */
  label: string
  /** Variante visuelle */
  severity?: 'primary' | 'secondary' | 'success' | 'info' | 'warning' | 'danger'
  /** Icône PrimeIcons */
  icon?: string
  /** État de chargement */
  loading?: boolean
  /** Désactivé */
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  severity: 'primary',
  loading: false,
  disabled: false,
})

interface Emits {
  (e: 'click', event: MouseEvent): void
}

const emit = defineEmits<Emits>()
</script>

<template>
  <Button
    :label="props.label"
    :severity="props.severity"
    :icon="props.icon"
    :loading="props.loading"
    :disabled="props.disabled"
    @click="emit('click', $event)"
  />
</template>
```

### Composant avec v-model

```vue
<!-- components/shared/BaseInput.vue -->
<script setup lang="ts">
import InputText from 'primevue/inputtext'
import { computed } from 'vue'

interface Props {
  /** Valeur du champ (v-model) */
  modelValue: string
  /** Label du champ */
  label?: string
  /** Placeholder */
  placeholder?: string
  /** Message d'erreur */
  error?: string
  /** Requis */
  required?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: '',
  required: false,
})

interface Emits {
  (e: 'update:modelValue', value: string): void
  (e: 'blur'): void
}

const emit = defineEmits<Emits>()

/ ✅ BON : Computed pour v-model bidirectionnel
const inputValue = computed({
  get: () => props.modelValue,
  set: (value: string) => emit('update:modelValue', value),
})

const hasError = computed(() => Boolean(props.error))
</script>

<template>
  <div class="field">
    <label v-if="props.label" class="block mb-2">
      {{ props.label }}
      <span v-if="props.required" class="text-red-500">*</span>
    </label>
    <InputText
      v-model="inputValue"
      :placeholder="props.placeholder"
      :class="{ 'p-invalid': hasError }"
      @blur="emit('blur')"
    />
    <small v-if="hasError" class="p-error block mt-1">
      {{ props.error }}
    </small>
  </div>
</template>
```

### Composant avec Slots

```vue
<!-- components/shared/BaseCard.vue -->
<script setup lang="ts">
import Card from 'primevue/card'

interface Props {
  /** Titre de la carte */
  title?: string
  /** Sous-titre */
  subtitle?: string
}

const props = defineProps<Props>()

/ ✅ BON : Slots typés
defineSlots<{
  /** Contenu principal */
  default(): any
  /** En-tête personnalisé */
  header?(): any
  /** Pied de page */
  footer?(): any
}>()
</script>

<template>
  <Card>
    <template #header>
      <slot name="header">
        <div v-if="props.title" class="p-card-title">{{ props.title }}</div>
        <div v-if="props.subtitle" class="p-card-subtitle">{{ props.subtitle }}</div>
      </slot>
    </template>

    <template #content>
      <slot />
    </template>

    <template v-if="$slots.footer" #footer>
      <slot name="footer" />
    </template>
  </Card>
</template>
```

### Composant avec Expose

```vue
<!-- components/shared/BaseModal.vue -->
<script setup lang="ts">
import Dialog from 'primevue/dialog'
import { ref } from 'vue'

interface Props {
  /** Titre de la modal */
  title: string
  /** Largeur */
  width?: string
}

const props = withDefaults(defineProps<Props>(), {
  width: '450px',
})

const visible = ref(false)

/ ✅ BON : Expose pour API publique du composant
function open() {
  visible.value = true
}

function close() {
  visible.value = false
}

defineExpose({
  open,
  close,
  visible,
})
</script>

<template>
  <Dialog
    v-model:visible="visible"
    :header="props.title"
    :style="{ width: props.width }"
    modal
  >
    <slot />
    
    <template #footer>
      <slot name="footer">
        <Button label="Fermer" @click="close" />
      </slot>
    </template>
  </Dialog>
</template>
```

## 🎨 Intégration PrimeVue

### Import des Composants

```typescript
/ main.ts
import { createApp } from 'vue'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import ConfirmationService from 'primevue/confirmationservice'

/ Styles PrimeVue
import 'primevue/resources/themes/lara-light-blue/theme.css'
import 'primeicons/primeicons.css'
import 'primeflex/primeflex.css'

const app = createApp(App)

app.use(PrimeVue, { ripple: true })
app.use(ToastService)
app.use(ConfirmationService)
```

### Utilisation Correcte des Composants PrimeVue

```vue
<script setup lang="ts">
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import { useToast } from 'primevue/usetoast'
import { useConfirm } from 'primevue/useconfirm'

const toast = useToast()
const confirm = useConfirm()

interface User {
  id: string
  name: string
  email: string
  status: 'active' | 'inactive'
}

const users = ref<User[]>([])

function showSuccess(message: string) {
  toast.add({
    severity: 'success',
    summary: 'Succès',
    detail: message,
    life: 3000,
  })
}

function confirmDelete(user: User) {
  confirm.require({
    message: `Voulez-vous supprimer ${user.name} ?`,
    header: 'Confirmation',
    icon: 'pi pi-exclamation-triangle',
    acceptClass: 'p-button-danger',
    accept: () => deleteUser(user.id),
  })
}
</script>

<template>
  <DataTable
    :value="users"
    paginator
    :rows="10"
    :rowsPerPageOptions="[5, 10, 25, 50]"
    tableStyle="min-width: 50rem"
  >
    <Column field="name" header="Nom" sortable />
    <Column field="email" header="Email" sortable />
    <Column field="status" header="Statut">
      <template #body="{ data }">
        <Tag :severity="data.status === 'active' ? 'success' : 'danger'">
          {{ data.status }}
        </Tag>
      </template>
    </Column>
    <Column header="Actions">
      <template #body="{ data }">
        <Button
          icon="pi pi-trash"
          severity="danger"
          text
          @click="confirmDelete(data)"
        />
      </template>
    </Column>
  </DataTable>
</template>
```

## 🚫 Anti-Patterns à Éviter

### ❌ Props Mutées Directement

```vue
<script setup lang="ts">
/ ❌ MAUVAIS : Mutation directe de prop
const props = defineProps<{ count: number }>()
props.count++ / Erreur !

/ ✅ BON : Émettre un événement
const emit = defineEmits<{
  (e: 'update:count', value: number): void
}>()
function increment() {
  emit('update:count', props.count + 1)
}
</script>
```

### ❌ Logique Métier dans le Template

```vue
<!-- ❌ MAUVAIS : Calcul complexe dans le template -->
<template>
  <div>{{ items.filter(i => i.active).map(i => i.name).join(', ') }}</div>
</template>

<!-- ✅ BON : Utiliser un computed -->
<script setup lang="ts">
const activeItemNames = computed(() =>
  items.value.filter(i => i.active).map(i => i.name).join(', ')
)
</script>

<template>
  <div>{{ activeItemNames }}</div>
</template>
```

### ❌ Composants Trop Gros

```vue
<!-- ❌ MAUVAIS : > 300 lignes dans un seul fichier -->
<!-- ✅ BON : Extraire en sous-composants et composables -->
```

## ✅ Checklist Composant

- [ ] Props typées avec interface TypeScript
- [ ] Emits typés avec interface TypeScript
- [ ] Slots documentés avec `defineSlots`
- [ ] Pas de mutation directe des props
- [ ] Computed pour les calculs dérivés
- [ ] Styles scopés avec `<style scoped>`
- [ ] Nommage PascalCase du fichier
- [ ] Documentation JSDoc pour les props/emits
