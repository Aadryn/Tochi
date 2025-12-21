---
description: 'Blazor component and application patterns'
applyTo: '**/*.razor,**/*.razor.cs,**/*.razor.css,**/*.razor.js'
---

# Blazor + MudBlazor Engineering Guide

## General Guidelines
- Keep **all** C# logic in code-behind (`.razor.cs`) files and expose only simple properties and method calls to templates.
- Use Blazor components idiomatically: write modern C# (C# 13) and follow .NET naming and design standards.
- Prefer inline logic only for trivial scenarios; move complex behavior into code-behind or dedicated services.
- Embrace async/await end-to-end for UI actions and data access to keep the UI responsive.
- Structure components with clear separation of concerns and leverage MudBlazor for a consistent look and feel.
- Use EventCallbacks for interactions, passing the minimal payload required.

## Component Structure
- Maintain the standard file quartet when applicable:
  - `Component.razor` – markup only.
  - `Component.razor.cs` – component logic, state, and dependency injection.
  - `Component.razor.css` – isolated styles for the component.
  - `Component.razor.js` – isolated JavaScript modules for advanced interactions.
- Use PascalCase for component file names (e.g., `UserProfileCard.razor`).
- Treat code-behind partial classes as the single source of truth for state and behavior.
- Adoptez une approche inspirée du design system : identifiez si votre composant relève d'un niveau fondation (contrôles simples réutilisables), composition (ensembles interactifs) ou fonctionnalité (cas métier complet) avant d'écrire le code. Structurez les dossiers en conséquence (`Components/Foundation`, `Components/Composition`, `Features/...`) mais gardez des noms descriptifs centrés sur l'usage (jamais `Atom`, `Molecule`, etc.).
- Lorsqu'un composant peut être réutilisé, placez-le dans un dossier partagé et fournissez un README court décrivant son rôle, ses paramètres et ses dépendances stylistiques pour faciliter l'intégration future.
- Favorisez la construction par assemblage : une page fonctionnelle doit orchestrer des composants composés qui, eux, réemploient les éléments fondation. Évitez la duplication de markup en extrayant les sections communes au niveau approprié.

## Naming Conventions
- Use PascalCase for components, methods, properties, and public members.
- Use camelCase for private fields and locals; prefix private fields with `_` only when consistent with surrounding code.
- Prefix interfaces with `I` (e.g., `IUserService`).

## Razor Templates (`.razor`)
- Keep templates declarative: include only `@page` directives and simple property/method references.
- Move all `@using` and `@inject` directives into code-behind files.
- Favor semantic HTML elements and minimize wrapper `<div>` usage.
- Bind data with `@bind` and strongly typed `For` expressions.

```razor
@page "/users/{UserId:int}"

<p>@DisplayName</p>
<p>@UserStatus</p>
<MudButton Disabled="@IsSubmitDisabled" OnClick="@HandleSaveAsync">
	@SubmitButtonText
</MudButton>

<!-- Avoid complex expressions inside the template -->
```

## Code-Behind (`.razor.cs`)
- Derive from `ComponentBase` (or relevant base component) using partial classes.
- Inject services exclusively in code-behind via `[Inject]`.
- Expose computed properties for the template (e.g., display text, loading indicators, button states).
- Use lifecycle methods (`OnInitializedAsync`, `OnParametersSetAsync`, `OnAfterRenderAsync`) to orchestrate data loading and effects.
- Dispose of unmanaged resources and JS interop handles appropriately.

```csharp
public partial class UserProfileCard : ComponentBase, IDisposable
{
	[Parameter] public User User { get; set; } = null!;
	[Parameter] public EventCallback<User> OnUserUpdated { get; set; }

	[Inject] private IUserService UserService { get; set; } = null!;
	[Inject] private NavigationManager Navigation { get; set; } = null!;

	public string DisplayName => User?.Name ?? "Unknown";
	public string UserStatus => IsUserActive ? "Active" : "Inactive";
	public bool IsSubmitDisabled => IsLoading || !IsFormValid;
	public string SubmitButtonText => IsLoading ? "Saving..." : "Save";

	protected override async Task OnInitializedAsync()
	{
		await LoadDataAsync();
	}

	public void Dispose()
	{
		Timer?.Dispose();
	}
}
```

