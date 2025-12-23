---
description: Vue 3 PrimeVue - Composants UI, theming, DataTable, formulaires, dialogues
name: Vue3_PrimeVue
applyTo: "**/frontend/components/**/*.vue,**/frontend/views/**/*.vue"
---

# Vue 3 PrimeVue

Guide complet pour l'intégration et l'utilisation de PrimeVue 3 avec Vue 3.

## ⛔ À NE PAS FAIRE

- **N'importe jamais** tous les composants globalement (tree-shaking impossible)
- **Ne modifie jamais** les styles PrimeVue directement (utiliser les variables CSS)
- **N'utilise jamais** de styles inline pour les composants PrimeVue
- **Ne mélange jamais** plusieurs thèmes dans la même application
- **N'ignore jamais** l'accessibilité des composants PrimeVue

## ✅ À FAIRE

- **Utilise toujours** l'import à la demande (auto-import plugin)
- **Utilise toujours** les CSS variables pour le theming
- **Utilise toujours** les slots pour personnaliser les composants
- **Utilise toujours** les PassThrough pour les styles avancés
- **Documente toujours** les props et events des composants wrapper

## 🔧 Configuration

### Installation et Setup

```typescript
// main.ts
import { createApp } from 'vue';
import PrimeVue from 'primevue/config';
import App from './App.vue';

// Thème et styles
import 'primevue/resources/themes/lara-light-blue/theme.css';
import 'primeicons/primeicons.css';

// Services
import ToastService from 'primevue/toastservice';
import ConfirmationService from 'primevue/confirmationservice';
import DialogService from 'primevue/dialogservice';

const app = createApp(App);

app.use(PrimeVue, {
  ripple: true,
  inputStyle: 'outlined', // 'filled' | 'outlined'
  locale: {
    // Traductions françaises
    startsWith: 'Commence par',
    contains: 'Contient',
    notContains: 'Ne contient pas',
    endsWith: 'Se termine par',
    equals: 'Égal à',
    notEquals: 'Différent de',
    noFilter: 'Aucun filtre',
    lt: 'Inférieur à',
    lte: 'Inférieur ou égal à',
    gt: 'Supérieur à',
    gte: 'Supérieur ou égal à',
    dateIs: 'La date est',
    dateIsNot: 'La date n\'est pas',
    dateBefore: 'Avant le',
    dateAfter: 'Après le',
    clear: 'Effacer',
    apply: 'Appliquer',
    matchAll: 'Correspond à tous',
    matchAny: 'Correspond à un',
    addRule: 'Ajouter une règle',
    removeRule: 'Supprimer la règle',
    accept: 'Oui',
    reject: 'Non',
    choose: 'Choisir',
    upload: 'Télécharger',
    cancel: 'Annuler',
    pending: 'En attente',
    fileSizeTypes: ['O', 'Ko', 'Mo', 'Go', 'To', 'Po', 'Eo', 'Zo', 'Yo'],
    dayNames: ['Dimanche', 'Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi', 'Samedi'],
    dayNamesShort: ['Dim', 'Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam'],
    dayNamesMin: ['Di', 'Lu', 'Ma', 'Me', 'Je', 'Ve', 'Sa'],
    monthNames: ['Janvier', 'Février', 'Mars', 'Avril', 'Mai', 'Juin', 'Juillet', 'Août', 'Septembre', 'Octobre', 'Novembre', 'Décembre'],
    monthNamesShort: ['Jan', 'Fév', 'Mar', 'Avr', 'Mai', 'Jun', 'Jul', 'Aoû', 'Sep', 'Oct', 'Nov', 'Déc'],
    today: 'Aujourd\'hui',
    weekHeader: 'Sm',
    firstDayOfWeek: 1, // Lundi
    dateFormat: 'dd/mm/yy',
    weak: 'Faible',
    medium: 'Moyen',
    strong: 'Fort',
    passwordPrompt: 'Entrez un mot de passe',
    emptyFilterMessage: 'Aucun résultat trouvé',
    emptyMessage: 'Aucune option disponible',
    aria: {
      trueLabel: 'Vrai',
      falseLabel: 'Faux',
      nullLabel: 'Non sélectionné',
      pageLabel: 'Page {page}',
      firstPageLabel: 'Première page',
      lastPageLabel: 'Dernière page',
      nextPageLabel: 'Page suivante',
      previousPageLabel: 'Page précédente',
    },
  },
});

app.use(ToastService);
app.use(ConfirmationService);
app.use(DialogService);

app.mount('#app');
```

