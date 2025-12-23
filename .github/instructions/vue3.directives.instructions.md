---
description: Vue 3 Directives - Directives built-in, custom directives, modifiers, lifecycle hooks
name: Vue3_Directives
applyTo: "**/directives/**/*.ts,**/*.vue"
---

# Vue 3 Directives

Guide complet pour les directives Vue 3 built-in et custom.

## ⛔ À NE PAS FAIRE

- **N'utilise jamais** de logique métier complexe dans les directives
- **Ne modifie jamais** le DOM de manière non réversible sans cleanup
- **N'accède jamais** à des données réactives directement (passer en argument)
- **Ne crée jamais** de memory leaks (toujours cleanup dans `unmounted`)
- **N'utilise jamais** v-html avec du contenu utilisateur non sanitizé

## ✅ À FAIRE

- **Utilise toujours** les hooks de lifecycle appropriés
- **Utilise toujours** le cleanup dans `unmounted` ou `beforeUnmount`
- **Documente toujours** les directives custom avec leurs arguments
- **Type toujours** les directives avec TypeScript
- **Teste toujours** les directives sur différents éléments

## 📌 Directives Built-in

### v-if / v-else-if / v-else

```vue
<script setup lang="ts">
import { ref } from 'vue';

type Status = 'loading' | 'success' | 'error';
const status = ref<Status>('loading');
const isAdmin = ref(false);
</script>

<template>
  <!-- Conditionnel simple -->
  <div v-if="isAdmin">Contenu admin</div>
  
  <!-- Chaîne conditionnelle -->
  <div v-if="status === 'loading'">
    <LoadingSpinner />
  </div>
  <div v-else-if="status === 'error'">
    <ErrorMessage />
  </div>
  <div v-else>
    <SuccessContent />
  </div>

  <!-- Avec template (pas de wrapper DOM) -->
  <template v-if="isAdmin">
    <AdminHeader />
    <AdminSidebar />
    <AdminContent />
  </template>

  <!-- ❌ MAUVAIS : v-if avec v-for -->
  <div v-for="item in items" v-if="item.active">{{ item.name }}</div>

  <!-- ✅ BON : template wrapper ou computed -->
  <template v-for="item in items" :key="item.id">
    <div v-if="item.active">{{ item.name }}</div>
  </template>
</template>
```

### v-show

```vue
<script setup lang="ts">
import { ref } from 'vue';

const isVisible = ref(true);
const isExpanded = ref(false);
</script>

<template>
  <!-- v-show : garde l'élément dans le DOM (toggle display) -->
  <!-- Utiliser pour toggle fréquent -->
  <div v-show="isVisible" class="panel">
    Contenu du panneau
  </div>

  <!-- ✅ BON : Menu dropdown (toggle fréquent) -->
  <div v-show="isExpanded" class="dropdown-menu">
    <a href="#">Option 1</a>
    <a href="#">Option 2</a>
  </div>

  <!-- ❌ MAUVAIS : Contenu lourd qui se toggle rarement (utiliser v-if) -->
  <HeavyComponent v-show="rareCondition" />
  
  <!-- ✅ BON : Contenu lourd avec v-if -->
  <HeavyComponent v-if="rareCondition" />
</template>
```

### v-for

