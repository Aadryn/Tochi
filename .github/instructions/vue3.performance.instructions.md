---
description: Vue 3 Performance - Optimisation, lazy loading, memoization, virtual scroll, devtools
name: Vue3_Performance
applyTo: "**/*.vue,**/*.ts"
---

# Vue 3 Performance

Guide complet pour optimiser les performances des applications Vue 3.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** `v-if` et `v-for` sur le même élément
- **Ne crée jamais** de fonctions ou objets inline dans le template
- **N'importe jamais** de gros composants de manière synchrone
- **Ne mute jamais** les props ou le state directement
- **N'utilise jamais** `deep: true` sur les watchers sans nécessité
- **N'oublie jamais** de nettoyer les event listeners et intervals

## ✅ À FAIRE

- **Utilise toujours** `v-once` pour le contenu statique
- **Utilise toujours** `shallowRef` quand la réactivité profonde n'est pas nécessaire
- **Utilise toujours** le lazy loading pour les routes et composants lourds
- **Utilise toujours** `computed` au lieu de méthodes pour les valeurs dérivées
- **Utilise toujours** `v-memo` pour les listes avec rendu coûteux
- **Mesure toujours** avant d'optimiser (Vue DevTools, Performance tab)

## 🚀 Optimisation du Rendu

### v-once pour Contenu Statique

```vue
<template>
  <!-- ✅ BON : Contenu qui ne change jamais -->
  <header v-once>
    <h1>{{ appTitle }}</h1>
    <nav>
      <a href="/about">À propos</a>
      <a href="/contact">Contact</a>
    </nav>
  </header>

  <!-- Contenu réactif normal -->
  <main>
    <p>{{ dynamicContent }}</p>
  </main>
</template>
```

### v-memo pour Memoization

```vue
<template>
  <!-- ✅ BON : Liste avec rendu coûteux, mémorisée par item.id et item.selected -->
  <div 
    v-for="item in list" 
    :key="item.id"
    v-memo="[item.id, item.selected]"
    class="expensive-item"
  >
    <ExpensiveComponent :data="item" />
  </div>

  <!-- ✅ BON : Mémoriser uniquement quand selectedId change -->
  <div v-memo="[selectedId]">
    <ComplexVisualization :data="chartData" />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';

const selectedId = ref<string | null>(null);
const list = ref([/* ... */]);
</script>
```

### Éviter v-if avec v-for

```vue
<template>
  <!-- ❌ MAUVAIS : v-if et v-for sur le même élément -->
  <div 
    v-for="item in items" 
    v-if="item.isVisible"
    :key="item.id"
  >
    {{ item.name }}
  </div>

  <!-- ✅ BON : Filtrer avec computed -->
  <div 
    v-for="item in visibleItems" 
    :key="item.id"
  >
    {{ item.name }}
  </div>

  <!-- ✅ BON : Ou wrapper avec template -->
  <template v-for="item in items" :key="item.id">
    <div v-if="item.isVisible">
      {{ item.name }}
    </div>
  </template>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';

const items = ref([/* ... */]);

/ ✅ Computed pour filtrer
const visibleItems = computed(() => 
  items.value.filter(item => item.isVisible)
);
</script>
```

### Éviter les Fonctions Inline

```vue
<template>
  <!-- ❌ MAUVAIS : Fonction créée à chaque rendu -->
  <button @click="() => handleClick(item.id)">Click</button>
  <div :style="{ color: getColor(item.status) }">Text</div>

  <!-- ✅ BON : Référence de fonction -->
  <button @click="handleItemClick">Click</button>
  <div :style="itemStyle">Text</div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{ item: { id: string; status: string } }>();

/ ✅ Handler défini une seule fois
function handleItemClick(): void {
  handleClick(props.item.id);
}

/ ✅ Computed pour les styles
const itemStyle = computed(() => ({
  color: getColor(props.item.status),
}));
</script>
```

