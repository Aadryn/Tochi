---
description: Performance Blazor - Optimisation du rendu, virtualisation, lazy loading, streaming, memoization
name: Blazor_Performance_Optimization
applyTo: "**/*.razor,**/*.razor.cs"
---

# Blazor - Optimisation des Performances

## 🚀 Optimisation du Rendu

### Contrôler les Re-renders avec ShouldRender()

```csharp
// Component.razor.cs
public partial class OptimizedComponent
{
  [Parameter] public string Title { get; set; }
  [Parameter] public List<ItemDto> Items { get; set; }
  
  private string _previousTitle;
  private int _previousItemsHash;
  
  protected override bool ShouldRender()
  {
    // ✅ BON : Rendre seulement si changements réels
    var titleChanged = Title != _previousTitle;
    var itemsChanged = Items?.GetHashCode() != _previousItemsHash;
    
    if (titleChanged || itemsChanged)
    {
      _previousTitle = Title;
      _previousItemsHash = Items?.GetHashCode() ?? 0;
      return true;
    }
    
    return false;
  }
}
```

```csharp
// ✅ BON : ShouldRender avec flag de changement
public partial class DataGridComponent
{
  private bool _dataChanged;
  
  protected override bool ShouldRender()
  {
    if (_dataChanged)
    {
      _dataChanged = false;
      return true;
    }
    return false;
  }
  
  private async Task LoadDataAsync()
  {
    items = await Service.GetAllAsync();
    _dataChanged = true; // Marquer pour re-render
    StateHasChanged();
  }
}
```

### Utiliser @key pour Stabiliser le DOM

```razor
<!-- ✅ BON : @key sur éléments de liste -->
<MudStack Spacing="2">
  @foreach (var item in items)
  {
    <ItemCard @key="item.Id" Item="@item" OnEdit="@HandleEdit"/>
  }
</MudStack>

<!-- ✅ BON : @key sur composants conditionnels -->
@if (showDetails)
{
  <DetailsPanel @key="selectedItem.Id" Item="@selectedItem"/>
}

<!-- ❌ MAUVAIS : Pas de @key sur liste dynamique -->
@foreach (var item in items)
{
  <ItemCard Item="@item"/> <!-- Blazor recrée tous les composants au changement -->
}
```

### Éviter les Re-renders Inutiles

```razor
<!-- ❌ MAUVAIS : Lambda inline recréée à chaque render -->
@foreach (var item in items)
{
  <MudButton OnClick="@(() => DeleteAsync(item.Id))">Delete</MudButton>
}

<!-- ✅ BON : Utiliser EventCallback avec paramètre -->
@foreach (var item in items)
{
  <MudButton OnClick="@(() => HandleDelete(item))">Delete</MudButton>
}

@code {
  private async Task HandleDelete(ItemDto item)
  {
    await DeleteAsync(item.Id);
  }
}
```

```csharp
// ✅ MEILLEUR : Composant dédié avec @key
<MudStack Spacing="2">
  @foreach (var item in items)
  {
    <ItemRow @key="item.Id" 
             Item="@item" 
             OnDelete="@DeleteAsync"/>
  }
</MudStack>

// ItemRow.razor.cs - Optimisé avec ShouldRender
public partial class ItemRow
{
  [Parameter] public ItemDto Item { get; set; }
  [Parameter] public EventCallback<string> OnDelete { get; set; }
  
  private ItemDto _previousItem;
  
  protected override bool ShouldRender()
  {
    if (Item != _previousItem || Item?.UpdatedAt != _previousItem?.UpdatedAt)
    {
      _previousItem = Item;
      return true;
    }
    return false;
  }
}
```

## 📜 Virtualisation pour Grandes Listes

### MudVirtualize (MudBlazor)