```vue
<script setup lang="ts">
import { ref, computed } from 'vue';

interface User {
  id: string;
  name: string;
  email: string;
}

const users = ref<User[]>([]);
const items = ref<string[]>(['a', 'b', 'c']);
</script>

<template>
  <!-- ✅ Toujours avec :key unique -->
  <ul>
    <li v-for="user in users" :key="user.id">
      {{ user.name }} - {{ user.email }}
    </li>
  </ul>

  <!-- Avec index (utiliser seulement si pas d'id unique) -->
  <ul>
    <li v-for="(item, index) in items" :key="index">
      {{ index + 1 }}. {{ item }}
    </li>
  </ul>

  <!-- Itérer sur un objet -->
  <dl>
    <template v-for="(value, key, index) in user" :key="key">
      <dt>{{ key }}</dt>
      <dd>{{ value }}</dd>
    </template>
  </dl>

  <!-- Itérer sur une range -->
  <span v-for="n in 5" :key="n">{{ n }}</span>

  <!-- Avec destructuring -->
  <div v-for="{ id, name, email } in users" :key="id">
    <h3>{{ name }}</h3>
    <p>{{ email }}</p>
  </div>

  <!-- Nested v-for -->
  <div v-for="category in categories" :key="category.id">
    <h2>{{ category.name }}</h2>
    <ul>
      <li v-for="product in category.products" :key="product.id">
        {{ product.name }}
      </li>
    </ul>
  </div>
</template>
```

### v-on (@)

```vue
<script setup lang="ts">
import { ref } from 'vue';

const count = ref(0);
const message = ref('');

function handleClick(event: MouseEvent): void {
  console.log('Clicked at', event.clientX, event.clientY);
}

function handleSubmit(event: Event): void {
  event.preventDefault();
  console.log('Form submitted');
}

function handleKeydown(event: KeyboardEvent): void {
  console.log('Key pressed:', event.key);
}
</script>

<template>
  <!-- Syntaxe courte @ -->
  <button @click="count++">Count: {{ count }}</button>
  
  <!-- Handler method -->
  <button @click="handleClick">Click me</button>
  
  <!-- Handler avec argument -->
  <button @click="handleClick($event)">With event</button>
  
  <!-- Modifiers -->
  <form @submit.prevent="handleSubmit">
    <input type="text" v-model="message" />
    <button type="submit">Submit</button>
  </form>

  <!-- Event modifiers -->
  <div @click.self="handleClick">
    <!-- .self : seulement si target === currentTarget -->
    <button @click.stop="handleClick">Stop propagation</button>
  </div>

  <!-- Key modifiers -->
  <input 
    @keydown.enter="handleSubmit"
    @keydown.esc="message = ''"
    @keyup.ctrl.s="save"
    @keydown.alt.shift.a="selectAll"
  />

  <!-- Mouse button modifiers -->
  <div 
    @click.left="handleLeftClick"
    @click.right.prevent="handleRightClick"
    @click.middle="handleMiddleClick"
  />

  <!-- Once modifier -->
  <button @click.once="handleClick">Only once</button>

  <!-- Passive modifier (améliore scroll performance) -->
  <div @scroll.passive="handleScroll" />

  <!-- Capture modifier -->
  <div @click.capture="handleClickCapture">
    <button @click="handleClick">Nested</button>
  </div>
</template>
```

### v-bind (:)

```vue
<script setup lang="ts">
import { ref, computed } from 'vue';

const imageUrl = ref('/path/to/image.jpg');
const isActive = ref(true);
const hasError = ref(false);
const customStyles = ref({
  color: 'red',
  fontSize: '14px',
});

const inputAttrs = computed(() => ({
  type: 'text',
  placeholder: 'Enter value',
  disabled: isActive.value,
}));
</script>

<template>
  <!-- Syntaxe courte : -->
  <img :src="imageUrl" :alt="imageAlt" />
  
  <!-- Binding de classe (object syntax) -->
  <div :class="{ active: isActive, 'has-error': hasError }">
    Class binding
  </div>

  <!-- Binding de classe (array syntax) -->
  <div :class="[isActive ? 'active' : '', hasError ? 'error' : '']">
    Array class binding
  </div>

  <!-- Binding de style (object) -->
  <div :style="{ color: textColor, fontSize: fontSize + 'px' }">
    Style binding
  </div>

  <!-- Binding de style (object ref) -->
  <div :style="customStyles">Styled content</div>

  <!-- Binding multiple attributs -->
  <input v-bind="inputAttrs" />

  <!-- Shorthand same-name (Vue 3.4+) -->
  <img :src :alt />

  <!-- Binding boolean -->
  <button :disabled="isLoading">Submit</button>

  <!-- Binding dynamique d'attribut -->
  <div :[dynamicAttr]="dynamicValue">Dynamic attr</div>

  <!-- Binding avec modifiers -->
  <div :class.camel="someClass" />
</template>
```