## 📦 Lazy Loading

### Composants Async

```vue
<script setup lang="ts">
import { defineAsyncComponent } from 'vue';

/ ✅ Lazy loading basique
const HeavyChart = defineAsyncComponent(
  () => import('@/components/HeavyChart.vue')
);

/ ✅ Avec loading et error states
const HeavyTable = defineAsyncComponent({
  loader: () => import('@/components/HeavyTable.vue'),
  loadingComponent: () => import('@/components/LoadingSpinner.vue'),
  errorComponent: () => import('@/components/ErrorState.vue'),
  delay: 200, / Délai avant d'afficher loading
  timeout: 10000, / Timeout en ms
});

/ ✅ Avec Suspense
const AsyncDashboard = defineAsyncComponent(
  () => import('@/components/Dashboard.vue')
);
</script>

<template>
  <!-- Avec Suspense pour async setup -->
  <Suspense>
    <template #default>
      <AsyncDashboard />
    </template>
    <template #fallback>
      <LoadingSpinner />
    </template>
  </Suspense>

  <!-- Composant conditionnel -->
  <HeavyChart v-if="showChart" :data="chartData" />
</template>
```

### Route Lazy Loading

```typescript
/ router/routes.ts
const routes = [
  {
    path: '/dashboard',
    component: () => import('@/views/DashboardView.vue'),
  },
  {
    path: '/admin',
    / Grouper les chunks par feature
    component: () => import(/* webpackChunkName: "admin" */ '@/views/AdminLayout.vue'),
    children: [
      {
        path: 'users',
        component: () => import(/* webpackChunkName: "admin" */ '@/views/AdminUsersView.vue'),
      },
      {
        path: 'settings',
        component: () => import(/* webpackChunkName: "admin" */ '@/views/AdminSettingsView.vue'),
      },
    ],
  },
];
```

### Lazy Loading Conditionnel

```vue
<script setup lang="ts">
import { ref, shallowRef, watch } from 'vue';

/ ✅ Charger le composant seulement quand nécessaire
const ChartComponent = shallowRef<Component | null>(null);
const showChart = ref(false);

watch(showChart, async (show) => {
  if (show && !ChartComponent.value) {
    const module = await import('@/components/HeavyChart.vue');
    ChartComponent.value = module.default;
  }
});
</script>

<template>
  <button @click="showChart = true">Afficher le graphique</button>
  
  <component 
    v-if="ChartComponent && showChart" 
    :is="ChartComponent" 
    :data="data" 
  />
</template>
```

## 🧠 Réactivité Optimisée

### shallowRef vs ref

```typescript
import { ref, shallowRef, triggerRef } from 'vue';

/ ❌ ref avec deep reactivity (coûteux pour gros objets)
const deepData = ref({
  users: [/* 1000 users */],
  metadata: { /* ... */ },
});

/ ✅ shallowRef - réactivité uniquement sur .value
const shallowData = shallowRef({
  users: [/* 1000 users */],
  metadata: { /* ... */ },
});

/ Pour déclencher une update avec shallowRef
function updateUser(index: number, name: string): void {
  shallowData.value.users[index].name = name;
  triggerRef(shallowData); / Déclencher manuellement
}

/ Ou remplacer l'objet entier
function setUsers(newUsers: User[]): void {
  shallowData.value = {
    ...shallowData.value,
    users: newUsers,
  };
}
```

### shallowReactive

```typescript
import { shallowReactive } from 'vue';

/ ✅ Réactivité uniquement au premier niveau
const state = shallowReactive({
  count: 0,
  nested: { value: 1 }, / Pas réactif en profondeur
});

state.count++; / ✅ Déclenche une update
state.nested.value++; / ❌ Ne déclenche PAS d'update
state.nested = { value: 2 }; / ✅ Déclenche une update
```

### Computed avec Cache

