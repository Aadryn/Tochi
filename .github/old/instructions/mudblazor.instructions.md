---
description: 'MudBlazor component craftsmanship'
applyTo: '**/*.razor,**/*.razor.cs,**/*.razor.css,**/*.razor.js'
---

# MudBlazor Component Playbook

> Cette fiche complète le guide `blazor.default.instructions.md`. Référez-vous d'abord à ce dernier pour la structure de base des composants; les sections ci-dessous couvrent des pratiques ciblées MudBlazor pour aller plus loin sans répétition inutile.

## Découpage et architecture des composants
- Évitez les composants « god object » : extrayez les groupes MudBlazor cohérents (cartes, dialogues, lignes de tableau) dans des composants enfants réutilisables. Chaque composant doit porter une responsabilité UI claire.
- Préférez des composants stateless pour l'affichage (recevant leurs données via `[Parameter]`) et encapsulez la logique métier ou de chargement dans des composants conteneurs ou des services.
- Partagez les fragments répétitifs (`RenderFragment`) pour composer `MudTable`, `MudGrid`, `MudTimeline`, etc. Cela réduit la duplication et améliore la testabilité.
- Pour les layouts complexes, composez `MudGrid` → `MudItem` ou `MudStack` en hiérarchies peu profondes ; fusionnez les conteneurs inutiles afin de limiter le DOM et conserver les animations fluides.
- Factorisez la configuration des `MudDialog`, `MudSnackbar`, `MudPopover` dans des helpers ou services dédiés afin de centraliser styles, positionnement et politiques d'accessibilité.
- Classez vos composants MudBlazor selon trois couches de conception :
	- **Fondation** : entrées, boutons enrichis, badges, tags… des briques UI génériques, exposées dans `Components/Foundation` ou équivalent.
	- **Composition** : combinaisons orientées interaction (barres d'outils, formulaires dynamiques, cartes enrichies) qui assemblent plusieurs éléments de fondation.
	- **Fonctionnalité** : composants orientés métier (tableaux de gestion, pages de workflow) hébergés dans `Features/...` et orchestrant la logique applicative.
	Gardez des noms de composants descriptifs centrés sur leur rôle réel (ex. `PromptToolbar`, `CollectionFilterChip`) sans jamais recourir aux termes « Atom », « Molecule », « Organism ».
- Chaque composant partagé doit documenter ses paramètres, slots (`RenderFragment`) et dépendances MudBlazor dans un fichier `README.md` court à la racine du dossier pour accélérer la découverte.
- Lorsqu'une variation visuelle n'implique pas de logique distincte, exposez des paramètres (`Color`, `Variant`, `Dense`) plutôt que de dériver un nouveau composant, afin de respecter la source de vérité.

## State management & données
- Synchronisez les formulaires via `MudForm` et exposez explicitement `IsValid`, `Errors`, `Reset()` depuis le composant parent pour éviter les appels directs sur l'instance du formulaire.
- Favorisez `EventCallback` / `EventCallback<T>` pour remonter les événements plutôt que des `Action` ou `Func`. Utilisez `InvokeAsync` afin de conserver la synchronisation du contexte Blazor.
- Centralisez les états complexes (filtres, pagination, sélection) dans des ViewModels dédiés ; injectez ces ViewModels en scoped afin de partager l'état entre composants siblings.
- Encapsulez les appels HTTP dans des services strongly-typed et fournissez au composant uniquement des DTO/records prêts à être affichés. Cela évite d'accrocher le composant à l'infrastructure.

## Grilles de gestion (ManagementDataGrid)
- Utilisez systématiquement `ManagementDataGrid<TItem>` pour les pages de gestion : il encapsule `MudDataGrid` avec toolbar, pagination, virtualisation et colonnes actions homogènes. Limitez le markup dans `.razor` aux sections `ChildContent`, `AdditionalToolbarContent` et `AdditionalActions`.
- Renseignez le titre, les permissions (`CanCreate`, `CanView`, `CanEdit`, `CanDelete`) et les colonnes via `TemplateColumn` en manipulant exclusivement des `nameof(ViewModel.Propriété)` pour garder les colonnes alignées avec les ViewModels.
- Alimentez la grille avec `ServerData="@LoadDataAsync"` (méthode définie dans le code-behind) renvoyant un `GridData<TItem>` construit à partir des handlers mediator. Ajustez la pagination (`PageNumber = state.Page + 1`) et traquez les filtres/sorts afin de maintenir un comportement cohérent avec le domaine.
- Déplacez les filtres client impossibles côté serveur dans le code-behind (`ApplyClientSideFilters`) et journalisez les décisions via `ILogger` pour faciliter le support.
- Exploitez `SearchString` / `SearchStringChanged` avec un debounce (500 ms) pour regénérer la grille sans surcharge réseau ; appelez `await DataGrid.ReloadAsync()` après chaque modification pertinente.
- Surfacez les actions contextuelles (approbation/rejet, navigation) via `AdditionalActions` en combinant `MudIconButton` + `AriaLabel`, et conservez les messages utilisateur dans `Resources/Culture` (`Culture.Format(...)`).

## Sécurité et robustesse
- N'affichez jamais du HTML non maîtrisé via `MarkupString` sans sanitation préalable ; privilégiez un rendu texte dans `MudText`. Utilisez une bibliothèque de sanitation côté service si le HTML est indispensable.
- Empêchez l'injection de scripts lors des bindings en appliquant des `InputType` appropriés (`InputType.Password`, `InputType.Email`, etc.) et en validant les données via DataAnnotations ou FluentValidation.
- Pour les composants interactifs (`MudAutocomplete`, `MudSelect`), vérifiez que les requêtes distantes prennent en compte l'identité de l'utilisateur et appliquent les ACL nécessaires. Ne renvoyez jamais d'objets complets en clair dans les options.
- Évitez de logger des informations sensibles dans les callbacks d'événements MudBlazor. Logguez uniquement des identifiants techniques ou des hachages.
- Activez systématiquement le `@attribute [ValidateAntiForgeryToken]` sur les pages Blazor Server qui exposent des formulaires critiques et injectez `IAntiforgery` si vous composez des requêtes manuelles.
- Chiffrez les secrets temporaires (tokens, clés API) côté serveur, et manipulez uniquement des substituts (`ReferenceId`) côté composant. Assurez-vous que les dialogues de confirmation masquent les identifiants sensibles.
- Limitez les tentatives d'actions sensibles (approbation, suppression) via un garde-fou de type `RateLimiter` ou `IQuotaService`, puis affichez un feedback MudSnackbar expliquant le blocage.
- Ajoutez une trace `CorrelationId` et le contexte utilisateur dans les logs de composants critiques afin d'améliorer la traçabilité lors des audits.

## Performance & réactivité
- Utilisez la virtualisation (`MudVirtualize`, propriété `Virtualize` de `MudTable`, `ServerData` paginé) pour les listes volumineuses. Fournissez un cache local (e.g. `IMemoryCache`) pour amortir les recharges.
- Limitez les re-rendus : implémentez `ShouldRender()` lorsque seuls certains paramètres déclenchent une mise à jour, et exploitez `@key` sur les listes MudBlazor pour stabiliser l'arbre DOM.
- Débouncerez les entrées utilisateur intensives (`MudTextField` → `DebounceInterval`) afin d'éviter l'appel en rafale des services.
- Préférez les paramètres `Instant` ou `Transition` adaptés sur `MudProgressCircular`, `MudSkeleton` et `MudOverlay` pour garder des feedbacks légers en évitant les animations lourdes.
- Chargez les modules JavaScript MudBlazor optionnels (`MudChart`, `MudEx`) uniquement lorsque nécessaire via `LazyLoad` ou `OnAfterRenderAsync` conditionnel.
- Mesurez la consommation mémoire et CPU des composants gourmands via Application Insights ou `dotnet-counters`; scindez ou cachez les portions lourdes (ex. colonnes calculées) lorsqu'elles dépassent vos budgets de perf.
- Offrez des niveaux de détail adaptatifs : chargez d'abord un résumé ( liste condensée ) puis les détails sur interaction (dialogue/expansion) pour réduire la charge initiale.

## Résilience applicative
- Encadrez les appels `ServerData` et actions de grille par des politiques de résilience (retry, circuit breaker) exposées via des services partagés ; en cas d'échec, affichez un état dégradé (`MudAlert` + bouton « Réessayer »).
- Utilisez `CancellationToken` partout où MudBlazor fournit des callbacks async (recherche, auto-complétion) pour annuler proprement les requêtes obsolètes.
- Pour les dialogues critiques (`MudDialog`), prévoyez un mécanisme de reprise (rechargement de la grille après fermeture, rappel automatique si l'utilisateur revient sur la page).
- Enregistrez des événements fonctionnels (`TelemetryClient.TrackEvent`) lorsqu'une action sensible réussit/échoue afin d'alimenter vos tableaux de bord de fiabilité.
- Externalisez les textes de secours (messages d'erreur, labels de fallback) dans les ressources pour garantir leur cohérence multi-langue lors des scénarios dégradés.

## Theming, styles et accessibilité
- Utilisez un thème central (`MudTheme`) pour gérer typographies, palettes et breakpoints. Ne modifiez pas les couleurs directement dans les composants ; exposez des paramètres pour accepter un `Color` ou `Variant` custom.
- Préférez les classes utilitaires (e.g. `mud-width-full`, `mud-elevation-3`) aux styles inline. Pour des customisations locales, tirez parti des CSS isolés (`.razor.css`) avec les variables CSS MudBlazor (`--mud-palette-…`).
- Respectez l'accessibilité : fournissez systématiquement `AriaLabel`, `AriaDescribedBy` sur les boutons iconiques (`MudIconButton`), et associez les labels aux inputs via `For`/`@bind-Value`.
- Configurez `MudTooltip` ou `MudPopover` pour les icônes critiques et assurez-vous que le contenu reste accessible au clavier (`FocusTrap`, `CloseOnEscape`).
- Pour les tableaux (`MudDataGrid`, `MudTable`), exposez des colonnes adaptatives et indiquez la colonne de tri actif via `aria-sort` dans le `HeaderCellClass`.

## Tests, maintenance et observabilité
- Testez les composants MudBlazor avec bUnit + `MudBlazor.Services`. Utilisez des data attributes (`data-test`) pour cibler les éléments interactifs sans dépendre des classes CSS.
- Vérifiez les scénarios d'accessibilité via Playwright ou Axe ; assurez-vous que les dialogues (`MudDialog`) sont correctement fermés via `MudDialogInstance.Close()` et qu'ils restaurent le focus.
- Instrumentez les interactions clés avec `TelemetryClient` ou des événements personnalisés pour suivre l'usage des composants critiques (ex : taux d'ouverture d'un `MudDrawer`).
- Documentez les composants personnalisés dans Storybook ou dans un styleguide interne afin de partager les conventions de découpage et les paramètres attendus.
- Ajoutez des tests de non-régression visuelle (Playwright + screenshots approuvés) pour les composants sensibles au design system, notamment les tableaux et toolbars.
- Intégrez Coverlet ou ReportGenerator pour suivre la couverture des composants critiques et exposez la tendance dans la CI.
- Définissez une politique de rotation (owner, reviewer) pour chaque composant partagé dans la documentation afin d'assurer une maintenance proactive.

## Checklist rapide
1. **Structure** : composants découpés, fragments réutilisables, logique métier isolée.
2. **Sécurité** : données validées, HTML traité, journalisation maîtrisée.
3. **Performance** : virtualisation, debounce, contrôle des re-render.
4. **UX & a11y** : thèmes centralisés, attributs ARIA, feedback cohérent.
5. **Tests & suivi** : scénarios bUnit/Playwright, instrumentation, documentation vivante.