### v-model

```vue
<script setup lang="ts">
import { ref } from 'vue';

const text = ref('');
const number = ref(0);
const selected = ref<string | null>(null);
const multiSelected = ref<string[]>([]);
const checked = ref(false);
const radioValue = ref('option1');
</script>

<template>
  <!-- Input text -->
  <input v-model="text" type="text" />
  
  <!-- Avec modifiers -->
  <input v-model.trim="text" type="text" />
  <input v-model.lazy="text" type="text" />
  <input v-model.number="number" type="number" />

  <!-- Textarea -->
  <textarea v-model="text" rows="4" />

  <!-- Checkbox simple -->
  <input v-model="checked" type="checkbox" />

  <!-- Checkbox multiple -->
  <label>
    <input v-model="multiSelected" type="checkbox" value="option1" />
    Option 1
  </label>
  <label>
    <input v-model="multiSelected" type="checkbox" value="option2" />
    Option 2
  </label>

  <!-- Radio -->
  <label>
    <input v-model="radioValue" type="radio" value="option1" />
    Option 1
  </label>
  <label>
    <input v-model="radioValue" type="radio" value="option2" />
    Option 2
  </label>

  <!-- Select -->
  <select v-model="selected">
    <option disabled value="">Choisir...</option>
    <option value="a">Option A</option>
    <option value="b">Option B</option>
  </select>

  <!-- Select multiple -->
  <select v-model="multiSelected" multiple>
    <option value="a">Option A</option>
    <option value="b">Option B</option>
    <option value="c">Option C</option>
  </select>
</template>
```

### v-slot (#)

```vue
<script setup lang="ts">
interface User {
  id: string;
  name: string;
}

const users: User[] = [
  { id: '1', name: 'Alice' },
  { id: '2', name: 'Bob' },
];
</script>

<template>
  <!-- Slot par défaut -->
  <BaseCard>
    <p>Contenu du slot par défaut</p>
  </BaseCard>

  <!-- Slots nommés -->
  <BaseLayout>
    <template #header>
      <h1>Header Content</h1>
    </template>
    
    <template #default>
      <p>Main Content</p>
    </template>
    
    <template #footer>
      <p>Footer Content</p>
    </template>
  </BaseLayout>

  <!-- Scoped slots -->
  <UserList :users="users">
    <template #item="{ user, index }">
      <div class="user-card">
        <span>{{ index + 1 }}.</span>
        <strong>{{ user.name }}</strong>
      </div>
    </template>
  </UserList>

  <!-- Shorthand pour slot par défaut -->
  <UserList :users="users" v-slot="{ user }">
    <span>{{ user.name }}</span>
  </UserList>

  <!-- Conditional slots -->
  <BaseCard>
    <template v-if="showHeader" #header>
      <h2>Optional Header</h2>
    </template>
    <p>Content</p>
  </BaseCard>
</template>
```

### v-html et v-text

```vue
<script setup lang="ts">
import DOMPurify from 'dompurify';
import { computed } from 'vue';

const rawHtml = '<span style="color: red;">Red text</span>';
const userContent = '<script>alert("XSS")</script><p>Safe content</p>';

/ ✅ Sanitize HTML before rendering
const sanitizedHtml = computed(() => DOMPurify.sanitize(userContent));
</script>

<template>
  <!-- v-text : équivalent à {{ }} mais remplace tout le contenu -->
  <span v-text="message" />

  <!-- v-html : attention XSS ! -->
  <!-- ✅ BON : HTML de confiance (généré côté serveur) -->
  <div v-html="rawHtml" />

  <!-- ✅ BON : HTML sanitizé -->
  <div v-html="sanitizedHtml" />

  <!-- ❌ MAUVAIS : HTML utilisateur non sanitizé -->
  <div v-html="userContent" />
</template>
```