```typescript
import { computed, ref } from 'vue';

const items = ref<Item[]>([]);
const filter = ref('');

/ ✅ computed est caché et recalculé seulement si dépendances changent
const filteredItems = computed(() => {
  console.log('Filtering...'); / Appelé seulement quand items ou filter change
  return items.value.filter(item => 
    item.name.toLowerCase().includes(filter.value.toLowerCase())
  );
});

/ ❌ Méthode recalculée à chaque rendu
function getFilteredItems(): Item[] {
  console.log('Filtering...'); / Appelé à CHAQUE rendu
  return items.value.filter(item => 
    item.name.toLowerCase().includes(filter.value.toLowerCase())
  );
}
```

### Watchers Optimisés

```typescript
import { watch, watchEffect, ref } from 'vue';

const user = ref<User | null>(null);

/ ❌ MAUVAIS : deep watcher coûteux
watch(
  user,
  (newUser) => { /* ... */ },
  { deep: true } / Observe TOUTES les propriétés
);

/ ✅ BON : Observer seulement ce qui est nécessaire
watch(
  () => user.value?.name,
  (newName) => { /* ... */ }
);

/ ✅ BON : Observer plusieurs propriétés spécifiques
watch(
  [() => user.value?.name, () => user.value?.email],
  ([newName, newEmail]) => { /* ... */ }
);

/ ✅ BON : watchEffect avec cleanup
watchEffect((onCleanup) => {
  const controller = new AbortController();
  
  fetchUser(user.value?.id, { signal: controller.signal });
  
  onCleanup(() => {
    controller.abort();
  });
});
```

## 📜 Virtual Scrolling

### Liste Virtualisée

```vue
<!-- components/VirtualList.vue -->
<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';

interface Props {
  items: unknown[];
  itemHeight: number;
  containerHeight: number;
  overscan?: number;
}

const props = withDefaults(defineProps<Props>(), {
  overscan: 5,
});

const scrollTop = ref(0);
const containerRef = ref<HTMLElement | null>(null);

/ Calcul des items visibles
const visibleRange = computed(() => {
  const start = Math.floor(scrollTop.value / props.itemHeight);
  const visibleCount = Math.ceil(props.containerHeight / props.itemHeight);
  
  return {
    start: Math.max(0, start - props.overscan),
    end: Math.min(props.items.length, start + visibleCount + props.overscan),
  };
});

const visibleItems = computed(() => {
  const { start, end } = visibleRange.value;
  return props.items.slice(start, end).map((item, index) => ({
    item,
    index: start + index,
  }));
});

const totalHeight = computed(() => props.items.length * props.itemHeight);
const offsetY = computed(() => visibleRange.value.start * props.itemHeight);

function onScroll(event: Event): void {
  scrollTop.value = (event.target as HTMLElement).scrollTop;
}
</script>

<template>
  <div 
    ref="containerRef"
    class="virtual-list-container"
    :style="{ height: `${containerHeight}px` }"
    @scroll="onScroll"
  >
    <div 
      class="virtual-list-spacer"
      :style="{ height: `${totalHeight}px` }"
    >
      <div 
        class="virtual-list-content"
        :style="{ transform: `translateY(${offsetY}px)` }"
      >
        <div
          v-for="{ item, index } in visibleItems"
          :key="index"
          class="virtual-list-item"
          :style="{ height: `${itemHeight}px` }"
        >
          <slot :item="item" :index="index" />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.virtual-list-container {
  overflow-y: auto;
  position: relative;
}

.virtual-list-spacer {
  position: relative;
}

.virtual-list-content {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
}
</style>
```

### Usage avec vue-virtual-scroller

