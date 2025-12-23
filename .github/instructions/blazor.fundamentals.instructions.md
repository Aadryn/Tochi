---
description: Règles fondamentales Blazor - Lifecycle, Event Handling, Data Binding, Dependency Injection, JavaScript Interop
name: Blazor_Fundamentals
applyTo: "**/backend/Presentation/**/*.razor,**/backend/Presentation/**/*.razor.cs"
---

# Blazor - Règles Fondamentales

## � Types de Fichiers à Créer

| Type de fichier | Usage | Nomenclature |
|----------------|-------|-------------|
| `*.razor` | Composants Blazor avec markup HTML/MudBlazor | `[Feature].razor` (ex: `UserList.razor`, `LoginForm.razor`) |
| `*.razor.cs` | Code-behind des composants complexes | `[Feature].razor.cs` (même nom que le `.razor`) |
| `*.razor.css` | Styles scopés au composant | `[Feature].razor.css` (même nom que le `.razor`) |
| `*Page.razor` | Pages routables Blazor | `[Feature]Page.razor` (ex: `DashboardPage.razor`, `UsersPage.razor`) |
| `*Layout.razor` | Layouts de mise en page | `[Context]Layout.razor` (ex: `MainLayout.razor`, `AuthLayout.razor`) |
| `*Dialog.razor` | Boîtes de dialogue MudBlazor | `[Action][Entity]Dialog.razor` (ex: `CreateUserDialog.razor`) |

## ⛔ À NE PAS FAIRE

- **N'appelle jamais** `StateHasChanged()` dans `OnInitialized` ou `OnParametersSet` (déjà implicite)
- **Ne charge jamais** de données dans le constructeur (utilise `OnInitializedAsync`)
- **N'utilise jamais** `@bind` avec `@onclick` sur le même élément sans séparer les concerns
- **N'oublie jamais** de désabonner les event handlers dans `Dispose()`
- **Ne capture jamais** `this` dans les callbacks JS sans précautions
- **N'injecte jamais** de services Scoped dans des Singletons
- **N'appelle jamais** JS Interop avant `OnAfterRender` (DOM non prêt)

## ✅ À FAIRE

- **Implémente toujours** `IDisposable` pour nettoyer les ressources
- **Charge toujours** les données dans `OnInitializedAsync()`
- **Utilise toujours** `@key` pour les listes afin d'optimiser le rendu
- **Préfère toujours** `EventCallback<T>` à `Action<T>` pour les paramètres d'événements
- **Valide toujours** `firstRender` dans `OnAfterRenderAsync` pour les initialisations uniques
- **Sépare toujours** le code-behind dans un fichier `.razor.cs` pour les composants complexes
- **Utilise toujours** `@inject` ou le constructeur pour la DI, jamais les deux

## 🔄 Component Lifecycle

### Ordre d'Exécution des Méthodes

```
1. SetParametersAsync()       → Paramètres reçus du parent
   ↓
2. OnInitialized()            → Initialisation (synchrone)
   OnInitializedAsync()       → Initialisation (asynchrone)
   ↓
3. OnParametersSet()          → Paramètres appliqués (synchrone)
   OnParametersSetAsync()     → Paramètres appliqués (asynchrone)
   ↓
4. OnAfterRender()            → Rendu terminé (synchrone)
   OnAfterRenderAsync()       → Rendu terminé (asynchrone)
   ↓
5. Dispose()                  → Nettoyage des ressources
```

### Utilisation Correcte du Lifecycle

