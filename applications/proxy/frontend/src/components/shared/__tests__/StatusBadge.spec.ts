import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/vue'
import StatusBadge from '@/components/shared/StatusBadge.vue'

describe('StatusBadge', () => {
  describe('rendu du statut', () => {
    it('doit afficher le badge healthy correctement', () => {
      const { container } = render(StatusBadge, {
        props: { status: 'healthy' },
      })

      const badge = screen.getByText('Opérationnel')
      expect(badge).toBeTruthy()
      expect(container.querySelector('.severity-success')).toBeTruthy()
    })

    it('doit afficher le badge degraded correctement', () => {
      const { container } = render(StatusBadge, {
        props: { status: 'degraded' },
      })

      const badge = screen.getByText('Dégradé')
      expect(badge).toBeTruthy()
      expect(container.querySelector('.severity-warning')).toBeTruthy()
    })

    it('doit afficher le badge unhealthy correctement', () => {
      const { container } = render(StatusBadge, {
        props: { status: 'unhealthy' },
      })

      const badge = screen.getByText('Hors service')
      expect(badge).toBeTruthy()
      expect(container.querySelector('.severity-danger')).toBeTruthy()
    })

    it('doit afficher le badge active correctement', () => {
      const { container } = render(StatusBadge, {
        props: { status: 'active' },
      })

      const badge = screen.getByText('Actif')
      expect(badge).toBeTruthy()
      expect(container.querySelector('.severity-success')).toBeTruthy()
    })

    it('doit afficher le badge inactive correctement', () => {
      const { container } = render(StatusBadge, {
        props: { status: 'inactive' },
      })

      const badge = screen.getByText('Inactif')
      expect(badge).toBeTruthy()
      expect(container.querySelector('.severity-danger')).toBeTruthy()
    })
  })

  describe('tailles', () => {
    it('doit appliquer la taille small', () => {
      const { container } = render(StatusBadge, {
        props: { status: 'healthy', size: 'small' },
      })

      expect(container.querySelector('.size-small')).toBeTruthy()
    })

    it('doit utiliser la taille medium par défaut', () => {
      const { container } = render(StatusBadge, {
        props: { status: 'healthy' },
      })

      expect(container.querySelector('.size-medium')).toBeTruthy()
    })

    it('doit appliquer la taille large', () => {
      const { container } = render(StatusBadge, {
        props: { status: 'healthy', size: 'large' },
      })

      expect(container.querySelector('.size-large')).toBeTruthy()
    })
  })

  describe('affichage', () => {
    it('doit afficher le dot de statut', () => {
      const { container } = render(StatusBadge, {
        props: { status: 'healthy' },
      })

      expect(container.querySelector('.status-dot')).toBeTruthy()
    })

    it('doit afficher le label de statut', () => {
      const { container } = render(StatusBadge, {
        props: { status: 'healthy' },
      })

      expect(container.querySelector('.status-label')).toBeTruthy()
    })

    it('doit afficher un statut inconnu avec son label brut', () => {
      render(StatusBadge, {
        props: { status: 'unknown-status' },
      })

      expect(screen.getByText('unknown-status')).toBeTruthy()
    })
  })
})