```vue
<script setup lang="ts">
import { RecycleScroller } from 'vue-virtual-scroller';
import 'vue-virtual-scroller/dist/vue-virtual-scroller.css';

interface Item {
  id: string;
  name: string;
}

defineProps<{
  items: Item[];
}>();
</script>

<template>
  <RecycleScroller
    class="scroller"
    :items="items"
    :item-size="50"
    key-field="id"
    v-slot="{ item }"
  >
    <div class="item">
      {{ item.name }}
    </div>
  </RecycleScroller>
</template>

<style scoped>
.scroller {
  height: 400px;
}

.item {
  height: 50px;
  padding: 12px;
}
</style>
```

## 🖼️ Images et Assets

### Lazy Loading d'Images

```vue
<script setup lang="ts">
import { ref, onMounted } from 'vue';

interface Props {
  src: string;
  alt: string;
  placeholder?: string;
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: '/placeholder.jpg',
});

const imageRef = ref<HTMLImageElement | null>(null);
const isLoaded = ref(false);
const currentSrc = ref(props.placeholder);

onMounted(() => {
  if (!imageRef.value) return;

  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          / Charger l'image réelle
          const img = new Image();
          img.onload = () => {
            currentSrc.value = props.src;
            isLoaded.value = true;
          };
          img.src = props.src;
          observer.disconnect();
        }
      });
    },
    { rootMargin: '50px' }
  );

  observer.observe(imageRef.value);
});
</script>

<template>
  <img
    ref="imageRef"
    :src="currentSrc"
    :alt="alt"
    :class="{ 'is-loaded': isLoaded }"
    loading="lazy"
  />
</template>

<style scoped>
img {
  transition: opacity 0.3s ease;
  opacity: 0.5;
}

img.is-loaded {
  opacity: 1;
}
</style>
```

### Responsive Images

```vue
<template>
  <picture>
    <source
      media="(min-width: 1200px)"
      :srcset="`${imagePath}-large.webp`"
      type="image/webp"
    />
    <source
      media="(min-width: 768px)"
      :srcset="`${imagePath}-medium.webp`"
      type="image/webp"
    />
    <source
      :srcset="`${imagePath}-small.webp`"
      type="image/webp"
    />
    <img
      :src="`${imagePath}-small.jpg`"
      :alt="alt"
      loading="lazy"
      decoding="async"
    />
  </picture>
</template>
```

## 🧹 Memory Management

### Cleanup des Resources

```vue
<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';

const data = ref<Data | null>(null);
let interval: number | null = null;
let abortController: AbortController | null = null;

onMounted(() => {
  / Polling avec interval
  interval = window.setInterval(fetchData, 5000);
  
  / Event listener global
  window.addEventListener('resize', handleResize);
  
  / Fetch initial
  fetchData();
});

onUnmounted(() => {
  / ✅ Nettoyer l'interval
  if (interval) {
    clearInterval(interval);
  }
  
  / ✅ Annuler les requêtes en cours
  if (abortController) {
    abortController.abort();
  }
  
  / ✅ Retirer les event listeners
  window.removeEventListener('resize', handleResize);
  
  / ✅ Nettoyer les références
  data.value = null;
});

async function fetchData(): Promise<void> {
  abortController = new AbortController();
  
  try {
    const response = await fetch('/api/data', {
      signal: abortController.signal,
    });
    data.value = await response.json();
  } catch (error) {
    if ((error as Error).name !== 'AbortError') {
      console.error('Fetch error:', error);
    }
  }
}

function handleResize(): void {
  / ...
}
</script>
```

### Composable avec Cleanup