### Auto-Import avec unplugin

```typescript
// vite.config.ts
import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import Components from 'unplugin-vue-components/vite';
import { PrimeVueResolver } from 'unplugin-vue-components/resolvers';

export default defineConfig({
  plugins: [
    vue(),
    Components({
      resolvers: [
        PrimeVueResolver(),
      ],
    }),
  ],
});
```

## 🎨 Theming avec CSS Variables

### Variables de Thème Custom

```css
/* assets/styles/primevue-theme.css */
:root {
  /* Couleurs primaires */
  --primary-50: #f0f9ff;
  --primary-100: #e0f2fe;
  --primary-200: #bae6fd;
  --primary-300: #7dd3fc;
  --primary-400: #38bdf8;
  --primary-500: #0ea5e9;
  --primary-600: #0284c7;
  --primary-700: #0369a1;
  --primary-800: #075985;
  --primary-900: #0c4a6e;

  /* Surface colors */
  --surface-0: #ffffff;
  --surface-50: #fafafa;
  --surface-100: #f5f5f5;
  --surface-200: #eeeeee;
  --surface-300: #e0e0e0;
  --surface-400: #bdbdbd;
  --surface-500: #9e9e9e;
  --surface-600: #757575;
  --surface-700: #616161;
  --surface-800: #424242;
  --surface-900: #212121;

  /* Text colors */
  --text-color: var(--surface-900);
  --text-color-secondary: var(--surface-600);

  /* Border */
  --border-radius: 6px;
  --border-color: var(--surface-300);

  /* Focus */
  --focus-ring: 0 0 0 2px var(--surface-0), 0 0 0 4px var(--primary-500);
}

/* Dark mode */
[data-theme="dark"] {
  --surface-0: #121212;
  --surface-50: #1e1e1e;
  --surface-100: #2c2c2c;
  --surface-200: #3c3c3c;
  --surface-300: #4c4c4c;
  --surface-400: #6c6c6c;
  --surface-500: #8c8c8c;
  --surface-600: #a8a8a8;
  --surface-700: #c4c4c4;
  --surface-800: #e0e0e0;
  --surface-900: #f5f5f5;

  --text-color: var(--surface-900);
  --text-color-secondary: var(--surface-600);
  --border-color: var(--surface-400);
}
```

### PassThrough API

```vue
<script setup lang="ts">
import { ref } from 'vue';

const value = ref('');

// Configuration PassThrough pour personnalisation avancée
const inputTextPT = {
  root: {
    class: 'custom-input',
    'data-testid': 'custom-input',
  },
};

const buttonPT = {
  root: ({ props, context }) => ({
    class: [
      'custom-button',
      {
        'custom-button--loading': props.loading,
        'custom-button--disabled': context.disabled,
      },
    ],
  }),
  label: {
    class: 'custom-button__label',
  },
  icon: {
    class: 'custom-button__icon',
  },
};
</script>

<template>
  <InputText 
    v-model="value" 
    :pt="inputTextPT"
    placeholder="Entrez du texte"
  />
  
  <Button 
    label="Soumettre"
    :pt="buttonPT"
    icon="pi pi-check"
  />
</template>
```

### Global PassThrough

```typescript
// primevue.config.ts
import type { PrimeVuePTOptions } from 'primevue/config';

export const globalPT: PrimeVuePTOptions = {
  button: {
    root: {
      class: 'app-button',
    },
  },
  inputtext: {
    root: {
      class: 'app-input',
    },
  },
  datatable: {
    root: {
      class: 'app-datatable',
    },
    header: {
      class: 'app-datatable__header',
    },
    tbody: {
      class: 'app-datatable__body',
    },
  },
};

// main.ts
import { globalPT } from './primevue.config';

app.use(PrimeVue, {
  pt: globalPT,
});
```

## 📋 DataTable Avancé

### Configuration Complète

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { FilterMatchMode, FilterOperator } from 'primevue/api';
import type { DataTableFilterMeta, DataTableSortMeta } from 'primevue/datatable';

interface Product {
  id: string;
  code: string;
  name: string;
  category: string;
  quantity: number;
  price: number;
  status: 'INSTOCK' | 'LOWSTOCK' | 'OUTOFSTOCK';
  rating: number;
}

const products = ref<Product[]>([]);
const selectedProducts = ref<Product[]>([]);
const loading = ref(false);

// Pagination
const first = ref(0);
const rows = ref(10);
const totalRecords = ref(0);

