import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'

import App from './App.vue'
import router from './router'
import { i18n } from './locales'

// PrimeVue styles
import 'primevue/resources/themes/lara-light-blue/theme.css'
import 'primevue/resources/primevue.min.css'
import 'primeicons/primeicons.css'
import 'primeflex/primeflex.css'

// Global styles
import './assets/main.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)
app.use(i18n) // Internationalization
app.use(PrimeVue, { ripple: true })

app.mount('#app')