```razor
<!-- ✅ BON : Virtualisation pour > 100 items -->
<MudVirtualize Items="@items" Context="item" OverscanCount="5">
  <ItemTemplate>
    <MudListItem>
      <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="2">
        <MudIcon Icon="@Icons.Material.Filled.Description"/>
        <MudText>@item.Name</MudText>
      </MudStack>
    </MudListItem>
  </ItemTemplate>
</MudVirtualize>

<!-- ✅ BON : MudDataGrid avec virtualisation -->
<MudDataGrid T="PromptDto" 
             Items="@items"
             Virtualize="true"
             FixedHeader="true"
             Height="500px">
  <Columns>
    <PropertyColumn Property="x => x.Title"/>
    <PropertyColumn Property="x => x.Description"/>
  </Columns>
</MudDataGrid>
```

### Virtualize (Blazor natif)

```razor
<!-- ✅ BON : Virtualisation native Blazor -->
<div style="height: 500px; overflow-y: auto;">
  <Virtualize Items="@items" Context="item" OverscanCount="10">
    <ItemContent>
      <div class="item-row">
        <span>@item.Name</span>
        <span>@item.Description</span>
      </div>
    </ItemContent>
    <Placeholder>
      <div class="item-row loading">
        Chargement...
      </div>
    </Placeholder>
  </Virtualize>
</div>
```

### Virtualisation avec Chargement Lazy

```razor
<Virtualize ItemsProvider="@LoadItemsAsync" Context="item">
  <ItemContent>
    <ItemCard Item="@item"/>
  </ItemContent>
  <Placeholder>
    <MudProgressLinear Indeterminate="true" Color="Color.Primary"/>
  </Placeholder>
</Virtualize>

@code {
  private async ValueTask<ItemsProviderResult<ItemDto>> LoadItemsAsync(
    ItemsProviderRequest request)
  {
    // Charger seulement les items visibles
    var items = await Service.GetPagedAsync(
      skip: request.StartIndex,
      take: request.Count,
      cancellationToken: request.CancellationToken
    );
    
    var totalCount = await Service.GetCountAsync();
    
    return new ItemsProviderResult<ItemDto>(items, totalCount);
  }
}
```

## ⏱️ Debounce et Throttle

### Debounce sur Input

```razor
<MudTextField @bind-Value="searchTerm"
              Label="@Localizer["Common.Search"]"
              DebounceInterval="500"
              OnDebounceIntervalElapsed="@SearchAsync"
              Immediate="false"/>

@code {
  private string searchTerm = string.Empty;
  
  private async Task SearchAsync(string term)
  {
    // Exécuté 500ms après la dernière frappe
    await LoadResultsAsync(term);
  }
}
```

### Debounce Custom (sans MudBlazor)

```csharp
// Component.razor.cs
private System.Timers.Timer _debounceTimer;
private string searchTerm = string.Empty;

protected override void OnInitialized()
{
  _debounceTimer = new System.Timers.Timer(500);
  _debounceTimer.Elapsed += async (sender, e) => await SearchAsync();
  _debounceTimer.AutoReset = false;
}

private void OnSearchInput(ChangeEventArgs e)
{
  searchTerm = e.Value?.ToString() ?? string.Empty;
  _debounceTimer.Stop();
  _debounceTimer.Start();
}

private async Task SearchAsync()
{
  await InvokeAsync(async () =>
  {
    await LoadResultsAsync(searchTerm);
    StateHasChanged();
  });
}

public void Dispose()
{
  _debounceTimer?.Dispose();
}
```

### Throttle sur Scroll

```csharp
// Component.razor.cs
[Inject] private IJSRuntime JS { get; set; }

private DotNetObjectReference<ScrollComponent> objRef;
private DateTime _lastScrollTime = DateTime.MinValue;
private readonly TimeSpan _throttleInterval = TimeSpan.FromMilliseconds(100);

protected override async Task OnAfterRenderAsync(bool firstRender)
{
  if (firstRender)
  {
    objRef = DotNetObjectReference.Create(this);
    await JS.InvokeVoidAsync("registerScrollHandler", objRef);
  }
}

[JSInvokable]
public async Task OnScroll(int scrollTop)
{
  var now = DateTime.UtcNow;
  if (now - _lastScrollTime < _throttleInterval)
    return; // Throttle
  
  _lastScrollTime = now;
  await HandleScrollAsync(scrollTop);
}

public async ValueTask DisposeAsync()
{
  await JS.InvokeVoidAsync("unregisterScrollHandler");
  objRef?.Dispose();
}
```

