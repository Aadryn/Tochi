---
description: Vue 3 Transitions - Animations d'entrée/sortie, TransitionGroup, hooks JavaScript, animations CSS
name: Vue3_Transitions
applyTo: "**/frontend/transitions/**/*.ts,**/frontend/components/**/*.vue"
---

# Vue 3 Transitions et Animations

Guide complet pour les transitions et animations Vue 3.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** de transitions sur des éléments avec v-show pour contenu lourd (préférer v-if)
- **Ne mélange jamais** CSS et JavaScript hooks sur la même transition sans :css="false"
- **N'oublie jamais** le callback `done()` dans les hooks JavaScript
- **N'anime jamais** des propriétés layout-triggering (left, top, width, height) sans will-change
- **Ne crée jamais** de memory leaks avec des animations GSAP non cleanup

## ✅ À FAIRE

- **Utilise toujours** des propriétés GPU-friendly (transform, opacity)
- **Utilise toujours** `will-change` pour les animations fréquentes
- **Utilise toujours** `appear` pour animer au premier rendu si nécessaire
- **Utilise toujours** des durées cohérentes (design tokens)
- **Cleanup toujours** les animations GSAP dans `onLeave` cancelled

## 📌 Transition de Base

### CSS Transitions

```vue
<script setup lang="ts">
import { ref } from 'vue';

const isVisible = ref(true);
</script>

<template>
  <button @click="isVisible = !isVisible">Toggle</button>
  
  <!-- Transition simple -->
  <Transition name="fade">
    <p v-if="isVisible">Hello World</p>
  </Transition>
</template>

<style scoped>
/* Classes de transition */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
```

### Classes de Transition

```css
/* 
  v-enter-from     : État initial d'entrée
  v-enter-active   : État actif d'entrée (transition/animation appliquée)
  v-enter-to       : État final d'entrée
  v-leave-from     : État initial de sortie
  v-leave-active   : État actif de sortie
  v-leave-to       : État final de sortie
*/

/* Fade */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Slide + Fade */
.slide-fade-enter-active {
  transition: all 0.3s ease-out;
}

.slide-fade-leave-active {
  transition: all 0.3s cubic-bezier(1, 0.5, 0.8, 1);
}

.slide-fade-enter-from,
.slide-fade-leave-to {
  transform: translateX(20px);
  opacity: 0;
}

/* Scale */
.scale-enter-active,
.scale-leave-active {
  transition: all 0.3s ease;
}

.scale-enter-from,
.scale-leave-to {
  opacity: 0;
  transform: scale(0.9);
}

/* Slide Up */
.slide-up-enter-active,
.slide-up-leave-active {
  transition: all 0.3s ease;
}

.slide-up-enter-from {
  opacity: 0;
  transform: translateY(20px);
}

.slide-up-leave-to {
  opacity: 0;
  transform: translateY(-20px);
}
```

### CSS Animations (keyframes)

```vue
<template>
  <Transition name="bounce">
    <p v-if="isVisible">Bouncing content</p>
  </Transition>
</template>

<style scoped>
.bounce-enter-active {
  animation: bounce-in 0.5s;
}

.bounce-leave-active {
  animation: bounce-in 0.5s reverse;
}

@keyframes bounce-in {
  0% {
    transform: scale(0);
  }
  50% {
    transform: scale(1.25);
  }
  100% {
    transform: scale(1);
  }
}
</style>
```

## 🎨 Modes de Transition

```vue
<script setup lang="ts">
import { ref } from 'vue';

const current = ref('A');
</script>

<template>
  <button @click="current = current === 'A' ? 'B' : 'A'">
    Switch: {{ current }}
  </button>
  
  <!-- mode="out-in" : attendre sortie avant entrée -->
  <Transition name="fade" mode="out-in">
    <component :is="current === 'A' ? ComponentA : ComponentB" :key="current" />
  </Transition>

  <!-- mode="in-out" : entrée avant sortie -->
  <Transition name="fade" mode="in-out">
    <div :key="current">{{ current }}</div>
  </Transition>

  <!-- Sans mode : entrée et sortie simultanées (overlay) -->
  <Transition name="fade">
    <div :key="current">{{ current }}</div>
  </Transition>
</template>
```

## 🎭 Transitions Conditionnelles