## Dependency Injection and Services
- Inject required services with `[Inject]` and keep them private.
- Handle service failures with structured logging and user feedback (e.g., MudBlazor snackbars).
- Wrap async operations in try/catch blocks, toggling loading states in `try/finally`.

```csharp
[Inject] private IUserService UserService { get; set; } = null!;
[Inject] private ISnackbar Snackbar { get; set; } = null!;
[Inject] private ILogger<UserProfileCard> Logger { get; set; } = null!;

private async Task HandleSaveAsync()
{
	try
	{
		IsLoading = true;
		await UserService.SaveAsync(User);
		Snackbar.Add("User saved successfully", Severity.Success);
	}
	catch (Exception ex)
	{
		Logger.LogError(ex, "Error saving user {UserId}", User?.Id);
		Snackbar.Add($"Error: {ex.Message}", Severity.Error);
	}
	finally
	{
		IsLoading = false;
		StateHasChanged();
	}
}
```

## Management Endpoint Patterns
- Les pages de liste du portail management doivent hériter de `ManagementListPageBase<TViewModel>` pour bénéficier des opérations CRUD standard (grid, navigation, snackbar). Renseignez `PageTitle` via le resource manager `Culture`, définissez `EntityName`, implémentez `LoadDataAsync` pour l'accès serveur et `PerformDeleteAsync` pour la suppression sécurisée; surchargez les hooks (`OnCreateClick`, `OnEditClick`, etc.) uniquement si le flux diffère.
- Exploitez `ManagementDataGrid<TItem>` comme surface UI principale : connectez le delegate `LoadDataAsync`, exposez `SearchString`/`SearchStringChanged`, et laissez la base gérer `ReloadAsync` via la propriété protégée `DataGrid`.
- Construisez les requêtes envoyées via `IMediatorAdapter` à partir de `GridState<TViewModel>` en ajustant la pagination (`Page = state.Page + 1`, `PageSize = state.PageSize`), en journalisant les paramètres (`Logger.LogInformation`) et en convertissant les résultats via les helpers (`PromptViewModelHelper.FromPrompts`, etc.).
- Lors des commandes (validation, suppression, mise à jour), récupérez l'identité courante via `IUserService.GetCurrentUserIdOrEmpty()` pour respecter l'audit trail, puis envoyez les commandes (`UpdatePromptCommand`, `DeletePromptCommand`) au mediator.
- Enrichissez l'affichage avec `IMicrosoftGraphService` pour résoudre les informations utilisateurs manquantes (par exemple le `DisplayName`). Gérez les cas inconnus avec un fallback user-friendly afin d'éviter des valeurs techniques en UI.
- Centralisez les chaînes visibles dans `Resources/Culture` et utilisez `Culture.Format(...)` pour les messages contextualisés (snackbars succès/erreur, titres de colonnes). Ne laissez aucun texte dur en dur dans les composants.

## MudBlazor Components
- Use MudBlazor components for UI consistency and adhere to configured themes.
- Prefer MudBlazor icons (Material Icons) and consistent color/variant usage.
- Centralize theme customization in `Program.cs` or theme service.

```razor
<MudCard>
	<MudCardContent>
		<MudTextField @bind-Value="Model.Email"
					  Label="Email"
					  Variant="Variant.Outlined"
					  Required="true" />
	</MudCardContent>
	<MudCardActions>
		<MudButton Variant="Variant.Filled"
				   Color="Color.Primary"
				   StartIcon="@Icons.Material.Filled.Save"
				   OnClick="@HandleSubmitAsync">
			Submit
		</MudButton>
	</MudCardActions>
</MudCard>
```

## Styles (`.razor.css`)
- Use isolated CSS per component to avoid cross-component leakage.
- Follow kebab-case naming for CSS classes and leverage MudBlazor CSS variables.
- Avoid `!important` unless absolutely necessary.

```css
.container {
	padding: 1rem;
	border-radius: 8px;
}

.custom-button {
	background-color: var(--mud-palette-primary);
	color: var(--mud-palette-primary-text);
}
```

## JavaScript Interop (`.razor.js`)
- Author JS modules as ES6 modules and import them in `OnAfterRenderAsync`.
- Return disposable objects for cleanup and dispose of them when the component is disposed.
- Keep interop logic minimal and well-encapsulated.

```javascript
export function initializeChart(elementId, data) {
	return {
		dispose: () => {
			// Cleanup logic
		}
	};
}
```

