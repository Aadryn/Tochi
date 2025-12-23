---
applyTo: "**/*Meter.cs"
---

- 

```csharp
public class CollectionMeter : AppMeterBase
{
  public override string MeterName => nameof(CollectionMeter);
  private readonly Counter<long> _collectionsCreated;

  public CollectionMeter(IMeterFactory meterFactory)
  {
    // IMeterFactory is provided by ASP.NET Core DI by default
    var meter = meterFactory.Create(MeterName);
    _collectionsCreated = meter.CreateCounter<long>("collection_created");
  }

  // Méthodes sans tags (compatibilité existante)
  public void RecordCollectionCreated()
  {
    _collectionsCreated.Add(1);
  }


  /// <summary>
  /// Enregistre une collection créée avec des métadonnées optionnelles.
  /// </summary>
  /// <param name="tags">Métadonnées sous forme de tuples (clé, valeur)</param>
  public void RecordCollectionCreated(params (string Key, object? Value)[] tags)
  {
    RecordCounter(_collectionsCreated, tags);
  }

}
```