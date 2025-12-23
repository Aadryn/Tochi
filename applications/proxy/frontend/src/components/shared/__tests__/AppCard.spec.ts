import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/vue'
import AppCard from '@/components/shared/AppCard.vue'

describe('AppCard', () => {
  describe('rendu de base', () => {
    it('doit rendre le slot par défaut', () => {
      render(AppCard, {
        slots: {
          default: '<p>Contenu de la carte</p>',
        },
      })

      expect(screen.getByText('Contenu de la carte')).toBeTruthy()
    })

    it('doit rendre le titre quand fourni', () => {
      render(AppCard, {
        props: { title: 'Titre de la carte' },
      })

      expect(screen.getByText('Titre de la carte')).toBeTruthy()
    })

    it('doit rendre le sous-titre quand fourni', () => {
      render(AppCard, {
        props: {
          title: 'Titre',
          subtitle: 'Sous-titre explicatif',
        },
      })

      expect(screen.getByText('Sous-titre explicatif')).toBeTruthy()
    })
  })

  describe('slots', () => {
    it('doit rendre le slot header', () => {
      render(AppCard, {
        slots: {
          header: '<div data-testid="custom-header">Header personnalisé</div>',
        },
      })

      expect(screen.getByTestId('custom-header')).toBeTruthy()
    })

    it('doit rendre le slot actions', () => {
      render(AppCard, {
        props: { title: 'Titre' },
        slots: {
          actions: '<button>Action</button>',
        },
      })

      expect(screen.getByRole('button', { name: 'Action' })).toBeTruthy()
    })

    it('doit rendre le slot footer', () => {
      render(AppCard, {
        slots: {
          footer: '<div data-testid="footer">Footer</div>',
        },
      })

      expect(screen.getByTestId('footer')).toBeTruthy()
    })
  })

  describe('padding', () => {
    it('doit avoir du padding par défaut', () => {
      const { container } = render(AppCard, {
        slots: {
          default: 'Contenu',
        },
      })

      expect(container.querySelector('.no-padding')).toBeFalsy()
    })

    it('doit appliquer no-padding quand padding est false', () => {
      const { container } = render(AppCard, {
        props: { padding: false },
        slots: {
          default: 'Contenu',
        },
      })

      expect(container.querySelector('.no-padding')).toBeTruthy()
    })
  })

  describe('structure', () => {
    it('doit avoir la classe card', () => {
      const { container } = render(AppCard, {
        slots: {
          default: 'Contenu',
        },
      })

      expect(container.querySelector('.card')).toBeTruthy()
    })

    it('doit afficher le header uniquement si titre ou slot header présent', () => {
      const { container: withTitle } = render(AppCard, {
        props: { title: 'Titre' },
      })

      const { container: withoutTitle } = render(AppCard)

      expect(withTitle.querySelector('.card-header')).toBeTruthy()
      expect(withoutTitle.querySelector('.card-header')).toBeFalsy()
    })
  })
})