```csharp
private IJSObjectReference? _jsModule;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
	if (firstRender)
	{
		_jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
			"import", "./Components/MyComponent.razor.js");
	}
}

public async ValueTask DisposeAsync()
{
	if (_jsModule != null)
		await _jsModule.DisposeAsync();
}
```

## Forms and Validation
- Use `EditForm` with `DataAnnotationsValidator` (or FluentValidation) for all forms.
- Keep validation models in code-behind or dedicated classes and manage form state centrally.
- Provide clear feedback for validation and submission states via MudBlazor alerts/snackbars.

```razor
<EditForm Model="@Model" OnValidSubmit="@HandleValidSubmitAsync">
	<DataAnnotationsValidator />
	<ValidationSummary />

	<MudTextField @bind-Value="Model.Name"
				  For="@(() => Model.Name)"
				  Label="Name"
				  Disabled="@IsFormDisabled" />

	<MudButton ButtonType="ButtonType.Submit"
			   Disabled="@IsSubmitDisabled">
		@SubmitButtonText
	</MudButton>
</EditForm>
```

```csharp
public partial class UserForm : ComponentBase
{
	[Parameter] public UserModel Model { get; set; } = new();

	private bool IsLoading { get; set; }
	private bool IsFormDisabled => IsLoading;
	private bool IsSubmitDisabled => IsLoading || !IsFormValid;
	private string SubmitButtonText => IsLoading ? "Submitting..." : "Submit";

	private async Task HandleValidSubmitAsync()
	{
		IsLoading = true;
		try
		{
			await ProcessFormSubmission();
		}
		finally
		{
			IsLoading = false;
			StateHasChanged();
		}
	}
}
```

## Load Lifecycle and State
- Define explicit states: NotLoaded, Loading, Loaded, and Error; expose them via read-only properties.
- Use `OnInitializedAsync` for initial fetches and `OnParametersSetAsync` when parameters change.
- Implement cancellation tokens for long-running operations (10-second timeout per retry).
- Provide retry buttons for failed loads (max 3 attempts before showing a persistent error message).
- Call `StateHasChanged()` only when state transitions require a UI update.

## State Management and Caching
- Use Cascading Parameters and EventCallbacks for simple state sharing.
- For complex scenarios, adopt Fluxor, BlazorState, or a scoped state container.
- Persist client state with `Blazored.LocalStorage`/`SessionStorage` for WebAssembly as needed.
- Cache frequently accessed data (IMemoryCache for server, distributed cache such as Redis for shared scenarios).
- Cache stable API responses to reduce redundant requests while keeping invalidation rules clear.

## Performance Optimization
- Use the `@key` directive when rendering lists to minimize DOM churn.
- Implement `ShouldRender()` to short-circuit unnecessary renders.
- Use `InvokeAsync(StateHasChanged)` when mutating state from non-UI threads or external callbacks.
- Profile and optimize API calls, ensuring they are asynchronous and cancelable.
- Identifiez vos chemins chauds (composants très utilisés) et surveillez leur temps de rendu via `EventCounters` ou Application Insights. Ajustez la granularité des composants si le diff virtuel devient coûteux.
- Préférez des structures immuables (records, `ImmutableArray`) pour faciliter les comparaisons d'état et réduire les re-render.
- Batch les mises à jour de collection (ex. `Users = Users with { Items = newList };`) au lieu de muter en boucle afin d'éviter des rendus successifs.
- Quand vous chargez de gros volumes, streammez ou paginez côté domaine et servez la donnée au fil de l'eau (ex. `IAsyncEnumerable`) afin d'abaisser l'empreinte mémoire.

```razor
@foreach (var user in Users)
{
	<UserCard @key="user.Id" User="user" />
}
```

```csharp
protected override bool ShouldRender()
{
	return _hasDataChanged;
}

private async Task HandleExternalEvent()
{
	await InvokeAsync(StateHasChanged);
}
```

## Error Handling
- Wrap error-prone sections in `<ErrorBoundary>` to display friendly messages.
- Log errors with structured logging (ILogger).
- Surface issues to users with MudBlazor alerts or snackbars and ensure remediation paths.

```razor
<ErrorBoundary>
	<ChildContent>
		@* Content that may throw errors *@
	</ChildContent>
	<ErrorContent Context="exception">
		<MudAlert Severity="Severity.Error">
			An error occurred: @exception.Message
		</MudAlert>
	</ErrorContent>
</ErrorBoundary>
```