## 🧩 Lazy Loading et Code Splitting

### Lazy Loading d'Assemblies

```csharp
// Program.cs
builder.Services.AddScoped(sp => new HttpClient 
{ 
  BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
});

// Router.razor
<Router AppAssembly="@typeof(App).Assembly"
        AdditionalAssemblies="@lazyLoadedAssemblies"
        OnNavigateAsync="@OnNavigateAsync">
  <Navigating>
    <MudProgressLinear Color="Color.Primary" Indeterminate="true"/>
  </Navigating>
  <Found Context="routeData">
    <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)"/>
  </Found>
</Router>

@code {
  private List<Assembly> lazyLoadedAssemblies = new();
  
  private async Task OnNavigateAsync(NavigationContext context)
  {
    if (context.Path.StartsWith("admin"))
    {
      var assemblies = await AssemblyLoader.LoadAssembliesAsync(
        new[] { "AdminModule.dll" }
      );
      lazyLoadedAssemblies.AddRange(assemblies);
    }
  }
}
```

### Lazy Loading de Composants

```razor
<!-- ✅ BON : Charger composant à la demande -->
@if (showDetails)
{
  <LazyComponent>
    <DetailsPanel Item="@selectedItem"/>
  </LazyComponent>
}

<!-- LazyComponent.razor -->
<div>
  @if (_loaded)
  {
    @ChildContent
  }
  else
  {
    <MudProgressCircular Indeterminate="true" Color="Color.Primary"/>
  }
</div>

@code {
  [Parameter] public RenderFragment ChildContent { get; set; }
  
  private bool _loaded;
  
  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (firstRender)
    {
      await Task.Delay(100); // Simuler chargement
      _loaded = true;
      StateHasChanged();
    }
  }
}
```

## 🌊 Streaming et Pagination

### Pagination Serveur (MudDataGrid)

```razor
<MudDataGrid T="PromptDto" 
             ServerData="@LoadServerDataAsync"
             Filterable="true"
             SortMode="SortMode.Multiple">
  <Columns>
    <PropertyColumn Property="x => x.Title"/>
    <PropertyColumn Property="x => x.Description"/>
  </Columns>
</MudDataGrid>

@code {
  private async Task<GridData<PromptDto>> LoadServerDataAsync(GridState<PromptDto> state)
  {
    var result = await Service.GetPagedAsync(
      page: state.Page,
      pageSize: state.PageSize,
      sortLabel: state.SortLabel,
      sortDirection: state.SortDirection,
      filter: state.FilterDefinitions
    );
    
    return new GridData<PromptDto>
    {
      Items = result.Items,
      TotalItems = result.TotalCount
    };
  }
}
```

### Streaming Render (Blazor Server)

```razor
@attribute [StreamRendering]

<h3>@title</h3>

@if (items == null)
{
  <MudProgressLinear Indeterminate="true" Color="Color.Primary"/>
}
else
{
  <MudStack Spacing="2">
    @foreach (var item in items)
    {
      <ItemCard @key="item.Id" Item="@item"/>
    }
  </MudStack>
}

@code {
  private string title = "Chargement...";
  private List<ItemDto> items;
  
  protected override async Task OnInitializedAsync()
  {
    // Premier render avec données partielles
    title = "Liste des Items";
    
    // Deuxième render avec données complètes
    items = await Service.GetAllAsync();
  }
}
```

## 💾 Memoization et Caching

### Memoization de Calculs Coûteux

```csharp
// Component.razor.cs
private Dictionary<string, string> _formatCache = new();

private string FormatExpensive(string input)
{
  if (_formatCache.TryGetValue(input, out var cached))
    return cached;
  
  // Calcul coûteux
  var result = PerformExpensiveOperation(input);
  _formatCache[input] = result;
  
  return result;
}
```

### Lazy<T> pour Initialisation Différée