```vue
<script setup lang="ts">
import { ref, computed } from 'vue';

type Direction = 'left' | 'right';

const currentIndex = ref(0);
const direction = ref<Direction>('right');

const transitionName = computed(() => 
  direction.value === 'right' ? 'slide-right' : 'slide-left'
);

function navigate(newIndex: number): void {
  direction.value = newIndex > currentIndex.value ? 'right' : 'left';
  currentIndex.value = newIndex;
}
</script>

<template>
  <div class="carousel">
    <button @click="navigate(currentIndex - 1)">Prev</button>
    <button @click="navigate(currentIndex + 1)">Next</button>
    
    <Transition :name="transitionName" mode="out-in">
      <div :key="currentIndex" class="slide">
        Slide {{ currentIndex }}
      </div>
    </Transition>
  </div>
</template>

<style scoped>
.slide-right-enter-active,
.slide-right-leave-active,
.slide-left-enter-active,
.slide-left-leave-active {
  transition: all 0.3s ease;
}

.slide-right-enter-from {
  transform: translateX(100%);
  opacity: 0;
}

.slide-right-leave-to {
  transform: translateX(-100%);
  opacity: 0;
}

.slide-left-enter-from {
  transform: translateX(-100%);
  opacity: 0;
}

.slide-left-leave-to {
  transform: translateX(100%);
  opacity: 0;
}
</style>
```

## 🔄 TransitionGroup

### Liste Animée

```vue
<script setup lang="ts">
import { ref, computed } from 'vue';

interface Item {
  id: string;
  text: string;
}

const items = ref<Item[]>([
  { id: '1', text: 'Item 1' },
  { id: '2', text: 'Item 2' },
  { id: '3', text: 'Item 3' },
]);

let nextId = 4;

function addItem(): void {
  const index = Math.floor(Math.random() * items.value.length);
  items.value.splice(index, 0, {
    id: String(nextId++),
    text: `Item ${nextId - 1}`,
  });
}

function removeItem(id: string): void {
  const index = items.value.findIndex((item) => item.id === id);
  if (index !== -1) {
    items.value.splice(index, 1);
  }
}

function shuffle(): void {
  items.value = [...items.value].sort(() => Math.random() - 0.5);
}
</script>

<template>
  <div>
    <button @click="addItem">Add</button>
    <button @click="shuffle">Shuffle</button>
    
    <!-- tag="ul" : élément wrapper rendu -->
    <TransitionGroup name="list" tag="ul" class="list">
      <li v-for="item in items" :key="item.id" class="list-item">
        {{ item.text }}
        <button @click="removeItem(item.id)">×</button>
      </li>
    </TransitionGroup>
  </div>
</template>

<style scoped>
.list {
  position: relative;
  padding: 0;
  list-style: none;
}

.list-item {
  display: flex;
  justify-content: space-between;
  padding: 8px 16px;
  margin-bottom: 4px;
  background: #f3f4f6;
  border-radius: 4px;
}

/* Transitions d'entrée/sortie */
.list-enter-active,
.list-leave-active {
  transition: all 0.5s ease;
}

.list-enter-from,
.list-leave-to {
  opacity: 0;
  transform: translateX(30px);
}

/* IMPORTANT : v-move pour les déplacements */
.list-move {
  transition: transform 0.5s ease;
}

/* Fix pour leave pendant move */
.list-leave-active {
  position: absolute;
  width: 100%;
}
</style>
```

### Staggered Animations

```vue
<script setup lang="ts">
import { ref, computed } from 'vue';

interface Item {
  id: string;
  msg: string;
}

const query = ref('');
const list = ref<Item[]>([
  { id: '1', msg: 'Bruce Lee' },
  { id: '2', msg: 'Jackie Chan' },
  { id: '3', msg: 'Jet Li' },
  { id: '4', msg: 'Donnie Yen' },
  { id: '5', msg: 'Tony Jaa' },
]);

const filteredList = computed(() =>
  list.value.filter((item) =>
    item.msg.toLowerCase().includes(query.value.toLowerCase())
  )
);

function onBeforeEnter(el: Element): void {
  (el as HTMLElement).style.opacity = '0';
  (el as HTMLElement).style.transform = 'translateY(20px)';
}

function onEnter(el: Element, done: () => void): void {
  const htmlEl = el as HTMLElement;
  const index = Number(htmlEl.dataset.index);
  const delay = index * 100;

  setTimeout(() => {
    htmlEl.style.transition = 'all 0.4s ease';
    htmlEl.style.opacity = '1';
    htmlEl.style.transform = 'translateY(0)';
    
    htmlEl.addEventListener('transitionend', done, { once: true });
  }, delay);
}

function onLeave(el: Element, done: () => void): void {
  const htmlEl = el as HTMLElement;
  const index = Number(htmlEl.dataset.index);
  const delay = index * 50;

  setTimeout(() => {
    htmlEl.style.transition = 'all 0.3s ease';
    htmlEl.style.opacity = '0';
    htmlEl.style.transform = 'translateX(-20px)';
    
    htmlEl.addEventListener('transitionend', done, { once: true });
  }, delay);
}
</script>

<template>
  <input v-model="query" placeholder="Search..." />
  
  <TransitionGroup
    tag="ul"
    :css="false"
    @before-enter="onBeforeEnter"
    @enter="onEnter"
    @leave="onLeave"
  >
    <li
      v-for="(item, index) in filteredList"
      :key="item.id"
      :data-index="index"
    >
      {{ item.msg }}
    </li>
  </TransitionGroup>
</template>
```