## Accessibility and User Experience
- Add ARIA labels/attributes for assistive technologies and keep forms accessible.
- Manage loading, empty, and error states in the UI with clear visuals and messaging.
- Provide skeletons or progress indicators for long-running operations.

```razor
<MudTextField Label="User Name"
			  aria-describedby="username-help"
			  @bind-Value="Model.Name"
			  Disabled="@IsInputDisabled" />
<MudText id="username-help" Typo="Typo.caption">
	@HelpText
</MudText>

@if (IsLoading)
{
	<MudProgressCircular Indeterminate="true" />
}
else if (ShouldShowError)
{
	<MudAlert Severity="Severity.Error">@ErrorDisplayMessage</MudAlert>
}
```

## API Integration, Security, and Documentation
- Use `HttpClient` (or typed clients) with resilience policies (retry, circuit breaker) for external calls.
- Handle API failures gracefully and report actionable messages to the UI.
- Enforce authentication/authorization using ASP.NET Core Identity or JWT as appropriate.
- Always serve via HTTPS and configure CORS carefully.
- Document APIs with Swagger/OpenAPI and ensure XML comments are enabled for controllers/models.
- Appliquez systématiquement des en-têtes de sécurité (`Content-Security-Policy`, `X-Frame-Options`) via `IApplicationBuilder` et vérifiez qu'aucun composant n'introduit de dépendance externe non approuvée.
- Classez les données manipulées (publique/interne/confidentielle) et masquez-les dans les logs ou les exceptions. Pour les audits, logguez uniquement des identifiants techniques.
- Centralisez les politiques de résilience (Polly) dans un `IPolicyRegistry` partagé et utilisez-les aussi bien côté services que dans les composants via des wrappers (ex. `IMediatorAdapter.ExecuteAsync` avec retry).
- Validez les payloads entrants dans les formulaires contre des DTO dédiés et refusez les champs inattendus (`BindProperties` explicite) pour limiter les risques d'over-posting.

## Maintainability & Testability
- Documentez chaque composant partagé (`README.md`, exemples de markup, dépendances CSS/JS) et mettez à jour le changelog interne lors de modifications breaking.
- Activez les analyzers Roslyn recommandés (`EnableNETAnalyzers`, règles stylecop) et traitez les avertissements comme des erreurs dans la CI afin de préserver la qualité du code.
- Structurez vos tests en 3 couches :
	- tests unitaires (helpers, mapping, logique de base) via xUnit/MSTest,
	- tests de composants via bUnit (interactions UI, rendu conditionnel),
	- tests end-to-end (Playwright) pour les parcours critiques.
- Utilisez des `TestDataBuilders` pour générer des ViewModels cohérents et évitez la duplication de setup. Placez-les dans `Tests/Builders`.
- Ajoutez une instrumentation de couverture (Coverlet) et surveillez les métriques clés (branches critiques des handlers, composants core) avec un seuil minimum fixé dans la pipeline.

## Resilience & Observability
- Entourez les appels critiques (`MediatorAdapter`, services externes) de circuits `try/catch` spécifiques, exposez des retours de secours (fallback ViewModel vide, placeholders) et affichez des call-to-action (bouton « Réessayer »).
- Injectez `IHttpContextAccessor` ou des fournisseurs de corrélation pour propager les `CorrelationId` jusqu’aux logs du composant, facilitant le diagnostic multi-service.
- Publiez des événements fonctionnels (ex. `PromptApproved`) via `TelemetryClient.TrackEvent` avec des propriétés anonymisées afin de suivre l'adoption et détecter les anomalies.
- Lors d'intégrations signalR ou streaming, surveillez la saturation via `CircuitHandler`; si le seuil est dépassé, degradez l'UX (désactivez animations, réduisez la fréquence des refresh) pour protéger le serveur.
- Préparez des plans de reprise : scripts pour réinitialiser l'état du cache, documentation runbook et alertes connectées aux métriques (latence, erreurs 5xx, débit).

## Testing, Debugging, and Tooling
- Cover components and services with automated tests (xUnit/NUnit/MSTest) focusing on interaction and state.
- Use Visual Studio Enterprise or equivalent tooling for debugging, profiling, and diagnostics.
- Validate UI behavior using browser dev tools or Playwright integration tests where applicable.