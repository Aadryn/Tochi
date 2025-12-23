import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/vue'
import LoadingSpinner from '@/components/shared/LoadingSpinner.vue'

describe('LoadingSpinner', () => {
  describe('rendu de base', () => {
    it('doit rendre le spinner quand loading est true', () => {
      const { container } = render(LoadingSpinner, {
        props: { loading: true },
      })

      expect(container.querySelector('.loading-spinner')).toBeTruthy()
    })

    it('ne doit pas rendre quand loading est false', () => {
      const { container } = render(LoadingSpinner, {
        props: { loading: false },
      })

      expect(container.querySelector('.loading-spinner')).toBeFalsy()
    })

    it('doit afficher le message par défaut', () => {
      render(LoadingSpinner, {
        props: { loading: true },
      })

      expect(screen.getByText('Chargement en cours...')).toBeTruthy()
    })

    it('doit afficher un message personnalisé', () => {
      render(LoadingSpinner, {
        props: { loading: true, message: 'Veuillez patienter' },
      })

      expect(screen.getByText('Veuillez patienter')).toBeTruthy()
    })
  })

  describe('icône', () => {
    it('doit afficher l\'icône de spinner', () => {
      const { container } = render(LoadingSpinner, {
        props: { loading: true },
      })

      expect(container.querySelector('.pi-spinner')).toBeTruthy()
    })

    it('doit avoir la classe pi-spin pour l\'animation', () => {
      const { container } = render(LoadingSpinner, {
        props: { loading: true },
      })

      expect(container.querySelector('.pi-spin')).toBeTruthy()
    })
  })

  describe('structure', () => {
    it('doit avoir la bonne structure HTML', () => {
      const { container } = render(LoadingSpinner, {
        props: { loading: true },
      })

      expect(container.querySelector('.spinner-icon')).toBeTruthy()
      expect(container.querySelector('.spinner-message')).toBeTruthy()
    })
  })
})