```typescript
/ composables/usePolling.ts
import { ref, onUnmounted, type Ref } from 'vue';

interface UsePollingOptions<T> {
  fetcher: () => Promise<T>;
  interval: number;
  immediate?: boolean;
}

export function usePolling<T>(options: UsePollingOptions<T>): {
  data: Ref<T | null>;
  error: Ref<Error | null>;
  isLoading: Ref<boolean>;
  start: () => void;
  stop: () => void;
} {
  const { fetcher, interval, immediate = true } = options;
  
  const data = ref<T | null>(null) as Ref<T | null>;
  const error = ref<Error | null>(null);
  const isLoading = ref(false);
  
  let intervalId: number | null = null;
  let abortController: AbortController | null = null;

  async function poll(): Promise<void> {
    isLoading.value = true;
    abortController = new AbortController();
    
    try {
      data.value = await fetcher();
      error.value = null;
    } catch (e) {
      if ((e as Error).name !== 'AbortError') {
        error.value = e as Error;
      }
    } finally {
      isLoading.value = false;
    }
  }

  function start(): void {
    if (intervalId) return;
    poll();
    intervalId = window.setInterval(poll, interval);
  }

  function stop(): void {
    if (intervalId) {
      clearInterval(intervalId);
      intervalId = null;
    }
    if (abortController) {
      abortController.abort();
    }
  }

  if (immediate) {
    start();
  }

  / ✅ Cleanup automatique
  onUnmounted(stop);

  return { data, error, isLoading, start, stop };
}
```

## 📊 Monitoring Performance

### Vue DevTools Markers

```typescript
import { onMounted, onUpdated } from 'vue';

/ Markers pour DevTools Performance
onMounted(() => {
  performance.mark('component-mounted');
});

onUpdated(() => {
  performance.mark('component-updated');
  performance.measure('update-duration', 'component-mounted', 'component-updated');
});

/ Mesurer une opération coûteuse
function expensiveOperation(): void {
  performance.mark('operation-start');
  
  / ... opération ...
  
  performance.mark('operation-end');
  performance.measure('operation-duration', 'operation-start', 'operation-end');
  
  const measures = performance.getEntriesByName('operation-duration');
  console.log(`Operation took ${measures[0].duration}ms`);
}
```

### Custom Performance Hook

```typescript
/ composables/usePerformance.ts
import { onMounted, onUpdated, onUnmounted, getCurrentInstance } from 'vue';

export function usePerformanceMonitor(componentName?: string): void {
  const instance = getCurrentInstance();
  const name = componentName || instance?.type.__name || 'Unknown';
  
  let mountTime = 0;
  let updateCount = 0;

  onMounted(() => {
    mountTime = performance.now();
    console.log(`[${name}] Mounted at ${mountTime.toFixed(2)}ms`);
  });

  onUpdated(() => {
    updateCount++;
    const now = performance.now();
    console.log(`[${name}] Update #${updateCount} at ${now.toFixed(2)}ms`);
  });

  onUnmounted(() => {
    const lifetime = performance.now() - mountTime;
    console.log(`[${name}] Unmounted after ${lifetime.toFixed(2)}ms, ${updateCount} updates`);
  });
}
```

## ⚡ Build Optimization

### Vite Configuration

```typescript
/ vite.config.ts
import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';

export default defineConfig({
  plugins: [vue()],
  build: {
    / Activer le tree-shaking
    treeshake: true,
    
    / Minification
    minify: 'terser',
    terserOptions: {
      compress: {
        drop_console: true,
        drop_debugger: true,
      },
    },
    
    / Code splitting
    rollupOptions: {
      output: {
        manualChunks: {
          / Séparer les vendors
          'vendor-vue': ['vue', 'vue-router', 'pinia'],
          'vendor-ui': ['primevue'],
          'vendor-utils': ['lodash-es', 'date-fns'],
        },
      },
    },
    
    / Taille des chunks
    chunkSizeWarningLimit: 500,
  },
  
  / Optimisation des dépendances
  optimizeDeps: {
    include: ['vue', 'vue-router', 'pinia'],
  },
});
```

### Analyse du Bundle

```bash
# Installer rollup-plugin-visualizer
npm install -D rollup-plugin-visualizer

# Dans vite.config.ts
import { visualizer } from 'rollup-plugin-visualizer';

export default defineConfig({
  plugins: [
    vue(),
    visualizer({
      open: true,
      gzipSize: true,
      brotliSize: true,
    }),
  ],
});
```