## 🛠️ Directives Custom

### Structure de Base

```typescript
/ directives/vFocus.ts
import type { Directive, DirectiveBinding } from 'vue';

/**
 * Directive pour focus automatique sur un élément
 * Usage: v-focus ou v-focus="condition"
 */
export const vFocus: Directive<HTMLElement, boolean | undefined> = {
  / Appelé avant que l'élément soit inséré dans le DOM
  beforeMount(el: HTMLElement, binding: DirectiveBinding<boolean | undefined>) {
    / binding.value : valeur passée (v-focus="value")
    / binding.oldValue : ancienne valeur (dans updated)
    / binding.arg : argument (v-focus:arg)
    / binding.modifiers : modifiers (v-focus.modifier)
    / binding.instance : instance du composant
    / binding.dir : la directive elle-même
  },

  / Appelé quand l'élément est inséré dans le DOM
  mounted(el: HTMLElement, binding: DirectiveBinding<boolean | undefined>) {
    if (binding.value !== false) {
      el.focus();
    }
  },

  / Appelé quand le VNode parent et ses enfants sont mis à jour
  updated(el: HTMLElement, binding: DirectiveBinding<boolean | undefined>) {
    if (binding.value && !binding.oldValue) {
      el.focus();
    }
  },

  / Appelé avant que l'élément soit retiré du DOM
  beforeUnmount(el: HTMLElement) {
    / Cleanup si nécessaire
  },

  / Appelé quand l'élément est retiré du DOM
  unmounted(el: HTMLElement) {
    / Cleanup final
  },
};

/ Shorthand (si seulement mounted et updated avec même logique)
export const vFocusShort: Directive<HTMLElement, boolean | undefined> = (el, binding) => {
  if (binding.value !== false) {
    el.focus();
  }
};
```

### Directive Click Outside

```typescript
/ directives/vClickOutside.ts
import type { Directive, DirectiveBinding } from 'vue';

type ClickOutsideHandler = (event: MouseEvent) => void;

interface ClickOutsideElement extends HTMLElement {
  __clickOutsideHandler?: (event: MouseEvent) => void;
}

/**
 * Directive pour détecter les clics en dehors d'un élément
 * Usage: v-click-outside="handler"
 */
export const vClickOutside: Directive<ClickOutsideElement, ClickOutsideHandler> = {
  mounted(el: ClickOutsideElement, binding: DirectiveBinding<ClickOutsideHandler>) {
    const handler = binding.value;
    
    if (typeof handler !== 'function') {
      console.warn('[v-click-outside] expects a function as value');
      return;
    }

    el.__clickOutsideHandler = (event: MouseEvent) => {
      const target = event.target as Node;
      
      / Vérifier que le clic est en dehors de l'élément
      if (!el.contains(target) && el !== target) {
        handler(event);
      }
    };

    / Utiliser capture pour intercepter avant que l'événement ne bulle
    document.addEventListener('click', el.__clickOutsideHandler, true);
  },

  beforeUnmount(el: ClickOutsideElement) {
    if (el.__clickOutsideHandler) {
      document.removeEventListener('click', el.__clickOutsideHandler, true);
      delete el.__clickOutsideHandler;
    }
  },
};
```

### Directive Intersection Observer