```csharp
// Component.razor.cs
public partial class MyComponent : IAsyncDisposable
{
  [Inject] private IService Service { get; set; }
  [Inject] private IJSRuntime JS { get; set; }
  
  [Parameter] public string ItemId { get; set; }
  
  private Item item;
  private bool isLoading = true;
  
  // ✅ BON : Initialisation asynchrone
  protected override async Task OnInitializedAsync()
  {
    // Charger les données initiales UNE SEULE FOIS
    await LoadDataAsync();
  }
  
  // ✅ BON : Réagir aux changements de paramètres
  protected override async Task OnParametersSetAsync()
  {
    // Recharger si ItemId change
    if (ItemId != item?.Id)
    {
      await LoadDataAsync();
    }
  }
  
  // ✅ BON : JavaScript Interop APRÈS le rendu
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      // Initialiser JavaScript (charts, maps, etc.)
      await JS.InvokeVoidAsync("initializeChart", "chart-container");
    }
  }
  
  private async Task LoadDataAsync()
  {
    isLoading = true;
    StateHasChanged(); // Forcer le rendu
    
    item = await Service.GetByIdAsync(ItemId);
    
    isLoading = false;
    StateHasChanged(); // Forcer le rendu
  }
  
  // ✅ BON : Nettoyage des ressources
  public async ValueTask DisposeAsync()
  {
    // Nettoyer les abonnements, timers, etc.
    await JS.InvokeVoidAsync("disposeChart", "chart-container");
  }
}
```

### ❌ Erreurs Courantes

```csharp
// ❌ MAUVAIS : Logique dans le constructeur
public MyComponent()
{
  // NE JAMAIS mettre de logique ici
  // Les injections ne sont pas encore disponibles
  item = Service.GetById(123); // ❌ Service est null !
}

// ❌ MAUVAIS : JavaScript Interop dans OnInitialized
protected override async Task OnInitializedAsync()
{
  // ❌ DOM pas encore rendu !
  await JS.InvokeVoidAsync("initializeChart", "chart-container");
}

// ❌ MAUVAIS : Recharger données dans OnParametersSet sans condition
protected override async Task OnParametersSetAsync()
{
  // ❌ Exécuté à chaque render, même sans changement de paramètres !
  await LoadDataAsync();
}
```

## 📊 Data Binding

### One-Way Binding

```razor
<!-- ✅ BON : Affichage simple -->
<MudText>@userName</MudText>
<MudText>@($"Total: {total:C}")</MudText>
<MudText>@DateTime.Now.ToString("dd/MM/yyyy")</MudText>

<!-- ✅ BON : Binding conditionnel -->
<MudAlert Severity="@(isSuccess ? Severity.Success : Severity.Error)">
  @message
</MudAlert>

<!-- ✅ BON : Binding d'attributs -->
<MudButton Disabled="@isLoading" Color="@buttonColor">
  @Localizer["Common.Submit"]
</MudButton>
```

### Two-Way Binding (@bind)

```razor
<!-- ✅ BON : Two-way binding simple -->
<MudTextField @bind-Value="userName" Label="@Localizer["User.Name"]"/>

<!-- ✅ BON : Two-way binding avec événement -->
<MudTextField @bind-Value="searchTerm" 
              @bind-Value:event="oninput"
              Label="@Localizer["Common.Search"]"/>

<!-- ✅ BON : Two-way binding custom -->
<MudTextField Value="@userName" 
              ValueChanged="@((string value) => HandleNameChanged(value))"
              Label="@Localizer["User.Name"]"/>
```

```csharp
// Code-behind
private string userName = string.Empty;
private string searchTerm = string.Empty;

private void HandleNameChanged(string value)
{
  userName = value;
  // Logique additionnelle
  ValidateName(value);
}
```

### Binding sur Objets Complexes

```razor
<!-- ✅ BON : Binding sur propriétés d'objet -->
<MudTextField @bind-Value="user.Name" Label="@Localizer["User.Name"]"/>
<MudTextField @bind-Value="user.Email" Label="@Localizer["User.Email"]"/>
<MudSelect @bind-Value="user.Role" Label="@Localizer["User.Role"]">
  <MudSelectItem Value="@("Admin")">Admin</MudSelectItem>
  <MudSelectItem Value="@("User")">User</MudSelectItem>
</MudSelect>
```

```csharp
// Code-behind
private UserModel user = new();

public class UserModel
{
  public string Name { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string Role { get; set; } = "User";
}
```

## 🎯 Event Handling

### Événements Standard