```csharp
// Component.razor.cs
private Lazy<ExpensiveObject> _expensiveObject = new(() => 
  new ExpensiveObject()
);

private void UseExpensiveObject()
{
  // Créé seulement au premier accès
  var obj = _expensiveObject.Value;
  obj.DoSomething();
}
```

### Caching avec MemoryCache

```csharp
// Service avec MemoryCache
public class CachedPromptService : IPromptService
{
  private readonly IPromptService _innerService;
  private readonly IMemoryCache _cache;
  private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
  
  public CachedPromptService(IPromptService innerService, IMemoryCache cache)
  {
    _innerService = innerService;
    _cache = cache;
  }
  
  public async Task<List<PromptDto>> GetAllAsync()
  {
    const string cacheKey = "prompts_all";
    
    if (_cache.TryGetValue(cacheKey, out List<PromptDto> cached))
      return cached;
    
    var prompts = await _innerService.GetAllAsync();
    
    _cache.Set(cacheKey, prompts, _cacheDuration);
    
    return prompts;
  }
  
  public void InvalidateCache()
  {
    _cache.Remove("prompts_all");
  }
}

// Program.cs
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IPromptService, PromptService>();
builder.Services.Decorate<IPromptService, CachedPromptService>();
```

## 🔧 Optimisation du JavaScript Interop

### Batch JavaScript Calls

```csharp
// ❌ MAUVAIS : Appels JavaScript multiples
foreach (var id in itemIds)
{
  await JS.InvokeVoidAsync("highlightElement", id);
}

// ✅ BON : Appel JavaScript unique avec batch
await JS.InvokeVoidAsync("highlightElements", itemIds);
```

```javascript
// wwwroot/js/app.js

// ✅ BON : Batch processing en JavaScript
window.highlightElements = function(ids) {
  ids.forEach(id => {
    const element = document.getElementById(id);
    if (element) {
      element.classList.add('highlight');
    }
  });
};
```

### Module JavaScript avec Disposal

```csharp
// Component.razor.cs
private IJSObjectReference _jsModule;

protected override async Task OnAfterRenderAsync(bool firstRender)
{
  if (firstRender)
  {
    _jsModule = await JS.InvokeAsync<IJSObjectReference>(
      "import", "./js/chart-module.js"
    );
    
    await _jsModule.InvokeVoidAsync("initialize", "chart-container");
  }
}

public async ValueTask DisposeAsync()
{
  if (_jsModule != null)
  {
    await _jsModule.InvokeVoidAsync("dispose");
    await _jsModule.DisposeAsync();
  }
}
```

## 📊 Monitoring des Performances

### Mesurer le Temps de Rendu

```csharp
// Component.razor.cs
[Inject] private ILogger<MyComponent> Logger { get; set; }

private System.Diagnostics.Stopwatch _renderStopwatch;

protected override void OnInitialized()
{
  _renderStopwatch = System.Diagnostics.Stopwatch.StartNew();
}

protected override void OnAfterRender(bool firstRender)
{
  _renderStopwatch.Stop();
  
  if (firstRender)
  {
    Logger.LogInformation(
      "Component rendered in {ElapsedMs}ms", 
      _renderStopwatch.ElapsedMilliseconds
    );
  }
}
```

### Performance Counters Custom

```csharp
// Services/PerformanceMonitor.cs
public class PerformanceMonitor
{
  private readonly ILogger<PerformanceMonitor> _logger;
  private readonly Dictionary<string, List<long>> _metrics = new();
  
  public IDisposable Measure(string operationName)
  {
    return new PerformanceMeasurement(operationName, this);
  }
  
  internal void RecordMeasurement(string operation, long elapsedMs)
  {
    if (!_metrics.ContainsKey(operation))
      _metrics[operation] = new List<long>();
    
    _metrics[operation].Add(elapsedMs);
    
    if (_metrics[operation].Count >= 100)
    {
      var avg = _metrics[operation].Average();
      _logger.LogInformation(
        "Average time for {Operation}: {AvgMs}ms", 
        operation, avg
      );
      _metrics[operation].Clear();
    }
  }
  
  private class PerformanceMeasurement : IDisposable
  {
    private readonly string _operation;
    private readonly PerformanceMonitor _monitor;
    private readonly System.Diagnostics.Stopwatch _stopwatch;
    
    public PerformanceMeasurement(string operation, PerformanceMonitor monitor)
    {
      _operation = operation;
      _monitor = monitor;
      _stopwatch = System.Diagnostics.Stopwatch.StartNew();
    }
    
    public void Dispose()
    {
      _stopwatch.Stop();
      _monitor.RecordMeasurement(_operation, _stopwatch.ElapsedMilliseconds);
    }
  }
}
```