```typescript
/ directives/vIntersect.ts
import type { Directive, DirectiveBinding } from 'vue';

interface IntersectOptions {
  handler: (isIntersecting: boolean, entry: IntersectionObserverEntry) => void;
  options?: IntersectionObserverInit;
  once?: boolean;
}

interface IntersectElement extends HTMLElement {
  __intersectObserver?: IntersectionObserver;
}

/**
 * Directive pour Intersection Observer
 * Usage: v-intersect="{ handler, options, once }"
 * ou v-intersect="handler" pour une utilisation simple
 */
export const vIntersect: Directive<IntersectElement, IntersectOptions | ((isIntersecting: boolean) => void)> = {
  mounted(el: IntersectElement, binding) {
    const value = binding.value;
    
    / Normaliser les options
    const config: IntersectOptions = typeof value === 'function'
      ? { handler: (isIntersecting) => value(isIntersecting) }
      : value;

    const { handler, options = {}, once = false } = config;

    const observerOptions: IntersectionObserverInit = {
      root: options.root ?? null,
      rootMargin: options.rootMargin ?? '0px',
      threshold: options.threshold ?? 0,
    };

    el.__intersectObserver = new IntersectionObserver((entries) => {
      entries.forEach((entry) => {
        handler(entry.isIntersecting, entry);
        
        if (once && entry.isIntersecting) {
          el.__intersectObserver?.disconnect();
        }
      });
    }, observerOptions);

    el.__intersectObserver.observe(el);
  },

  beforeUnmount(el: IntersectElement) {
    el.__intersectObserver?.disconnect();
    delete el.__intersectObserver;
  },
};

/ Usage dans composant
/*
<template>
  <div v-intersect="handleIntersect">
    Contenu observé
  </div>

  <div v-intersect="{ handler: onVisible, once: true }">
    Charge une seule fois quand visible
  </div>

  <div v-intersect="{ 
    handler: onIntersect, 
    options: { threshold: 0.5, rootMargin: '100px' } 
  }">
    Options personnalisées
  </div>
</template>
*/
```

### Directive Tooltip

```typescript
/ directives/vTooltip.ts
import type { Directive, DirectiveBinding } from 'vue';

interface TooltipOptions {
  text: string;
  position?: 'top' | 'bottom' | 'left' | 'right';
  delay?: number;
}

interface TooltipElement extends HTMLElement {
  __tooltipElement?: HTMLDivElement;
  __tooltipShowTimeout?: number;
  __tooltipHideTimeout?: number;
  __tooltipHandlers?: {
    mouseenter: () => void;
    mouseleave: () => void;
  };
}

const POSITIONS = {
  top: { bottom: '100%', left: '50%', transform: 'translateX(-50%) translateY(-8px)' },
  bottom: { top: '100%', left: '50%', transform: 'translateX(-50%) translateY(8px)' },
  left: { right: '100%', top: '50%', transform: 'translateY(-50%) translateX(-8px)' },
  right: { left: '100%', top: '50%', transform: 'translateY(-50%) translateX(8px)' },
};

function createTooltip(text: string, position: TooltipOptions['position'] = 'top'): HTMLDivElement {
  const tooltip = document.createElement('div');
  tooltip.textContent = text;
  tooltip.className = 'v-tooltip';
  
  Object.assign(tooltip.style, {
    position: 'absolute',
    padding: '6px 12px',
    backgroundColor: 'rgba(0, 0, 0, 0.8)',
    color: 'white',
    borderRadius: '4px',
    fontSize: '12px',
    whiteSpace: 'nowrap',
    zIndex: '9999',
    pointerEvents: 'none',
    opacity: '0',
    transition: 'opacity 0.2s ease',
    ...POSITIONS[position],
  });

  return tooltip;
}

/**
 * Directive pour afficher un tooltip
 * Usage: v-tooltip="'Texte'" ou v-tooltip="{ text: 'Texte', position: 'bottom' }"
 */
export const vTooltip: Directive<TooltipElement, string | TooltipOptions> = {
  mounted(el: TooltipElement, binding: DirectiveBinding<string | TooltipOptions>) {
    const config: TooltipOptions = typeof binding.value === 'string'
      ? { text: binding.value }
      : binding.value;

    const { text, position = 'top', delay = 200 } = config;

    / Rendre l'élément positionné pour le tooltip
    if (getComputedStyle(el).position === 'static') {
      el.style.position = 'relative';
    }

    const tooltip = createTooltip(text, position);
    el.__tooltipElement = tooltip;

    const show = (): void => {
      clearTimeout(el.__tooltipHideTimeout);
      
      el.__tooltipShowTimeout = window.setTimeout(() => {
        el.appendChild(tooltip);
        / Force reflow pour l'animation
        tooltip.offsetHeight;
        tooltip.style.opacity = '1';
      }, delay);
    };

    const hide = (): void => {
      clearTimeout(el.__tooltipShowTimeout);
      
      tooltip.style.opacity = '0';
      el.__tooltipHideTimeout = window.setTimeout(() => {
        tooltip.remove();
      }, 200);
    };

    el.__tooltipHandlers = { mouseenter: show, mouseleave: hide };
    
    el.addEventListener('mouseenter', show);
    el.addEventListener('mouseleave', hide);
  },

  updated(el: TooltipElement, binding: DirectiveBinding<string | TooltipOptions>) {
    const config: TooltipOptions = typeof binding.value === 'string'
      ? { text: binding.value }
      : binding.value;

    if (el.__tooltipElement) {
      el.__tooltipElement.textContent = config.text;
    }
  },

  beforeUnmount(el: TooltipElement) {
    clearTimeout(el.__tooltipShowTimeout);
    clearTimeout(el.__tooltipHideTimeout);
    
    if (el.__tooltipHandlers) {
      el.removeEventListener('mouseenter', el.__tooltipHandlers.mouseenter);
      el.removeEventListener('mouseleave', el.__tooltipHandlers.mouseleave);
    }
    
    el.__tooltipElement?.remove();
  },
};
```