```razor
<!-- ✅ BON : Événements avec méthodes -->
<MudButton OnClick="@HandleClickAsync">
  @Localizer["Common.Submit"]
</MudButton>

<MudTextField @onchange="@HandleChangeAsync" 
              @oninput="@HandleInputAsync"
              @onfocus="@HandleFocus"
              @onblur="@HandleBlur"/>

<!-- ✅ BON : Événements avec lambda -->
<MudButton OnClick="@(() => DeleteAsync(item.Id))">
  @Localizer["Common.Delete"]
</MudButton>

<!-- ✅ BON : Événements avec paramètres -->
<MudButton OnClick="@(async () => await UpdateStatusAsync(item, "Approved"))">
  @Localizer["Common.Approve"]
</MudButton>
```

```csharp
// Code-behind
private async Task HandleClickAsync()
{
  await SubmitAsync();
}

private async Task HandleChangeAsync(ChangeEventArgs e)
{
  var value = e.Value?.ToString();
  await ProcessChangeAsync(value);
}

private async Task HandleInputAsync(ChangeEventArgs e)
{
  var value = e.Value?.ToString();
  searchTerm = value;
  await SearchAsync(value);
}

private void HandleFocus(FocusEventArgs e)
{
  isFocused = true;
}

private void HandleBlur(FocusEventArgs e)
{
  isFocused = false;
  ValidateField();
}
```

### Événements avec EventCallback

```razor
<!-- Composant Enfant -->
@code {
  [Parameter] public EventCallback OnSaved { get; set; }
  [Parameter] public EventCallback<string> OnSearchChanged { get; set; }
  [Parameter] public EventCallback<ItemDto> OnItemSelected { get; set; }
  
  private async Task SaveAsync()
  {
    // Logique de sauvegarde
    await OnSaved.InvokeAsync();
  }
  
  private async Task HandleSearchAsync(string term)
  {
    await OnSearchChanged.InvokeAsync(term);
  }
  
  private async Task SelectItemAsync(ItemDto item)
  {
    await OnItemSelected.InvokeAsync(item);
  }
}
```

```razor
<!-- Composant Parent -->
<ChildComponent OnSaved="@HandleSavedAsync"
                OnSearchChanged="@HandleSearchChangedAsync"
                OnItemSelected="@HandleItemSelectedAsync"/>

@code {
  private async Task HandleSavedAsync()
  {
    await ReloadDataAsync();
    Snackbar.Add("Saved successfully", Severity.Success);
  }
  
  private async Task HandleSearchChangedAsync(string term)
  {
    await SearchAsync(term);
  }
  
  private async Task HandleItemSelectedAsync(ItemDto item)
  {
    selectedItem = item;
    await LoadDetailsAsync(item.Id);
  }
}
```

### Prévenir la Propagation d'Événements

```razor
<!-- ✅ BON : Empêcher la propagation -->
<div @onclick="@HandleOuterClick">
  <MudButton @onclick="@HandleInnerClick" @onclick:stopPropagation="true">
    Click Me
  </MudButton>
</div>

<!-- ✅ BON : Empêcher le comportement par défaut -->
<form @onsubmit="@HandleSubmitAsync" @onsubmit:preventDefault="true">
  <MudButton ButtonType="ButtonType.Submit">Submit</MudButton>
</form>
```

## 💉 Dependency Injection

### Injection de Services

```csharp
// Component.razor.cs
public partial class MyComponent
{
  // ✅ BON : Property injection (RECOMMANDÉ)
  [Inject] private IPromptService PromptService { get; set; }
  [Inject] private IStringLocalizer<SharedResources> Localizer { get; set; }
  [Inject] private ISnackbar Snackbar { get; set; }
  [Inject] private NavigationManager Navigation { get; set; }
  [Inject] private IJSRuntime JS { get; set; }
  [Inject] private ILogger<MyComponent> Logger { get; set; }
  
  // ❌ MAUVAIS : Constructor injection (ne fonctionne pas bien avec Blazor)
  public MyComponent(IPromptService promptService)
  {
    // Ne pas utiliser dans les composants Blazor
  }
}
```

```razor
<!-- Alternative : Injection dans .razor -->
@inject IPromptService PromptService
@inject IStringLocalizer<SharedResources> Localizer
@inject ISnackbar Snackbar
@inject NavigationManager Navigation
```