// Tri
const multiSortMeta = ref<DataTableSortMeta[]>([]);

// Filtres
const filters = ref<DataTableFilterMeta>({
  global: { value: null, matchMode: FilterMatchMode.CONTAINS },
  name: { 
    operator: FilterOperator.AND, 
    constraints: [{ value: null, matchMode: FilterMatchMode.STARTS_WITH }] 
  },
  category: { value: null, matchMode: FilterMatchMode.IN },
  status: { value: null, matchMode: FilterMatchMode.EQUALS },
  price: { 
    operator: FilterOperator.AND, 
    constraints: [{ value: null, matchMode: FilterMatchMode.GREATER_THAN_OR_EQUAL_TO }] 
  },
});

const categories = ref(['Accessories', 'Clothing', 'Electronics', 'Fitness']);
const statuses = ref(['INSTOCK', 'LOWSTOCK', 'OUTOFSTOCK']);

async function loadProducts(event?: { first: number; rows: number; sortField?: string; sortOrder?: number }): Promise<void> {
  loading.value = true;
  
  try {
    const params = new URLSearchParams({
      page: String(Math.floor((event?.first ?? 0) / (event?.rows ?? 10))),
      size: String(event?.rows ?? 10),
      ...(event?.sortField && { sortField: event.sortField }),
      ...(event?.sortOrder && { sortOrder: String(event.sortOrder) }),
    });

    const response = await fetch(`/api/products?${params}`);
    const data = await response.json();
    
    products.value = data.items;
    totalRecords.value = data.total;
  } finally {
    loading.value = false;
  }
}

function onPage(event: { first: number; rows: number }): void {
  first.value = event.first;
  rows.value = event.rows;
  loadProducts(event);
}

function onSort(event: { sortField: string; sortOrder: number }): void {
  loadProducts({ first: first.value, rows: rows.value, ...event });
}

function exportCSV(): void {
  // Export handled by DataTable ref
}

function getStatusSeverity(status: string): string {
  switch (status) {
    case 'INSTOCK': return 'success';
    case 'LOWSTOCK': return 'warning';
    case 'OUTOFSTOCK': return 'danger';
    default: return 'info';
  }
}

function formatCurrency(value: number): string {
  return new Intl.NumberFormat('fr-FR', {
    style: 'currency',
    currency: 'EUR',
  }).format(value);
}

onMounted(() => {
  loadProducts();
});
</script>