### Directive Ripple Effect

```typescript
/ directives/vRipple.ts
import type { Directive } from 'vue';

interface RippleElement extends HTMLElement {
  __rippleHandler?: (event: MouseEvent) => void;
}

/**
 * Directive pour effet ripple Material Design
 * Usage: v-ripple ou v-ripple="{ color: 'rgba(255,255,255,0.3)' }"
 */
export const vRipple: Directive<RippleElement, { color?: string } | undefined> = {
  mounted(el: RippleElement, binding) {
    const color = binding.value?.color ?? 'rgba(0, 0, 0, 0.2)';
    
    / Style de base pour le conteneur
    el.style.position = 'relative';
    el.style.overflow = 'hidden';

    el.__rippleHandler = (event: MouseEvent) => {
      const rect = el.getBoundingClientRect();
      const x = event.clientX - rect.left;
      const y = event.clientY - rect.top;

      const ripple = document.createElement('span');
      ripple.className = 'ripple-effect';
      
      const size = Math.max(rect.width, rect.height) * 2;
      
      Object.assign(ripple.style, {
        position: 'absolute',
        borderRadius: '50%',
        backgroundColor: color,
        width: `${size}px`,
        height: `${size}px`,
        left: `${x - size / 2}px`,
        top: `${y - size / 2}px`,
        transform: 'scale(0)',
        opacity: '1',
        pointerEvents: 'none',
        animation: 'ripple-animation 0.6s ease-out',
      });

      el.appendChild(ripple);

      ripple.addEventListener('animationend', () => {
        ripple.remove();
      });
    };

    el.addEventListener('click', el.__rippleHandler);

    / Ajouter les keyframes si pas déjà présents
    if (!document.querySelector('#ripple-keyframes')) {
      const style = document.createElement('style');
      style.id = 'ripple-keyframes';
      style.textContent = `
        @keyframes ripple-animation {
          to {
            transform: scale(1);
            opacity: 0;
          }
        }
      `;
      document.head.appendChild(style);
    }
  },

  beforeUnmount(el: RippleElement) {
    if (el.__rippleHandler) {
      el.removeEventListener('click', el.__rippleHandler);
    }
  },
};
```

## 📦 Enregistrement Global

