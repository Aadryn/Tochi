import { describe, it, expect } from 'vitest'
import { render, screen, fireEvent } from '@testing-library/vue'
import EmptyState from '@/components/shared/EmptyState.vue'

describe('EmptyState', () => {
  describe('rendu de base', () => {
    it('doit afficher le titre', () => {
      render(EmptyState, {
        props: { title: 'Aucun résultat' },
      })

      expect(screen.getByText('Aucun résultat')).toBeTruthy()
    })

    it('doit afficher la description quand fournie', () => {
      render(EmptyState, {
        props: {
          title: 'Aucun résultat',
          description: 'Essayez avec d\'autres critères',
        },
      })

      expect(screen.getByText('Essayez avec d\'autres critères')).toBeTruthy()
    })

    it('doit afficher une icône personnalisée', () => {
      const { container } = render(EmptyState, {
        props: {
          title: 'Aucun résultat',
          icon: 'pi pi-search',
        },
      })

      expect(container.querySelector('.pi-search')).toBeTruthy()
    })

    it('ne doit pas afficher d\'icône par défaut', () => {
      const { container } = render(EmptyState, {
        props: { title: 'Aucun résultat' },
      })

      expect(container.querySelector('.empty-icon')).toBeFalsy()
    })
  })

  describe('bouton d\'action', () => {
    it('doit afficher le bouton quand actionLabel est fourni', () => {
      render(EmptyState, {
        props: {
          title: 'Aucun provider',
          actionLabel: 'Ajouter un provider',
        },
      })

      expect(screen.getByRole('button', { name: 'Ajouter un provider' })).toBeTruthy()
    })

    it('doit émettre l\'événement action au clic', async () => {
      const { emitted } = render(EmptyState, {
        props: {
          title: 'Aucun provider',
          actionLabel: 'Ajouter',
        },
      })

      const button = screen.getByRole('button', { name: 'Ajouter' })
      await fireEvent.click(button)

      expect(emitted()).toHaveProperty('action')
      expect(emitted().action).toHaveLength(1)
    })

    it('ne doit pas afficher le bouton sans actionLabel', () => {
      render(EmptyState, {
        props: { title: 'Aucun résultat' },
      })

      expect(screen.queryByRole('button')).toBeFalsy()
    })
  })

  describe('slot', () => {
    it('doit rendre le contenu du slot par défaut', () => {
      render(EmptyState, {
        props: { title: 'Vide' },
        slots: {
          default: '<div data-testid="custom-content">Contenu personnalisé</div>',
        },
      })

      expect(screen.getByTestId('custom-content')).toBeTruthy()
    })
  })

  describe('structure', () => {
    it('doit avoir la classe empty-state', () => {
      const { container } = render(EmptyState, {
        props: { title: 'Vide' },
      })

      expect(container.querySelector('.empty-state')).toBeTruthy()
    })

    it('doit avoir la classe empty-title sur le titre', () => {
      const { container } = render(EmptyState, {
        props: { title: 'Vide' },
      })

      expect(container.querySelector('.empty-title')).toBeTruthy()
    })

    it('doit avoir la classe empty-description sur la description', () => {
      const { container } = render(EmptyState, {
        props: { title: 'Vide', description: 'Description' },
      })

      expect(container.querySelector('.empty-description')).toBeTruthy()
    })
  })
})
