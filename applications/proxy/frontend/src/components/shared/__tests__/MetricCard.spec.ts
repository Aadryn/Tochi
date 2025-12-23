import { describe, it, expect } from 'vitest'
import { render, screen } from '@testing-library/vue'
import MetricCard from '@/components/shared/MetricCard.vue'

describe('MetricCard', () => {
  describe('rendu de base', () => {
    it('doit afficher le titre', () => {
      render(MetricCard, {
        props: {
          title: 'Requêtes totales',
          value: '1,234',
        },
      })

      expect(screen.getByText('Requêtes totales')).toBeTruthy()
    })

    it('doit afficher la valeur', () => {
      render(MetricCard, {
        props: {
          title: 'Requêtes totales',
          value: '1,234',
        },
      })

      expect(screen.getByText('1,234')).toBeTruthy()
    })

    it('doit afficher l\'icône quand fournie', () => {
      const { container } = render(MetricCard, {
        props: {
          title: 'Requêtes',
          value: '100',
          icon: 'pi pi-chart-bar',
        },
      })

      expect(container.querySelector('.pi-chart-bar')).toBeTruthy()
    })
  })

  describe('tendance (trend)', () => {
    it('doit afficher la tendance positive', () => {
      render(MetricCard, {
        props: {
          title: 'Requêtes',
          value: '100',
          trend: 15.5,
          trendLabel: 'vs hier',
        },
      })

      expect(screen.getByText('15.5%')).toBeTruthy()
      expect(screen.getByText('vs hier')).toBeTruthy()
    })

    it('doit afficher la tendance négative', () => {
      render(MetricCard, {
        props: {
          title: 'Erreurs',
          value: '50',
          trend: -10.2,
        },
      })

      expect(screen.getByText('10.2%')).toBeTruthy()
    })

    it('doit afficher la tendance à zéro', () => {
      render(MetricCard, {
        props: {
          title: 'Stable',
          value: '100',
          trend: 0,
        },
      })

      expect(screen.getByText('0%')).toBeTruthy()
    })

    it('doit appliquer la classe positive pour tendance positive', () => {
      const { container } = render(MetricCard, {
        props: {
          title: 'Requêtes',
          value: '100',
          trend: 10,
        },
      })

      expect(container.querySelector('.metric-trend.positive')).toBeTruthy()
    })

    it('doit ne pas appliquer la classe positive pour tendance négative', () => {
      const { container } = render(MetricCard, {
        props: {
          title: 'Erreurs',
          value: '50',
          trend: -5,
        },
      })

      const trendElement = container.querySelector('.metric-trend')
      expect(trendElement).toBeTruthy()
      expect(trendElement?.classList.contains('positive')).toBe(false)
    })
  })

  describe('affichage de l\'icône de tendance', () => {
    it('doit afficher une flèche vers le haut pour tendance positive', () => {
      const { container } = render(MetricCard, {
        props: {
          title: 'Requêtes',
          value: '100',
          trend: 10,
        },
      })

      expect(container.querySelector('.pi-arrow-up')).toBeTruthy()
    })

    it('doit afficher une flèche vers le bas pour tendance négative', () => {
      const { container } = render(MetricCard, {
        props: {
          title: 'Erreurs',
          value: '50',
          trend: -5,
        },
      })

      expect(container.querySelector('.pi-arrow-down')).toBeTruthy()
    })
  })

  describe('couleurs', () => {
    it('doit appliquer la couleur success', () => {
      const { container } = render(MetricCard, {
        props: {
          title: 'Succès',
          value: '95%',
          color: 'success',
        },
      })

      expect(container.querySelector('.color-success')).toBeTruthy()
    })

    it('doit appliquer la couleur warning', () => {
      const { container } = render(MetricCard, {
        props: {
          title: 'Attention',
          value: '75%',
          color: 'warning',
        },
      })

      expect(container.querySelector('.color-warning')).toBeTruthy()
    })

    it('doit appliquer la couleur danger', () => {
      const { container } = render(MetricCard, {
        props: {
          title: 'Erreurs',
          value: '10',
          color: 'danger',
        },
      })

      expect(container.querySelector('.color-danger')).toBeTruthy()
    })

    it('doit appliquer la couleur primary par défaut', () => {
      const { container } = render(MetricCard, {
        props: {
          title: 'Métrique',
          value: '100',
        },
      })

      expect(container.querySelector('.color-primary')).toBeTruthy()
    })
  })
})