```typescript
/ directives/index.ts
export { vFocus } from './vFocus';
export { vClickOutside } from './vClickOutside';
export { vIntersect } from './vIntersect';
export { vTooltip } from './vTooltip';
export { vRipple } from './vRipple';

/ main.ts
import { createApp } from 'vue';
import App from './App.vue';
import { vFocus, vClickOutside, vIntersect, vTooltip, vRipple } from './directives';

const app = createApp(App);

/ Enregistrement global
app.directive('focus', vFocus);
app.directive('click-outside', vClickOutside);
app.directive('intersect', vIntersect);
app.directive('tooltip', vTooltip);
app.directive('ripple', vRipple);

app.mount('#app');
```

## 🧪 Tests de Directives

```typescript
/ directives/__tests__/vFocus.spec.ts
import { mount } from '@vue/test-utils';
import { vFocus } from '../vFocus';

describe('vFocus directive', () => {
  it('focuses element on mount', async () => {
    const wrapper = mount({
      template: '<input v-focus />',
      directives: { focus: vFocus },
    }, {
      attachTo: document.body,
    });

    expect(document.activeElement).toBe(wrapper.find('input').element);
    wrapper.unmount();
  });

  it('does not focus when value is false', async () => {
    const wrapper = mount({
      template: '<input v-focus="false" />',
      directives: { focus: vFocus },
    }, {
      attachTo: document.body,
    });

    expect(document.activeElement).not.toBe(wrapper.find('input').element);
    wrapper.unmount();
  });
});

/ directives/__tests__/vClickOutside.spec.ts
import { mount } from '@vue/test-utils';
import { vClickOutside } from '../vClickOutside';

describe('vClickOutside directive', () => {
  it('calls handler when clicking outside', async () => {
    const handler = vi.fn();
    
    const wrapper = mount({
      template: '<div v-click-outside="handler"><button>Inside</button></div>',
      directives: { clickOutside: vClickOutside },
      setup() {
        return { handler };
      },
    }, {
      attachTo: document.body,
    });

    / Click inside - should not trigger
    await wrapper.find('button').trigger('click');
    expect(handler).not.toHaveBeenCalled();

    / Click outside - should trigger
    document.body.click();
    expect(handler).toHaveBeenCalled();

    wrapper.unmount();
  });
});
```

## 🎯 Usage Patterns

```vue
<script setup lang="ts">
import { ref } from 'vue';

const isDropdownOpen = ref(false);
const items = ref([1, 2, 3, 4, 5]);

function closeDropdown(): void {
  isDropdownOpen.value = false;
}

function handleVisible(isVisible: boolean): void {
  if (isVisible) {
    loadMoreItems();
  }
}

async function loadMoreItems(): Promise<void> {
  / Charger plus d'items
}
</script>

<template>
  <!-- Focus automatique -->
  <input v-focus placeholder="Auto-focused on mount" />

  <!-- Dropdown avec click outside -->
  <div class="dropdown" v-click-outside="closeDropdown">
    <button @click="isDropdownOpen = !isDropdownOpen">
      Toggle Dropdown
    </button>
    <div v-show="isDropdownOpen" class="dropdown-menu">
      <a href="#">Option 1</a>
      <a href="#">Option 2</a>
    </div>
  </div>

  <!-- Infinite scroll avec intersection -->
  <ul>
    <li v-for="item in items" :key="item">
      Item {{ item }}
    </li>
    <li v-intersect="handleVisible" class="loading-trigger">
      <LoadingSpinner />
    </li>
  </ul>

  <!-- Tooltips -->
  <button v-tooltip="'Action rapide'">
    <Icon name="lightning" />
  </button>
  
  <button v-tooltip="{ text: 'Supprimer', position: 'bottom' }">
    <Icon name="trash" />
  </button>

  <!-- Ripple effect -->
  <button v-ripple class="btn-primary">
    Click me
  </button>
  
  <button v-ripple="{ color: 'rgba(255, 255, 255, 0.4)' }" class="btn-dark">
    Custom ripple
  </button>
</template>
```