<template>
  <div class="datatable-container">
    <!-- Toolbar -->
    <Toolbar class="mb-4">
      <template #start>
        <Button 
          label="Nouveau" 
          icon="pi pi-plus" 
          severity="success" 
          class="mr-2" 
        />
        <Button 
          label="Supprimer" 
          icon="pi pi-trash" 
          severity="danger" 
          :disabled="!selectedProducts.length"
        />
      </template>
      <template #end>
        <Button 
          label="Exporter" 
          icon="pi pi-upload" 
          severity="help" 
          @click="exportCSV"
        />
      </template>
    </Toolbar>

    <!-- DataTable -->
    <DataTable
      v-model:selection="selectedProducts"
      v-model:filters="filters"
      :value="products"
      :loading="loading"
      :paginator="true"
      :rows="rows"
      :first="first"
      :totalRecords="totalRecords"
      :rowsPerPageOptions="[5, 10, 25, 50]"
      :lazy="true"
      :sortMode="'multiple'"
      :multiSortMeta="multiSortMeta"
      :globalFilterFields="['name', 'category', 'status']"
      filterDisplay="menu"
      dataKey="id"
      stripedRows
      showGridlines
      responsiveLayout="scroll"
      paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink CurrentPageReport RowsPerPageDropdown"
      currentPageReportTemplate="{first} à {last} sur {totalRecords} produits"
      @page="onPage"
      @sort="onSort"
    >
      <!-- Header -->
      <template #header>
        <div class="flex justify-content-between align-items-center">
          <h4 class="m-0">Gestion des Produits</h4>
          <span class="p-input-icon-left">
            <i class="pi pi-search" />
            <InputText 
              v-model="filters.global.value" 
              placeholder="Rechercher..."
            />
          </span>
        </div>
      </template>

      <!-- Selection Column -->
      <Column 
        selectionMode="multiple" 
        style="width: 3rem"
        :exportable="false"
      />

      <!-- Code Column -->
      <Column 
        field="code" 
        header="Code" 
        sortable 
        style="min-width: 8rem"
      />

      <!-- Name Column with Filter -->
      <Column 
        field="name" 
        header="Nom" 
        sortable 
        :showFilterMatchModes="false"
        style="min-width: 14rem"
      >
        <template #body="{ data }">
          {{ data.name }}
        </template>
        <template #filter="{ filterModel, filterCallback }">
          <InputText
            v-model="filterModel.value"
            type="text"
            class="p-column-filter"
            placeholder="Rechercher par nom"
            @input="filterCallback()"
          />
        </template>
      </Column>

      <!-- Category Column with MultiSelect Filter -->
      <Column 
        field="category" 
        header="Catégorie" 
        sortable
        :filterMenuStyle="{ width: '14rem' }"
        style="min-width: 12rem"
      >
        <template #body="{ data }">
          {{ data.category }}
        </template>
        <template #filter="{ filterModel, filterCallback }">
          <MultiSelect
            v-model="filterModel.value"
            :options="categories"
            placeholder="Toutes"
            class="p-column-filter"
            @change="filterCallback()"
          />
        </template>
      </Column>

      <!-- Price Column with Range Filter -->
      <Column 
        field="price" 
        header="Prix" 
        sortable 
        dataType="numeric"
        style="min-width: 10rem"
      >
        <template #body="{ data }">
          {{ formatCurrency(data.price) }}
        </template>
        <template #filter="{ filterModel, filterCallback }">
          <InputNumber
            v-model="filterModel.value"
            mode="currency"
            currency="EUR"
            locale="fr-FR"
            @input="filterCallback()"
          />
        </template>
      </Column>

      <!-- Rating Column -->
      <Column 
        field="rating" 
        header="Avis" 
        sortable 
        style="min-width: 10rem"
      >
        <template #body="{ data }">
          <Rating 
            :modelValue="data.rating" 
            readonly 
            :cancel="false"
          />
        </template>
      </Column>

      <!-- Status Column with Tag -->
      <Column 
        field="status" 
        header="Statut" 
        sortable 
        :filterMenuStyle="{ width: '14rem' }"
        style="min-width: 10rem"
      >
        <template #body="{ data }">
          <Tag 
            :value="data.status" 
            :severity="getStatusSeverity(data.status)"
          />
        </template>
        <template #filter="{ filterModel, filterCallback }">
          <Dropdown
            v-model="filterModel.value"
            :options="statuses"
            placeholder="Sélectionner"
            class="p-column-filter"
            showClear
            @change="filterCallback()"
          >
            <template #option="{ option }">
              <Tag :value="option" :severity="getStatusSeverity(option)" />
            </template>
          </Dropdown>
        </template>
      </Column>

      <!-- Actions Column -->
      <Column 
        :exportable="false" 
        style="min-width: 8rem"
      >
        <template #body="{ data }">
          <Button 
            icon="pi pi-pencil" 
            rounded 
            outlined 
            class="mr-2"
            @click="editProduct(data)"
          />
          <Button 
            icon="pi pi-trash" 
            rounded 
            outlined 
            severity="danger"
            @click="confirmDelete(data)"
          />
        </template>
      </Column>

      <!-- Empty State -->
      <template #empty>
        <div class="text-center py-4">
          <i class="pi pi-inbox text-4xl text-400 mb-3" />
          <p class="text-600">Aucun produit trouvé.</p>
        </div>
      </template>

      <!-- Loading State -->
      <template #loading>
        <div class="text-center py-4">
          <ProgressSpinner style="width: 50px; height: 50px" />
          <p class="text-600 mt-2">Chargement des produits...</p>
        </div>
      </template>
    </DataTable>
  </div>
</template>
```

### DataTable avec Row Expansion

```vue
<script setup lang="ts">
import { ref } from 'vue';

interface Order {
  id: string;
  customer: string;
  date: Date;
  status: string;
  items: OrderItem[];
}

interface OrderItem {
  id: string;
  product: string;
  quantity: number;
  price: number;
}

const orders = ref<Order[]>([]);
const expandedRows = ref<Order[]>([]);

function expandAll(): void {
  expandedRows.value = orders.value;
}

function collapseAll(): void {
  expandedRows.value = [];
}
</script>