### Scopes de Services

```csharp
// ✅ BON : Enregistrement selon le besoin
public static IServiceCollection AddApplicationServices(this IServiceCollection services)
{
  // Singleton : Instance unique pour toute l'application
  services.AddSingleton<IConfigurationService, ConfigurationService>();
  
  // Scoped : Instance unique par requête/circuit SignalR
  services.AddScoped<IPromptService, PromptService>();
  services.AddScoped<IUserContext, UserContext>();
  
  // Transient : Nouvelle instance à chaque injection
  services.AddTransient<IEmailService, EmailService>();
  services.AddTransient<INotificationService, NotificationService>();
  
  return services;
}
```

**Recommandations :**
- ✅ **Scoped** pour services métier (accès base de données, état utilisateur)
- ✅ **Singleton** pour services sans état (configuration, cache partagé)
- ✅ **Transient** pour services légers et stateless (email, notifications)

## 🔗 Component Parameters

### Déclaration de Paramètres

```csharp
// Component.razor.cs
public partial class ItemCard
{
  // ✅ BON : Paramètre obligatoire
  [Parameter, EditorRequired]
  public ItemDto Item { get; set; } = null!;
  
  // ✅ BON : Paramètre optionnel avec valeur par défaut
  [Parameter]
  public bool ShowActions { get; set; } = true;
  
  // ✅ BON : Paramètre EventCallback
  [Parameter]
  public EventCallback<string> OnEdit { get; set; }
  
  // ✅ BON : CascadingParameter (reçu d'un parent CascadingValue)
  [CascadingParameter]
  public ThemeProvider Theme { get; set; }
  
  // ❌ MAUVAIS : Pas d'attribut [Parameter]
  public string Title { get; set; } // Ne sera pas bindé depuis le parent
}
```

### Utilisation des Paramètres

```razor
<!-- Composant Parent -->
<ItemCard Item="@currentItem"
          ShowActions="true"
          OnEdit="@HandleEditAsync"/>

<ItemCard Item="@currentItem"
          ShowActions="@(!isReadOnly)"
          OnEdit="@(id => EditItemAsync(id))"/>
```

### CascadingValue et CascadingParameter

```razor
<!-- Composant Parent : Fournir une valeur cascadée -->
<CascadingValue Value="@theme">
  <ChildComponent1/>
  <ChildComponent2/>
</CascadingValue>

<CascadingValue Value="@userContext" Name="UserContext">
  <ChildComponent3/>
</CascadingValue>
```

```csharp
// Composant Enfant : Recevoir la valeur cascadée
public partial class ChildComponent1
{
  [CascadingParameter]
  private ThemeProvider Theme { get; set; }
}

public partial class ChildComponent3
{
  [CascadingParameter(Name = "UserContext")]
  private UserContext UserContext { get; set; }
}
```

## 🌐 JavaScript Interop

### Appeler JavaScript depuis C#

```csharp
// Component.razor.cs
[Inject] private IJSRuntime JS { get; set; }

// ✅ BON : Appel void (sans retour)
private async Task InitializeChartAsync()
{
  await JS.InvokeVoidAsync("initializeChart", "chart-container", chartData);
}

// ✅ BON : Appel avec retour de valeur
private async Task<bool> ConfirmDeleteAsync()
{
  return await JS.InvokeAsync<bool>("confirm", "Êtes-vous sûr ?");
}

// ✅ BON : Appel avec timeout
private async Task<string> GetUserLocationAsync()
{
  var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
  return await JS.InvokeAsync<string>("getUserLocation", cts.Token);
}

// ✅ BON : Gestion d'erreurs
private async Task CallJavaScriptAsync()
{
  try
  {
    await JS.InvokeVoidAsync("myFunction");
  }
  catch (JSException ex)
  {
    Logger.LogError(ex, "JavaScript error occurred");
    Snackbar.Add("Erreur JavaScript", Severity.Error);
  }
}
```

### JavaScript pour Blazor

