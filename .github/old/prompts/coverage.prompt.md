---
mode: 'agent'
model: Claude Sonnet 4
description: 'Improve code coverage to 100% in unit test projects'
---
continue d'implémenter/ajouter des tests unitaires pour obtenir une couverture de code globale de 100%. 
Priorise les tests sur les éléments avec le plus faible taux de couverture. 
Zero failing test, Zero build warning, Zero build errors. 
Ne jamais utiliser FluentAssertions et Moq. 
Zero skipped tests.
Toujours lancer le coverage avec la configuration coverlet.runsettings

Toujours utiliser les instructions suivantes :
- .github/instructions/csharp-test-instructions.md