<template>
  <DataTable
    v-model:expandedRows="expandedRows"
    :value="orders"
    dataKey="id"
    responsiveLayout="scroll"
  >
    <template #header>
      <div class="flex flex-wrap justify-content-end gap-2">
        <Button 
          icon="pi pi-plus" 
          label="Tout déplier" 
          text
          @click="expandAll"
        />
        <Button 
          icon="pi pi-minus" 
          label="Tout replier" 
          text
          @click="collapseAll"
        />
      </div>
    </template>

    <Column expander style="width: 3rem" />
    <Column field="id" header="ID" sortable />
    <Column field="customer" header="Client" sortable />
    <Column field="date" header="Date" sortable>
      <template #body="{ data }">
        {{ new Date(data.date).toLocaleDateString('fr-FR') }}
      </template>
    </Column>
    <Column field="status" header="Statut" sortable />

    <!-- Expansion Template -->
    <template #expansion="{ data }">
      <div class="p-3">
        <h5>Détails de la commande {{ data.id }}</h5>
        <DataTable :value="data.items" responsiveLayout="scroll">
          <Column field="product" header="Produit" />
          <Column field="quantity" header="Quantité" />
          <Column field="price" header="Prix unitaire">
            <template #body="{ data: item }">
              {{ formatCurrency(item.price) }}
            </template>
          </Column>
          <Column header="Total">
            <template #body="{ data: item }">
              {{ formatCurrency(item.quantity * item.price) }}
            </template>
          </Column>
        </DataTable>
      </div>
    </template>
  </DataTable>
</template>
```

## 📝 Formulaires PrimeVue

### Formulaire Complet avec Validation

```vue
<script setup lang="ts">
import { ref, reactive } from 'vue';
import { useVuelidate } from '@vuelidate/core';
import { required, email, minLength, helpers } from '@vuelidate/validators';

interface FormData {
  name: string;
  email: string;
  password: string;
  country: { name: string; code: string } | null;
  birthdate: Date | null;
  gender: string;
  interests: string[];
  newsletter: boolean;
  bio: string;
}

const formData = reactive<FormData>({
  name: '',
  email: '',
  password: '',
  country: null,
  birthdate: null,
  gender: '',
  interests: [],
  newsletter: false,
  bio: '',
});

const countries = ref([
  { name: 'France', code: 'FR' },
  { name: 'Belgique', code: 'BE' },
  { name: 'Suisse', code: 'CH' },
  { name: 'Canada', code: 'CA' },
]);

const genders = ref(['Homme', 'Femme', 'Autre', 'Ne pas préciser']);

const interestsOptions = ref([
  'Technologie',
  'Sport',
  'Musique',
  'Voyage',
  'Cuisine',
  'Lecture',
]);

const rules = {
  name: { 
    required: helpers.withMessage('Le nom est requis', required),
    minLength: helpers.withMessage('Le nom doit contenir au moins 2 caractères', minLength(2)),
  },
  email: { 
    required: helpers.withMessage('L\'email est requis', required),
    email: helpers.withMessage('L\'email n\'est pas valide', email),
  },
  password: { 
    required: helpers.withMessage('Le mot de passe est requis', required),
    minLength: helpers.withMessage('Le mot de passe doit contenir au moins 8 caractères', minLength(8)),
  },
  country: { 
    required: helpers.withMessage('Le pays est requis', required),
  },
};

const v$ = useVuelidate(rules, formData);

const submitted = ref(false);
const loading = ref(false);

async function handleSubmit(): Promise<void> {
  submitted.value = true;
  
  const isValid = await v$.value.$validate();
  if (!isValid) return;
  
  loading.value = true;
  
  try {
    await new Promise(resolve => setTimeout(resolve, 1000));
    console.log('Form submitted:', formData);
    // Reset form
    resetForm();
  } finally {
    loading.value = false;
  }
}

function resetForm(): void {
  formData.name = '';
  formData.email = '';
  formData.password = '';
  formData.country = null;
  formData.birthdate = null;
  formData.gender = '';
  formData.interests = [];
  formData.newsletter = false;
  formData.bio = '';
  
  submitted.value = false;
  v$.value.$reset();
}

function getErrorMessage(field: keyof typeof rules): string | undefined {
  return v$.value[field].$errors[0]?.$message as string | undefined;
}
</script>