```javascript
// wwwroot/js/app.js

// ✅ BON : Fonction JavaScript exposée globalement
window.initializeChart = function(containerId, data) {
  const container = document.getElementById(containerId);
  // Logique d'initialisation du chart
};

window.getUserLocation = function() {
  return new Promise((resolve, reject) => {
    navigator.geolocation.getCurrentPosition(
      position => resolve({
        latitude: position.coords.latitude,
        longitude: position.coords.longitude
      }),
      error => reject(error.message)
    );
  });
};

// ✅ BON : Module JavaScript
export function initializeMap(containerId, options) {
  const container = document.getElementById(containerId);
  // Initialiser la carte
  return {
    dispose: () => {
      // Nettoyage
    }
  };
}
```

### Appeler C# depuis JavaScript (JSInvokable)

```csharp
// Component.razor.cs
public partial class MyComponent
{
  private DotNetObjectReference<MyComponent> objRef;
  
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      objRef = DotNetObjectReference.Create(this);
      await JS.InvokeVoidAsync("registerComponent", objRef);
    }
  }
  
  // ✅ BON : Méthode invocable depuis JavaScript
  [JSInvokable]
  public async Task OnDataReceived(string data)
  {
    // Traiter les données reçues depuis JavaScript
    await ProcessDataAsync(data);
    StateHasChanged(); // Forcer le rendu
  }
  
  // ✅ BON : Méthode static invocable
  [JSInvokable("MyComponent.StaticMethod")]
  public static Task<string> GetStaticData()
  {
    return Task.FromResult("Static data");
  }
  
  public async ValueTask DisposeAsync()
  {
    await JS.InvokeVoidAsync("unregisterComponent");
    objRef?.Dispose();
  }
}
```

```javascript
// JavaScript appelant C#
let dotNetHelper;

window.registerComponent = function(helper) {
  dotNetHelper = helper;
};

window.sendDataToBlazor = async function(data) {
  // Appeler méthode d'instance
  await dotNetHelper.invokeMethodAsync('OnDataReceived', data);
  
  // Appeler méthode statique
  const result = await DotNet.invokeMethodAsync('MyAssembly', 'MyComponent.StaticMethod');
};

window.unregisterComponent = function() {
  dotNetHelper = null;
};
```

## 🔄 State Management

### État Local du Composant

```csharp
// ✅ BON : État privé du composant
private string searchTerm = string.Empty;
private List<ItemDto> items = new();
private bool isLoading = false;
private int currentPage = 1;
```

### État Partagé entre Composants (Service)

```csharp
// Services/AppState.cs
public class AppState
{
  public event Action OnChange;
  
  private string _userName = string.Empty;
  public string UserName
  {
    get => _userName;
    set
    {
      _userName = value;
      NotifyStateChanged();
    }
  }
  
  private void NotifyStateChanged() => OnChange?.Invoke();
}
```

```csharp
// Component.razor.cs
[Inject] private AppState AppState { get; set; }

protected override void OnInitialized()
{
  AppState.OnChange += StateHasChanged;
}

private void UpdateUserName(string name)
{
  AppState.UserName = name; // Notifie tous les composants abonnés
}

public void Dispose()
{
  AppState.OnChange -= StateHasChanged;
}
```

### État avec Fluxor (Redux-like)

```csharp
// States/PromptState.cs
public record PromptState
{
  public List<PromptDto> Prompts { get; init; } = new();
  public bool IsLoading { get; init; }
  public string ErrorMessage { get; init; } = string.Empty;
}

// Actions/PromptActions.cs
public record LoadPromptsAction();
public record PromptsLoadedAction(List<PromptDto> Prompts);
public record PromptsLoadFailedAction(string ErrorMessage);

// Reducers/PromptReducer.cs
public static class PromptReducer
{
  [ReducerMethod]
  public static PromptState OnLoadPrompts(PromptState state, LoadPromptsAction action)
  {
    return state with { IsLoading = true };
  }
  
  [ReducerMethod]
  public static PromptState OnPromptsLoaded(PromptState state, PromptsLoadedAction action)
  {
    return state with { Prompts = action.Prompts, IsLoading = false };
  }
}

// Component.razor.cs
[Inject] private IState<PromptState> PromptState { get; set; }
[Inject] private IDispatcher Dispatcher { get; set; }

protected override void OnInitialized()
{
  Dispatcher.Dispatch(new LoadPromptsAction());
}
```