```csharp
// Utilisation
[Inject] private PerformanceMonitor PerfMonitor { get; set; }

private async Task LoadDataAsync()
{
  using (PerfMonitor.Measure("LoadData"))
  {
    items = await Service.GetAllAsync();
  }
}
```

## 📋 Checklist Performance

### ✅ Rendu
- [ ] `ShouldRender()` implémenté pour composants complexes
- [ ] `@key` sur listes dynamiques
- [ ] Pas de lambda inline dans boucles
- [ ] Composants dédiés pour items de liste

### ✅ Virtualisation
- [ ] `MudVirtualize` ou `<Virtualize>` pour listes > 100 items
- [ ] `OverscanCount` configuré appropriément
- [ ] Hauteur fixe sur conteneur de virtualisation
- [ ] `ItemsProvider` pour chargement lazy

### ✅ Debounce/Throttle
- [ ] Debounce 300-500ms sur inputs de recherche
- [ ] Throttle sur événements fréquents (scroll, resize)
- [ ] Timers disposés dans `Dispose()`

### ✅ Lazy Loading
- [ ] Assemblies chargés à la demande
- [ ] Composants lourds chargés conditionnellement
- [ ] Modules JavaScript importés dynamiquement

### ✅ Caching
- [ ] Résultats coûteux mis en cache
- [ ] Invalidation de cache sur modifications
- [ ] `MemoryCache` pour données partagées
- [ ] Memoization pour calculs répétitifs

### ✅ JavaScript Interop
- [ ] Appels JavaScript batchés si possible
- [ ] Modules JavaScript avec `DisposeAsync()`
- [ ] Pas d'appels JS dans boucles serrées
- [ ] `DotNetObjectReference` disposé

### ✅ Monitoring
- [ ] Temps de rendu loggés en développement
- [ ] Opérations longues (>500ms) identifiées
- [ ] Métriques de performance collectées

## 🔍 Scripts de Détection de Problèmes

```powershell
# Détecter composants sans @key dans foreach
Get-ChildItem -Recurse -Filter "*.razor" | 
  Select-String -Pattern "@foreach\s*\([^)]+\)\s*{[^@]*<\w+" | 
  Where-Object { $_.Line -notmatch "@key" } |
  Select-Object Path, LineNumber

# Détecter lambda inline dans foreach
Get-ChildItem -Recurse -Filter "*.razor" | 
  Select-String -Pattern "@foreach.*OnClick=\"@\(\(\)" | 
  Select-Object Path, LineNumber

# Détecter grandes listes sans virtualisation
Get-ChildItem -Recurse -Filter "*.razor" | 
  Select-String -Pattern "@foreach.*items" | 
  Where-Object { $_.Line -notmatch "Virtualize" } |
  Select-Object Path, LineNumber

# Détecter JS Interop dans boucles
Get-ChildItem -Recurse -Filter "*.razor.cs" | 
  Select-String -Pattern "foreach.*JS\.Invoke" | 
  Select-Object Path, LineNumber
```

## 📚 Ressources

### Documentation Officielle
- [Blazor Performance Best Practices](https://learn.microsoft.com/en-us/aspnet/core/blazor/performance)
- [Virtualization](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/virtualization)
- [Lazy Loading](https://learn.microsoft.com/en-us/aspnet/core/blazor/webassembly-lazy-load-assemblies)

### Outils
- [Blazor WebAssembly Performance Profiler](https://learn.microsoft.com/en-us/aspnet/core/blazor/webassembly-performance-best-practices)
- [Chrome DevTools Performance](https://developer.chrome.com/docs/devtools/performance/)