<template>
  <form @submit.prevent="handleSubmit" class="p-fluid">
    <div class="formgrid grid">
      <!-- Name -->
      <div class="field col-12 md:col-6">
        <label for="name">Nom *</label>
        <InputText
          id="name"
          v-model="formData.name"
          :class="{ 'p-invalid': submitted && v$.name.$error }"
          aria-describedby="name-error"
        />
        <small 
          v-if="submitted && v$.name.$error" 
          id="name-error"
          class="p-error"
        >
          {{ getErrorMessage('name') }}
        </small>
      </div>

      <!-- Email -->
      <div class="field col-12 md:col-6">
        <label for="email">Email *</label>
        <InputText
          id="email"
          v-model="formData.email"
          type="email"
          :class="{ 'p-invalid': submitted && v$.email.$error }"
          aria-describedby="email-error"
        />
        <small 
          v-if="submitted && v$.email.$error" 
          id="email-error"
          class="p-error"
        >
          {{ getErrorMessage('email') }}
        </small>
      </div>

      <!-- Password -->
      <div class="field col-12 md:col-6">
        <label for="password">Mot de passe *</label>
        <Password
          id="password"
          v-model="formData.password"
          toggleMask
          :feedback="true"
          :class="{ 'p-invalid': submitted && v$.password.$error }"
          aria-describedby="password-error"
        >
          <template #header>
            <h6>Choisissez un mot de passe</h6>
          </template>
          <template #footer>
            <Divider />
            <p class="mt-2">Suggestions</p>
            <ul class="pl-2 ml-2 mt-0" style="line-height: 1.5">
              <li>Au moins une minuscule</li>
              <li>Au moins une majuscule</li>
              <li>Au moins un chiffre</li>
              <li>Minimum 8 caractères</li>
            </ul>
          </template>
        </Password>
        <small 
          v-if="submitted && v$.password.$error" 
          id="password-error"
          class="p-error"
        >
          {{ getErrorMessage('password') }}
        </small>
      </div>

      <!-- Country -->
      <div class="field col-12 md:col-6">
        <label for="country">Pays *</label>
        <Dropdown
          id="country"
          v-model="formData.country"
          :options="countries"
          optionLabel="name"
          placeholder="Sélectionnez un pays"
          :class="{ 'p-invalid': submitted && v$.country.$error }"
          aria-describedby="country-error"
        >
          <template #value="{ value, placeholder }">
            <div v-if="value" class="flex align-items-center">
              <span :class="`fi fi-${value.code.toLowerCase()} mr-2`" />
              <span>{{ value.name }}</span>
            </div>
            <span v-else>{{ placeholder }}</span>
          </template>
          <template #option="{ option }">
            <div class="flex align-items-center">
              <span :class="`fi fi-${option.code.toLowerCase()} mr-2`" />
              <span>{{ option.name }}</span>
            </div>
          </template>
        </Dropdown>
        <small 
          v-if="submitted && v$.country.$error" 
          id="country-error"
          class="p-error"
        >
          {{ getErrorMessage('country') }}
        </small>
      </div>

      <!-- Birthdate -->
      <div class="field col-12 md:col-6">
        <label for="birthdate">Date de naissance</label>
        <Calendar
          id="birthdate"
          v-model="formData.birthdate"
          dateFormat="dd/mm/yy"
          :showIcon="true"
          :maxDate="new Date()"
          placeholder="JJ/MM/AAAA"
        />
      </div>

      <!-- Gender -->
      <div class="field col-12 md:col-6">
        <label>Genre</label>
        <div class="flex flex-wrap gap-3">
          <div 
            v-for="gender in genders" 
            :key="gender"
            class="flex align-items-center"
          >
            <RadioButton
              :id="gender"
              v-model="formData.gender"
              :value="gender"
            />
            <label :for="gender" class="ml-2">{{ gender }}</label>
          </div>
        </div>
      </div>

      <!-- Interests -->
      <div class="field col-12">
        <label>Centres d'intérêt</label>
        <div class="flex flex-wrap gap-3">
          <div 
            v-for="interest in interestsOptions" 
            :key="interest"
            class="flex align-items-center"
          >
            <Checkbox
              :id="interest"
              v-model="formData.interests"
              :value="interest"
            />
            <label :for="interest" class="ml-2">{{ interest }}</label>
          </div>
        </div>
      </div>

      <!-- Bio -->
      <div class="field col-12">
        <label for="bio">Biographie</label>
        <Textarea
          id="bio"
          v-model="formData.bio"
          rows="4"
          :autoResize="true"
          placeholder="Parlez-nous de vous..."
        />
      </div>

      <!-- Newsletter -->
      <div class="field col-12">
        <div class="flex align-items-center">
          <Checkbox
            id="newsletter"
            v-model="formData.newsletter"
            :binary="true"
          />
          <label for="newsletter" class="ml-2">
            Je souhaite recevoir la newsletter
          </label>
        </div>
      </div>
    </div>

    <!-- Actions -->
    <div class="flex justify-content-end gap-2 mt-4">
      <Button 
        label="Réinitialiser" 
        type="button" 
        severity="secondary"
        outlined
        @click="resetForm"
      />
      <Button 
        label="Soumettre" 
        type="submit" 
        :loading="loading"
      />
    </div>
  </form>