## 📋 Checklist Blazor Fundamentals

### ✅ Lifecycle
- [ ] Initialisation dans `OnInitializedAsync()`
- [ ] Paramètres trackés dans `OnParametersSetAsync()`
- [ ] JavaScript Interop dans `OnAfterRenderAsync(firstRender)`
- [ ] Ressources nettoyées dans `DisposeAsync()`
- [ ] Pas de logique dans le constructeur

### ✅ Data Binding
- [ ] `@bind-Value` pour two-way binding
- [ ] `@bind-Value:event` pour événements custom
- [ ] Binding conditionnel avec opérateur ternaire
- [ ] Formatage de données avec interpolation

### ✅ Event Handling
- [ ] `EventCallback` au lieu de `Action`/`Func`
- [ ] `@onclick:stopPropagation` si nécessaire
- [ ] `@onsubmit:preventDefault` pour formulaires
- [ ] Gestion d'erreurs dans les handlers

### ✅ Dependency Injection
- [ ] `[Inject]` sur propriétés (property injection)
- [ ] Scope approprié (Scoped, Singleton, Transient)
- [ ] Services enregistrés dans `Program.cs`

### ✅ Component Parameters
- [ ] `[Parameter]` sur propriétés publiques
- [ ] `[EditorRequired]` pour paramètres obligatoires
- [ ] Valeurs par défaut pour paramètres optionnels
- [ ] `CascadingParameter` pour valeurs partagées

### ✅ JavaScript Interop
- [ ] `OnAfterRenderAsync(firstRender)` pour init JS
- [ ] Gestion d'erreurs avec `try/catch`
- [ ] Nettoyage des ressources JS dans `DisposeAsync()`
- [ ] `DotNetObjectReference` disposé correctement

### ✅ State Management
- [ ] État local pour composants isolés
- [ ] Services Scoped pour état partagé simple
- [ ] Fluxor/Redux pour état complexe
- [ ] Désabonnement dans `Dispose()`

## 🔍 Scripts de Validation PowerShell

```powershell
# Vérifier les constructeurs avec logique
Get-ChildItem -Recurse -Filter "*.razor.cs" | 
  Select-String -Pattern "public\s+\w+Component\s*\([^)]*\)\s*{" -Context 0,5 | 
  Where-Object { $_.Context.PostContext -match "\w+\s*=" } |
  Select-Object Path, LineNumber

# Vérifier JavaScript Interop hors OnAfterRender
Get-ChildItem -Recurse -Filter "*.razor.cs" | 
  Select-String -Pattern "JS\.InvokeAsync" | 
  Where-Object { $_.Line -notmatch "OnAfterRender" } |
  Select-Object Path, LineNumber

# Vérifier EventCallback manquants (Action/Func utilisés)
Get-ChildItem -Recurse -Filter "*.razor.cs" | 
  Select-String -Pattern "\[Parameter\].*Action<|Func<" | 
  Select-Object Path, LineNumber

# Vérifier DotNetObjectReference sans Dispose
Get-ChildItem -Recurse -Filter "*.razor.cs" | 
  Select-String -Pattern "DotNetObjectReference" | 
  ForEach-Object { 
    $file = $_.Path
    $hasDispose = Select-String -Path $file -Pattern "\.Dispose\(\)"
    if (-not $hasDispose) { 
      [PSCustomObject]@{ Path = $file; Issue = "DotNetObjectReference sans Dispose" }
    }
  }
```

## 📚 Ressources

### Documentation Officielle
- [Blazor Lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle)
- [Data Binding](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/data-binding)
- [Event Handling](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/event-handling)
- [Dependency Injection](https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/dependency-injection)
- [JavaScript Interop](https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/)

### State Management
- [Fluxor](https://github.com/mrpmorris/Fluxor) - Redux pattern for Blazor
- [Blazor State](https://github.com/TimeWarpEngineering/blazor-state)