## 🎬 JavaScript Hooks

### Avec GSAP

```vue
<script setup lang="ts">
import { ref } from 'vue';
import gsap from 'gsap';

const isVisible = ref(false);

function onBeforeEnter(el: Element): void {
  gsap.set(el, {
    scaleX: 0.8,
    scaleY: 1.2,
    opacity: 0,
  });
}

function onEnter(el: Element, done: () => void): void {
  gsap.to(el, {
    duration: 0.5,
    scaleX: 1,
    scaleY: 1,
    opacity: 1,
    ease: 'elastic.out(1, 0.5)',
    onComplete: done,
  });
}

function onLeave(el: Element, done: () => void): void {
  gsap.to(el, {
    duration: 0.3,
    scaleX: 0.8,
    scaleY: 1.2,
    opacity: 0,
    ease: 'power2.in',
    onComplete: done,
  });
}
</script>

<template>
  <button @click="isVisible = !isVisible">Toggle</button>
  
  <!-- :css="false" désactive les classes CSS -->
  <Transition
    :css="false"
    @before-enter="onBeforeEnter"
    @enter="onEnter"
    @leave="onLeave"
  >
    <div v-if="isVisible" class="box">
      Animated with GSAP
    </div>
  </Transition>
</template>
```

### Hooks Disponibles

```vue
<script setup lang="ts">
// Hooks d'entrée
function onBeforeEnter(el: Element): void {
  // Avant que l'élément soit inséré
}

function onEnter(el: Element, done: () => void): void {
  // Quand l'élément est inséré (transition en cours)
  // IMPORTANT: appeler done() quand terminé
  done();
}

function onAfterEnter(el: Element): void {
  // Après la transition d'entrée
}

function onEnterCancelled(el: Element): void {
  // Transition d'entrée annulée
}

// Hooks de sortie
function onBeforeLeave(el: Element): void {
  // Avant la transition de sortie
}

function onLeave(el: Element, done: () => void): void {
  // Pendant la transition de sortie
  // IMPORTANT: appeler done() quand terminé
  done();
}

function onAfterLeave(el: Element): void {
  // Après la transition de sortie
}

function onLeaveCancelled(el: Element): void {
  // Transition de sortie annulée (v-show only)
}
</script>

<template>
  <Transition
    @before-enter="onBeforeEnter"
    @enter="onEnter"
    @after-enter="onAfterEnter"
    @enter-cancelled="onEnterCancelled"
    @before-leave="onBeforeLeave"
    @leave="onLeave"
    @after-leave="onAfterLeave"
    @leave-cancelled="onLeaveCancelled"
  >
    <div v-if="show">Content</div>
  </Transition>
</template>
```

## 🏁 Transition au Premier Rendu (appear)

```vue
<script setup lang="ts">
// Le contenu est visible au premier rendu et animé
</script>

<template>
  <!-- appear active les transitions au mount initial -->
  <Transition name="fade" appear>
    <div>Animé au premier rendu</div>
  </Transition>

  <!-- Classes personnalisées pour appear -->
  <Transition
    appear
    appear-from-class="custom-appear-from"
    appear-active-class="custom-appear-active"
    appear-to-class="custom-appear-to"
  >
    <div>Custom appear</div>
  </Transition>

  <!-- JavaScript hooks pour appear -->
  <Transition
    appear
    @before-appear="onBeforeAppear"
    @appear="onAppear"
    @after-appear="onAfterAppear"
  >
    <div>JS appear</div>
  </Transition>
</template>

<style scoped>
.custom-appear-from {
  opacity: 0;
  transform: scale(0.5);
}

.custom-appear-active {
  transition: all 0.5s ease;
}

.custom-appear-to {
  opacity: 1;
  transform: scale(1);
}
</style>
```