</template>
```

## 🗨️ Dialogues et Overlays

### Dialog avec Form

```vue
<script setup lang="ts">
import { ref, computed } from 'vue';

interface Product {
  id: string;
  name: string;
  price: number;
}

const visible = ref(false);
const editMode = ref(false);
const product = ref<Partial<Product>>({});

const dialogTitle = computed(() => 
  editMode.value ? 'Modifier le produit' : 'Nouveau produit'
);

function openNew(): void {
  product.value = {};
  editMode.value = false;
  visible.value = true;
}

function openEdit(data: Product): void {
  product.value = { ...data };
  editMode.value = true;
  visible.value = true;
}

function save(): void {
  // Validation et sauvegarde
  visible.value = false;
}
</script>

<template>
  <Dialog
    v-model:visible="visible"
    :header="dialogTitle"
    :modal="true"
    :closable="true"
    :draggable="false"
    :style="{ width: '450px' }"
    class="p-fluid"
  >
    <div class="field">
      <label for="product-name">Nom</label>
      <InputText 
        id="product-name"
        v-model="product.name" 
        autofocus
      />
    </div>

    <div class="field">
      <label for="product-price">Prix</label>
      <InputNumber
        id="product-price"
        v-model="product.price"
        mode="currency"
        currency="EUR"
        locale="fr-FR"
      />
    </div>

    <template #footer>
      <Button 
        label="Annuler" 
        icon="pi pi-times" 
        text
        @click="visible = false"
      />
      <Button 
        label="Enregistrer" 
        icon="pi pi-check" 
        @click="save"
      />
    </template>
  </Dialog>
</template>
```

### Confirmation Dialog

```vue
<script setup lang="ts">
import { useConfirm } from 'primevue/useconfirm';
import { useToast } from 'primevue/usetoast';

const confirm = useConfirm();
const toast = useToast();

function confirmDelete(product: Product): void {
  confirm.require({
    message: `Êtes-vous sûr de vouloir supprimer "${product.name}" ?`,
    header: 'Confirmation de suppression',
    icon: 'pi pi-exclamation-triangle',
    acceptClass: 'p-button-danger',
    acceptLabel: 'Supprimer',
    rejectLabel: 'Annuler',
    accept: () => {
      // Supprimer le produit
      toast.add({
        severity: 'success',
        summary: 'Succès',
        detail: 'Produit supprimé',
        life: 3000,
      });
    },
    reject: () => {
      toast.add({
        severity: 'info',
        summary: 'Annulé',
        detail: 'Suppression annulée',
        life: 3000,
      });
    },
  });
}
</script>

<template>
  <!-- À placer à la racine de l'application -->
  <Toast />
  <ConfirmDialog />
</template>
```

## 🧩 Composants Wrapper Réutilisables

### AppDataTable Générique

```vue
<!-- components/app/AppDataTable.vue -->
<script setup lang="ts" generic="T extends Record<string, unknown>">
import { ref, computed } from 'vue';
import type { DataTableFilterMeta } from 'primevue/datatable';

interface Column {
  field: keyof T & string;
  header: string;
  sortable?: boolean;
  filterable?: boolean;
  style?: string;
  bodySlot?: string;
}

interface Props {
  items: T[];
  columns: Column[];
  loading?: boolean;
  paginator?: boolean;
  rows?: number;
  dataKey?: keyof T & string;
  selectable?: boolean;
  globalFilterFields?: string[];
}

const props = withDefaults(defineProps<Props>(), {
  loading: false,
  paginator: true,
  rows: 10,
  dataKey: 'id' as keyof T & string,
  selectable: false,
  globalFilterFields: () => [],
});

const emit = defineEmits<{
  select: [items: T[]];
  rowClick: [item: T];
}>();

const selectedItems = ref<T[]>([]);
const filters = ref<DataTableFilterMeta>({
  global: { value: null, matchMode: 'contains' },
});

const filteredColumns = computed(() => 
  props.columns.filter(col => col.filterable)
);

function onSelectionChange(selection: T[]): void {
  selectedItems.value = selection;
  emit('select', selection);
}
</script>

<template>
  <DataTable
    v-model:selection="selectedItems"
    v-model:filters="filters"
    :value="items"
    :loading="loading"
    :paginator="paginator"
    :rows="rows"
    :dataKey="dataKey"
    :globalFilterFields="globalFilterFields"
    filterDisplay="menu"
    responsiveLayout="scroll"
    paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
    :rowsPerPageOptions="[5, 10, 25, 50]"
    @selectionChange="onSelectionChange"
    @rowClick="emit('rowClick', $event.data)"
  >
    <!-- Header slot -->
    <template #header>
      <slot name="header">
        <div class="flex justify-content-between">
          <slot name="header-start" />
          <span class="p-input-icon-left">
            <i class="pi pi-search" />
            <InputText 
              v-model="filters.global.value" 
              placeholder="Rechercher..."
            />
          </span>
        </div>
      </slot>
    </template>

    <!-- Selection column -->
    <Column 
      v-if="selectable"
      selectionMode="multiple" 
      style="width: 3rem"
    />

    <!-- Dynamic columns -->
    <Column
      v-for="col in columns"
      :key="col.field"
      :field="col.field"
      :header="col.header"
      :sortable="col.sortable"
      :style="col.style"
    >
      <template v-if="col.bodySlot" #body="slotProps">
        <slot :name="col.bodySlot" v-bind="slotProps" />
      </template>
    </Column>

    <!-- Actions column -->
    <Column v-if="$slots.actions" style="width: 8rem">
      <template #body="slotProps">
        <slot name="actions" v-bind="slotProps" />
      </template>
    </Column>

    <!-- Empty state -->
    <template #empty>
      <slot name="empty">
        <div class="text-center py-4">
          <i class="pi pi-inbox text-4xl text-400" />
          <p class="text-600 mt-2">Aucun résultat</p>
        </div>
      </slot>
    </template>
  </DataTable>
</template>
```

### Usage du Composant Wrapper

```vue
<script setup lang="ts">
import AppDataTable from '@/components/app/AppDataTable.vue';

interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  status: 'active' | 'inactive';
}

const users = ref<User[]>([]);
const loading = ref(false);

const columns = [
  { field: 'name', header: 'Nom', sortable: true },
  { field: 'email', header: 'Email', sortable: true },
  { field: 'role', header: 'Rôle', sortable: true },
  { field: 'status', header: 'Statut', bodySlot: 'status' },
];

function onSelect(selected: User[]): void {
  console.log('Selected:', selected);
}
</script>

<template>
  <AppDataTable
    :items="users"
    :columns="columns"
    :loading="loading"
    :selectable="true"
    :globalFilterFields="['name', 'email']"
    @select="onSelect"
  >
    <!-- Custom status column -->
    <template #status="{ data }">
      <Tag 
        :value="data.status" 
        :severity="data.status === 'active' ? 'success' : 'danger'"
      />
    </template>

    <!-- Actions -->
    <template #actions="{ data }">
      <Button icon="pi pi-pencil" text rounded />
      <Button icon="pi pi-trash" text rounded severity="danger" />
    </template>
  </AppDataTable>
</template>
```

## 🎯 Accessibilité

### ARIA et Navigation

```vue
<template>
  <!-- Formulaire accessible -->
  <form @submit.prevent="submit" role="form" aria-label="Formulaire d'inscription">
    <div class="field">
      <label id="email-label" for="email">Email *</label>
      <InputText
        id="email"
        v-model="email"
        aria-labelledby="email-label"
        aria-describedby="email-help email-error"
        aria-required="true"
        :aria-invalid="hasError"
      />
      <small id="email-help" class="p-text-secondary">
        Votre adresse email professionnelle
      </small>
      <small v-if="hasError" id="email-error" class="p-error" role="alert">
        {{ errorMessage }}
      </small>
    </div>

    <Button 
      type="submit"
      label="S'inscrire"
      :loading="loading"
      :disabled="!isValid"
      aria-describedby="submit-help"
    />
    <small id="submit-help" class="sr-only">
      Soumet le formulaire d'inscription
    </small>
  </form>

  <!-- DataTable accessible -->
  <DataTable
    :value="items"
    role="grid"
    aria-label="Liste des produits"
  >
    <Column field="name" header="Nom" aria-sort="none" />
  </DataTable>
</template>

<style scoped>
/* Screen reader only */
.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}
</style>
```