## 🎨 Classes Custom et Libraries Tierces

### Avec Animate.css

```vue
<script setup lang="ts">
import { ref } from 'vue';
import 'animate.css';

const isVisible = ref(true);
</script>

<template>
  <button @click="isVisible = !isVisible">Toggle</button>
  
  <Transition
    enter-active-class="animate__animated animate__bounceIn"
    leave-active-class="animate__animated animate__bounceOut"
  >
    <div v-if="isVisible" class="box">
      Animate.css
    </div>
  </Transition>

  <!-- Avec durée personnalisée -->
  <Transition
    enter-active-class="animate__animated animate__fadeInUp animate__faster"
    leave-active-class="animate__animated animate__fadeOutDown animate__faster"
  >
    <div v-if="isVisible">Fast animation</div>
  </Transition>
</template>
```

### Avec Motion One

```vue
<script setup lang="ts">
import { ref } from 'vue';
import { animate } from 'motion';

const isVisible = ref(true);

function onEnter(el: Element, done: () => void): void {
  animate(
    el,
    { opacity: [0, 1], y: [20, 0] },
    { duration: 0.4, easing: 'ease-out' }
  ).finished.then(done);
}

function onLeave(el: Element, done: () => void): void {
  animate(
    el,
    { opacity: [1, 0], y: [0, -20] },
    { duration: 0.3, easing: 'ease-in' }
  ).finished.then(done);
}
</script>

<template>
  <Transition :css="false" @enter="onEnter" @leave="onLeave">
    <div v-if="isVisible">Motion One animation</div>
  </Transition>
</template>
```

## 🔧 Composable de Transition Réutilisable

```typescript
// composables/useTransition.ts
import { ref, computed, type ComputedRef } from 'vue';
import type { TransitionProps } from 'vue';

interface TransitionConfig {
  name: string;
  duration?: number;
  enterDelay?: number;
  leaveDelay?: number;
  mode?: 'out-in' | 'in-out' | 'default';
}

interface UseTransitionReturn {
  transitionProps: ComputedRef<TransitionProps>;
  isAnimating: ComputedRef<boolean>;
  setAnimating: (value: boolean) => void;
}

export function useTransition(config: TransitionConfig): UseTransitionReturn {
  const isAnimating = ref(false);
  
  const transitionProps = computed<TransitionProps>(() => ({
    name: config.name,
    mode: config.mode === 'default' ? undefined : config.mode,
    onBeforeEnter: () => {
      isAnimating.value = true;
    },
    onAfterEnter: () => {
      isAnimating.value = false;
    },
    onBeforeLeave: () => {
      isAnimating.value = true;
    },
    onAfterLeave: () => {
      isAnimating.value = false;
    },
  }));

  return {
    transitionProps,
    isAnimating: computed(() => isAnimating.value),
    setAnimating: (value: boolean) => {
      isAnimating.value = value;
    },
  };
}

// composables/useStaggeredList.ts
import { ref, watch, type Ref } from 'vue';
import gsap from 'gsap';

interface StaggerConfig {
  staggerDelay?: number;
  duration?: number;
  ease?: string;
}

interface UseStaggeredListReturn {
  onBeforeEnter: (el: Element) => void;
  onEnter: (el: Element, done: () => void) => void;
  onLeave: (el: Element, done: () => void) => void;
}

export function useStaggeredList(
  items: Ref<unknown[]>,
  config: StaggerConfig = {}
): UseStaggeredListReturn {
  const { staggerDelay = 0.1, duration = 0.4, ease = 'power2.out' } = config;
  
  const animations = new Map<Element, gsap.core.Tween>();

  function onBeforeEnter(el: Element): void {
    gsap.set(el, { opacity: 0, y: 20 });
  }

  function onEnter(el: Element, done: () => void): void {
    const htmlEl = el as HTMLElement;
    const index = Number(htmlEl.dataset.index) || 0;
    
    const tween = gsap.to(el, {
      opacity: 1,
      y: 0,
      duration,
      delay: index * staggerDelay,
      ease,
      onComplete: () => {
        animations.delete(el);
        done();
      },
    });

    animations.set(el, tween);
  }

  function onLeave(el: Element, done: () => void): void {
    // Cancel any existing animation
    const existing = animations.get(el);
    if (existing) {
      existing.kill();
    }

    const tween = gsap.to(el, {
      opacity: 0,
      x: -20,
      duration: duration * 0.75,
      ease: 'power2.in',
      onComplete: () => {
        animations.delete(el);
        done();
      },
    });

    animations.set(el, tween);
  }

  // Cleanup on items change
  watch(items, () => {
    animations.forEach((tween) => tween.kill());
    animations.clear();
  });

  return {
    onBeforeEnter,
    onEnter,
    onLeave,
  };
}
```

## 📦 Composants de Transition Réutilisables

```vue
<!-- components/transitions/FadeTransition.vue -->
<script setup lang="ts">
withDefaults(defineProps<{
  duration?: number;
  mode?: 'out-in' | 'in-out';
  appear?: boolean;
}>(), {
  duration: 300,
  mode: 'out-in',
  appear: false,
});
</script>

<template>
  <Transition
    name="fade"
    :mode="mode"
    :appear="appear"
    :style="{ '--duration': `${duration}ms` }"
  >
    <slot />
  </Transition>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity var(--duration, 300ms) ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
```

```vue
<!-- components/transitions/SlideTransition.vue -->
<script setup lang="ts">
type Direction = 'up' | 'down' | 'left' | 'right';

withDefaults(defineProps<{
  direction?: Direction;
  duration?: number;
  distance?: number;
  mode?: 'out-in' | 'in-out';
}>(), {
  direction: 'up',
  duration: 300,
  distance: 20,
  mode: 'out-in',
});
</script>

<template>
  <Transition
    :name="`slide-${direction}`"
    :mode="mode"
    :style="{
      '--duration': `${duration}ms`,
      '--distance': `${distance}px`,
    }"
  >
    <slot />
  </Transition>
</template>

<style scoped>
.slide-up-enter-active,
.slide-up-leave-active,
.slide-down-enter-active,
.slide-down-leave-active,
.slide-left-enter-active,
.slide-left-leave-active,
.slide-right-enter-active,
.slide-right-leave-active {
  transition: all var(--duration) ease;
}

/* Slide Up */
.slide-up-enter-from {
  opacity: 0;
  transform: translateY(var(--distance));
}

.slide-up-leave-to {
  opacity: 0;
  transform: translateY(calc(var(--distance) * -1));
}

/* Slide Down */
.slide-down-enter-from {
  opacity: 0;
  transform: translateY(calc(var(--distance) * -1));
}

.slide-down-leave-to {
  opacity: 0;
  transform: translateY(var(--distance));
}

/* Slide Left */
.slide-left-enter-from {
  opacity: 0;
  transform: translateX(var(--distance));
}

.slide-left-leave-to {
  opacity: 0;
  transform: translateX(calc(var(--distance) * -1));
}

/* Slide Right */
.slide-right-enter-from {
  opacity: 0;
  transform: translateX(calc(var(--distance) * -1));
}

.slide-right-leave-to {
  opacity: 0;
  transform: translateX(var(--distance));
}
</style>
```

```vue
<!-- components/transitions/ExpandTransition.vue -->
<script setup lang="ts">
function onBeforeEnter(el: Element): void {
  const htmlEl = el as HTMLElement;
  htmlEl.style.height = '0';
  htmlEl.style.opacity = '0';
  htmlEl.style.overflow = 'hidden';
}

function onEnter(el: Element, done: () => void): void {
  const htmlEl = el as HTMLElement;
  htmlEl.style.transition = 'height 0.3s ease, opacity 0.3s ease';
  htmlEl.style.height = `${htmlEl.scrollHeight}px`;
  htmlEl.style.opacity = '1';
  
  htmlEl.addEventListener('transitionend', () => {
    htmlEl.style.height = '';
    htmlEl.style.overflow = '';
    done();
  }, { once: true });
}

function onLeave(el: Element, done: () => void): void {
  const htmlEl = el as HTMLElement;
  htmlEl.style.height = `${htmlEl.scrollHeight}px`;
  htmlEl.style.overflow = 'hidden';
  
  // Force reflow
  htmlEl.offsetHeight;
  
  htmlEl.style.transition = 'height 0.3s ease, opacity 0.3s ease';
  htmlEl.style.height = '0';
  htmlEl.style.opacity = '0';
  
  htmlEl.addEventListener('transitionend', done, { once: true });
}
</script>

<template>
  <Transition
    :css="false"
    @before-enter="onBeforeEnter"
    @enter="onEnter"
    @leave="onLeave"
  >
    <slot />
  </Transition>
</template>
```

## 🎯 Patterns Avancés

### Page Transitions avec Vue Router

```vue
<!-- App.vue -->
<script setup lang="ts">
import { useRoute } from 'vue-router';
import { computed } from 'vue';

const route = useRoute();

// Transition basée sur la meta de la route
const transitionName = computed(() => 
  (route.meta.transition as string) ?? 'fade'
);
</script>

<template>
  <RouterView v-slot="{ Component, route }">
    <Transition :name="transitionName" mode="out-in">
      <component :is="Component" :key="route.path" />
    </Transition>
  </RouterView>
</template>

<style>
/* Page transitions */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.slide-enter-active,
.slide-leave-active {
  transition: all 0.3s ease;
}

.slide-enter-from {
  opacity: 0;
  transform: translateX(50px);
}

.slide-leave-to {
  opacity: 0;
  transform: translateX(-50px);
}
</style>
```

```typescript
// router/index.ts
const routes = [
  {
    path: '/',
    component: Home,
    meta: { transition: 'fade' },
  },
  {
    path: '/about',
    component: About,
    meta: { transition: 'slide' },
  },
  {
    path: '/contact',
    component: Contact,
    meta: { transition: 'scale' },
  },
];
```

### Modal avec Transition

```vue
<!-- components/Modal.vue -->
<script setup lang="ts">
const props = defineProps<{
  modelValue: boolean;
}>();

const emit = defineEmits<{
  'update:modelValue': [value: boolean];
}>();

function close(): void {
  emit('update:modelValue', false);
}
</script>

<template>
  <Teleport to="body">
    <!-- Overlay -->
    <Transition name="fade">
      <div
        v-if="modelValue"
        class="modal-overlay"
        @click="close"
      />
    </Transition>

    <!-- Modal content -->
    <Transition name="modal">
      <div v-if="modelValue" class="modal-container">
        <div class="modal-content" @click.stop>
          <button class="modal-close" @click="close">×</button>
          <slot />
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 100;
}

.modal-container {
  position: fixed;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 101;
  pointer-events: none;
}

.modal-content {
  background: white;
  padding: 24px;
  border-radius: 8px;
  max-width: 500px;
  width: 90%;
  max-height: 90vh;
  overflow-y: auto;
  pointer-events: auto;
  position: relative;
}

.modal-close {
  position: absolute;
  top: 8px;
  right: 8px;
  background: none;
  border: none;
  font-size: 24px;
  cursor: pointer;
}

/* Overlay transition */
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

/* Modal transition */
.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s ease;
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
  transform: scale(0.9) translateY(20px);
}
</style>
```

## ⚡ Optimisations Performance

```vue
<script setup lang="ts">
import { ref } from 'vue';

const showHeavyComponent = ref(false);
</script>

<template>
  <!-- ✅ will-change pour animer fréquemment -->
  <Transition name="slide">
    <div v-if="isVisible" style="will-change: transform, opacity;">
      Optimized transition
    </div>
  </Transition>

  <!-- ✅ GPU-accelerated properties only -->
  <style>
  .optimized-enter-active,
  .optimized-leave-active {
    /* ✅ BON : transform et opacity sont GPU-friendly */
    transition: transform 0.3s ease, opacity 0.3s ease;
  }

  /* ❌ MAUVAIS : propriétés layout-triggering */
  .bad-transition {
    transition: width 0.3s, height 0.3s, left 0.3s;
  }
  </style>

  <!-- ✅ Lazy load composants lourds avec transition -->
  <Transition name="fade" mode="out-in">
    <Suspense v-if="showHeavyComponent">
      <template #default>
        <HeavyAsyncComponent />
      </template>
      <template #fallback>
        <LoadingSpinner />
      </template>
    </Suspense>
  </Transition>
</template>

<style>
/* ✅ Réduire paint avec contain */
.animated-list-item {
  contain: layout style;
}

/* ✅ Utiliser transform au lieu de left/top */
.slide-enter-from {
  transform: translateX(100%); /* ✅ */
  /* left: 100%; ❌ */
}
</style>
```
