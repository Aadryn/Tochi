---
description: C# performance optimization - memory management, allocations, benchmarking, profiling
name: CSharp_Performance_Optimization
applyTo: "**/*.cs"
---

# Performance et Optimisation C# - Guide Avancé

Guide complet pour optimiser les performances : gestion mémoire, allocations, structures de données, benchmarking et profiling.

## 🎯 Principes Fondamentaux (OBLIGATOIRES)

**RESPECTER ces 6 règles de performance :**

1. ✅ **MESURER avant d'optimiser** : Pas d'optimisation prématurée
   ```csharp
   // ✅ BON - Mesurer avec BenchmarkDotNet
   [MemoryDiagnoser]
   public class StringConcatenationBenchmark
   {
       [Benchmark]
       public string UsingStringConcat() { /* ... */ }
       
       [Benchmark]
       public string UsingStringBuilder() { /* ... */ }
   }
   
   // ❌ MAUVAIS - Optimiser sans mesurer
   // "StringBuilder est toujours plus rapide" (faux pour < 5 concaténations)
   ```

2. ✅ **Éviter les allocations dans les chemins critiques** : Réutiliser ou utiliser la stack
   ```csharp
   // ✅ BON - Span<T> sur stack (pas d'allocation)
   Span<byte> buffer = stackalloc byte[256];
   
   // ❌ MAUVAIS - Allocation inutile dans boucle
   for (int i = 0; i < 1000000; i++)
   {
       var temp = new byte[256];  // 1M allocations !
   }
   ```

3. ✅ **Préférer les value types pour petites données** : Éviter overhead des références
   ```csharp
   // ✅ BON - Struct pour petites données immutables
   public readonly struct Point
   {
       public int X { get; init; }
       public int Y { get; init; }
   }
   
   // ❌ MAUVAIS - Class pour données simples
   public class Point  // Allocation heap + overhead référence
   {
       public int X { get; set; }
       public int Y { get; set; }
   }
   ```

4. ✅ **Utiliser les APIs modernes performantes** : Span<T>, Memory<T>, ArrayPool
   ```csharp
   // ✅ BON - ArrayPool pour réutilisation
   var buffer = ArrayPool<byte>.Shared.Rent(1024);
   try
   {
       // Utiliser buffer
   }
   finally
   {
       ArrayPool<byte>.Shared.Return(buffer);
   }
   ```

5. ✅ **Éviter les boxing/unboxing** : Coûteux en allocations
   ```csharp
   // ❌ MAUVAIS - Boxing
   int value = 42;
   object boxed = value;  // Allocation heap
   
   // ✅ BON - Utiliser génériques
   void Process<T>(T value) where T : struct { }
   ```

6. ✅ **Optimiser les boucles critiques** : Éliminer travail inutile
   ```csharp
   // ✅ BON - Calculer longueur une fois
   int length = array.Length;
   for (int i = 0; i < length; i++)
   {
       Process(array[i]);
   }
   
   // ❌ MAUVAIS - Recalcule à chaque itération
   for (int i = 0; i < GetExpensiveLength(); i++)
   {
       Process(array[i]);
   }
   ```

## 🧮 Gestion Mémoire et Allocations

### Span<T> et Memory<T> - Zero-Copy

```csharp
// ✅ BON - Span<T> pour manipulation sans allocation
public static bool IsValidEmail(ReadOnlySpan<char> email)
{
    int atIndex = email.IndexOf('@');
    if (atIndex <= 0 || atIndex == email.Length - 1)
        return false;
    
    ReadOnlySpan<char> localPart = email[..atIndex];
    ReadOnlySpan<char> domain = email[(atIndex + 1)..];
    
    return localPart.Length > 0 && domain.Contains('.');
}

// Utilisation - pas d'allocation
string email = "user@example.com";
bool isValid = IsValidEmail(email.AsSpan());  // Pas de substring

// ❌ MAUVAIS - Allocations avec substring
public static bool IsValidEmail(string email)
{
    int atIndex = email.IndexOf('@');
    if (atIndex <= 0) return false;
    
    string localPart = email.Substring(0, atIndex);  // Allocation
    string domain = email.Substring(atIndex + 1);     // Allocation
    
    return localPart.Length > 0 && domain.Contains('.');
}

// ✅ BON - Memory<T> pour données asynchrones
public async Task<int> ProcessDataAsync(Memory<byte> data, CancellationToken cancellationToken)
{
    await Task.Delay(100, cancellationToken);
    
    // Memory<T> peut être utilisé dans async (Span<T> ne peut pas)
    return data.Length;
}

// ✅ BON - stackalloc pour petits buffers
public string ToHexString(ReadOnlySpan<byte> bytes)
{
    Span<char> chars = stackalloc char[bytes.Length * 2];
    
    for (int i = 0; i < bytes.Length; i++)
    {
        bytes[i].TryFormat(chars[(i * 2)..], out _, "X2");
    }
    
    return new string(chars);
}
```

### ArrayPool - Réutilisation de Buffers

```csharp
// ✅ BON - ArrayPool pour buffers temporaires
public async Task<byte[]> CompressDataAsync(byte[] data, CancellationToken cancellationToken)
{
    // Louer buffer du pool (réutilisé)
    var buffer = ArrayPool<byte>.Shared.Rent(data.Length * 2);
    try
    {
        using var memoryStream = new MemoryStream(buffer);
        using var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress);
        
        await gzipStream.WriteAsync(data, cancellationToken);
        await gzipStream.FlushAsync(cancellationToken);
        
        return memoryStream.ToArray();
    }
    finally
    {
        // Retourner au pool
        ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
    }
}

// ❌ MAUVAIS - Allocation à chaque appel
public async Task<byte[]> CompressDataAsync(byte[] data)
{
    var buffer = new byte[data.Length * 2];  // Allocation
    // ...
}

// ✅ BON - MemoryPool<T> pour Memory<T>
public class DataProcessor : IDisposable
{
    private readonly MemoryPool<byte> _memoryPool = MemoryPool<byte>.Shared;
    
    public async Task ProcessAsync(Stream stream, CancellationToken cancellationToken)
    {
        using IMemoryOwner<byte> owner = _memoryPool.Rent(4096);
        Memory<byte> buffer = owner.Memory;
        
        int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
        ProcessBuffer(buffer[..bytesRead].Span);
    }
    
    public void Dispose()
    {
        // Cleanup si nécessaire
    }
}
```

### Object Pooling - Réutilisation d'Objets

```csharp
// ✅ BON - ObjectPool pour objets coûteux
public class ExpensiveObjectPool
{
    private readonly ConcurrentBag<ExpensiveObject> _pool = new();
    private readonly Func<ExpensiveObject> _factory;
    
    public ExpensiveObjectPool(Func<ExpensiveObject> factory)
    {
        _factory = factory;
    }
    
    public ExpensiveObject Rent()
    {
        if (_pool.TryTake(out var obj))
        {
            return obj;
        }
        
        return _factory();
    }
    
    public void Return(ExpensiveObject obj)
    {
        obj.Reset();  // Nettoyer l'état
        _pool.Add(obj);
    }
}

// Utilisation
public class ExpensiveObjectPoolPolicy : IPooledObjectPolicy<ExpensiveObject>
{
    public ExpensiveObject Create() => new ExpensiveObject();
    
    public bool Return(ExpensiveObject obj)
    {
        obj.Reset();
        return true;
    }
}

// Avec Microsoft.Extensions.ObjectPool
private readonly ObjectPool<StringBuilder> _stringBuilderPool = 
    new DefaultObjectPoolProvider().CreateStringBuilderPool();

public string BuildComplexString(IEnumerable<string> parts)
{
    var sb = _stringBuilderPool.Get();
    try
    {
        foreach (var part in parts)
        {
            sb.AppendLine(part);
        }
        return sb.ToString();
    }
    finally
    {
        _stringBuilderPool.Return(sb);
    }
}
```

### Struct vs Class - Choix Performant

```csharp
// ✅ BON - Struct pour petites données immutables (< 16 bytes)
public readonly struct Point2D
{
    public int X { get; init; }  // 4 bytes
    public int Y { get; init; }  // 4 bytes
    // Total: 8 bytes - parfait pour struct
}

// ✅ BON - Struct avec Equals optimisé
public readonly struct Point2D : IEquatable<Point2D>
{
    public int X { get; init; }
    public int Y { get; init; }
    
    public bool Equals(Point2D other)
    {
        return X == other.X && Y == other.Y;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is Point2D other && Equals(other);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}

// ❌ MAUVAIS - Struct trop grand
public struct LargeData  // 1000 bytes - trop gros pour struct
{
    public byte[] Data;  // Référence vers array
    // Copying ce struct copie la référence, pas les données !
}

// ✅ BON - Class pour données mutables ou grandes
public class UserProfile  // Mutable et grande
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<Order> Orders { get; set; }
    // Total > 16 bytes - utiliser class
}

// ✅ BON - ref struct pour stack uniquement
public ref struct StackOnlyBuffer
{
    private Span<byte> _buffer;
    
    public StackOnlyBuffer(Span<byte> buffer)
    {
        _buffer = buffer;
    }
    
    // Ne peut pas être boxé, pas sur heap
    // Parfait pour buffers temporaires
}
```

## 🚀 Optimisations Collections

### Choix de la Collection Appropriée

```csharp
// ✅ BON - List<T> pour accès indexé fréquent
var users = new List<User>(capacity: 1000);  // Préallouer si taille connue

// ✅ BON - Dictionary<TKey, TValue> pour lookup par clé
var userById = new Dictionary<Guid, User>(capacity: 1000);

// ✅ BON - HashSet<T> pour unicité et contains
var processedIds = new HashSet<Guid>();

// ✅ BON - SortedSet<T> pour collection triée
var sortedScores = new SortedSet<int>();

// ✅ BON - Queue<T> pour FIFO
var taskQueue = new Queue<Task>();

// ✅ BON - Stack<T> pour LIFO
var operationStack = new Stack<Operation>();

// ✅ BON - LinkedList<T> pour insertions/suppressions fréquentes au milieu
var recentItems = new LinkedList<Item>();

// ❌ MAUVAIS - List.Contains en boucle (O(n²))
var users = new List<User>();
foreach (var user in allUsers)
{
    if (!users.Contains(user))  // O(n) à chaque itération
    {
        users.Add(user);
    }
}

// ✅ BON - HashSet.Contains (O(1))
var users = new HashSet<User>();
foreach (var user in allUsers)
{
    users.Add(user);  // Doublon automatiquement ignoré
}
```

### Éviter les Allocations LINQ

```csharp
// ❌ MAUVAIS - Multiples allocations LINQ
var result = users
    .Where(u => u.IsActive)      // Allocation énumérateur
    .Select(u => u.Name)         // Allocation énumérateur
    .OrderBy(n => n)             // Allocation array + tri
    .Take(10)                    // Allocation énumérateur
    .ToList();                   // Allocation List

// ✅ BON - Boucle manuelle pour hot path
var result = new List<User>(capacity: 10);
foreach (var user in users)
{
    if (user.IsActive)
    {
        result.Add(user);
        if (result.Count >= 10)
            break;
    }
}
result.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

// ✅ BON - LINQ OK si pas dans hot path
// Code plus lisible si performance non critique
var inactiveUsers = users.Where(u => !u.IsActive).ToList();

// ✅ BON - Éviter ToList() inutile
public IEnumerable<User> GetActiveUsers()
{
    return _context.Users.Where(u => u.IsActive);  // Exécution différée
}

// ❌ MAUVAIS - ToList() prématuré
public IEnumerable<User> GetActiveUsers()
{
    return _context.Users.Where(u => u.IsActive).ToList();  // Matérialise tout
}

// ✅ BON - Count() au lieu de Count
if (users.Any())  // ✅ S'arrête au premier élément
{
    // ...
}

// ❌ MAUVAIS
if (users.Count() > 0)  // ❌ Énumère tous les éléments
{
    // ...
}
```

### Capacity et Preallocation

```csharp
// ✅ BON - Préallouer si taille connue
var users = new List<User>(capacity: expectedCount);
var userDict = new Dictionary<Guid, User>(capacity: expectedCount);

// ❌ MAUVAIS - Pas de capacity, réallocations multiples
var users = new List<User>();  // Réallocations: 4 -> 8 -> 16 -> 32...
for (int i = 0; i < 1000; i++)
{
    users.Add(new User());  // Déclenche réallocation plusieurs fois
}

// ✅ BON - EnsureCapacity pour éviter réallocations
var users = new List<User>();
users.EnsureCapacity(1000);
for (int i = 0; i < 1000; i++)
{
    users.Add(new User());  // Pas de réallocation
}

// ✅ BON - CollectionsMarshal pour accès direct
using System.Runtime.InteropServices;

var list = new List<int> { 1, 2, 3, 4, 5 };
Span<int> span = CollectionsMarshal.AsSpan(list);

for (int i = 0; i < span.Length; i++)
{
    span[i] *= 2;  // Modification directe, pas de bounds check répété
}
```

## ⚡ String Performance

### StringBuilder vs Interpolation

```csharp
// ✅ BON - String interpolation pour < 5 concaténations
var message = $"User {user.Name} (ID: {user.Id}) logged in at {DateTime.Now:HH:mm}";

// ✅ BON - StringBuilder pour boucles
public string BuildReport(IEnumerable<Order> orders)
{
    var sb = new StringBuilder(capacity: orders.Count() * 50);  // Estimer taille
    
    sb.AppendLine("Order Report");
    sb.AppendLine("=============");
    
    foreach (var order in orders)
    {
        sb.AppendLine($"Order {order.Id}: {order.Total:C}");
    }
    
    return sb.ToString();
}

// ❌ MAUVAIS - Concaténation en boucle
public string BuildReport(IEnumerable<Order> orders)
{
    string report = "Order Report\n=============\n";
    foreach (var order in orders)
    {
        report += $"Order {order.Id}: {order.Total:C}\n";  // Allocation à chaque +=
    }
    return report;
}

// ✅ BON - String.Create pour construction optimisée
public static string ToHexString(ReadOnlySpan<byte> bytes)
{
    return string.Create(bytes.Length * 2, bytes, (chars, bytes) =>
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i].TryFormat(chars[(i * 2)..], out _, "X2");
        }
    });
}

// ✅ BON - ZString pour zero-allocation (bibliothèque externe)
// PM> Install-Package ZString
using var sb = ZString.CreateStringBuilder();
sb.Append("User: ");
sb.Append(user.Name);
sb.Append(", ID: ");
sb.Append(user.Id);
return sb.ToString();
```

### String Comparison Performance

```csharp
// ✅ BON - Ordinal pour comparaisons rapides
if (string.Equals(str1, str2, StringComparison.Ordinal))
{
    // Plus rapide que culture-aware
}

// ✅ BON - OrdinalIgnoreCase pour insensible à la casse
if (string.Equals(email1, email2, StringComparison.OrdinalIgnoreCase))
{
    // Pas de lowercase inutile
}

// ❌ MAUVAIS - ToLower pour comparaison
if (email1.ToLower() == email2.ToLower())  // 2 allocations !
{
    // ...
}

// ✅ BON - AsSpan pour éviter allocations
if (str.AsSpan().StartsWith("http://", StringComparison.Ordinal))
{
    // Pas de substring
}

// ❌ MAUVAIS - Substring pour vérification
if (str.Substring(0, 7) == "http://")  // Allocation substring
{
    // ...
}

// ✅ BON - String interning pour strings répétées
private static readonly string CachedValue = string.Intern("CommonValue");

// Réutilise la même instance en mémoire
```

## 🔢 Calculs et Algorithmes

### Éviter les Divisions Coûteuses

```csharp
// ✅ BON - Multiplication au lieu de division
int result = value * 10;  // Rapide

// ❌ LENT - Division
int result = value / 0.1;  // Plus lent

// ✅ BON - Shift bits au lieu de *2 ou /2
int doubled = value << 1;   // value * 2
int halved = value >> 1;    // value / 2

// ✅ BON - Modulo avec puissance de 2 -> AND
int capacity = 16;  // Puissance de 2
int index = hash & (capacity - 1);  // Équivalent à hash % 16, mais plus rapide

// ❌ LENT - Modulo
int index = hash % capacity;
```

### Éviter les Branches (Branch Prediction)

```csharp
// ✅ BON - Branchless pour conditions simples
int max = (a > b) ? a : b;
int absValue = (value < 0) ? -value : value;

// Ou avec Math
int max = Math.Max(a, b);
int absValue = Math.Abs(value);

// ✅ BON - Lookup table au lieu de if/else
private static readonly int[] MultiplierTable = { 1, 2, 4, 8, 16, 32, 64, 128 };

public int GetMultiplier(int level)
{
    return MultiplierTable[level];  // Pas de branches
}

// ❌ LENT - Multiples branches
public int GetMultiplier(int level)
{
    if (level == 0) return 1;
    if (level == 1) return 2;
    if (level == 2) return 4;
    // ...
}
```

### SIMD - Vectorisation

```csharp
using System.Numerics;

// ✅ BON - SIMD pour opérations sur arrays
public static void AddArrays(Span<float> left, ReadOnlySpan<float> right)
{
    int vectorSize = Vector<float>.Count;
    int i = 0;
    
    // Traiter par vecteurs (4 ou 8 floats à la fois)
    for (; i <= left.Length - vectorSize; i += vectorSize)
    {
        var leftVector = new Vector<float>(left[i..]);
        var rightVector = new Vector<float>(right[i..]);
        var result = leftVector + rightVector;
        result.CopyTo(left[i..]);
    }
    
    // Traiter les éléments restants
    for (; i < left.Length; i++)
    {
        left[i] += right[i];
    }
}

// ❌ LENT - Boucle classique
public static void AddArrays(float[] left, float[] right)
{
    for (int i = 0; i < left.Length; i++)
    {
        left[i] += right[i];  // Un par un
    }
}

// ✅ BON - Vector<T> pour calculs
public static float DotProduct(ReadOnlySpan<float> left, ReadOnlySpan<float> right)
{
    int vectorSize = Vector<float>.Count;
    var sumVector = Vector<float>.Zero;
    int i = 0;
    
    for (; i <= left.Length - vectorSize; i += vectorSize)
    {
        var leftVector = new Vector<float>(left[i..]);
        var rightVector = new Vector<float>(right[i..]);
        sumVector += leftVector * rightVector;
    }
    
    float sum = Vector.Dot(sumVector, Vector<float>.One);
    
    // Éléments restants
    for (; i < left.Length; i++)
    {
        sum += left[i] * right[i];
    }
    
    return sum;
}
```

## 🗄️ I/O et Sérialisation

### File I/O Performance

```csharp
// ✅ BON - Buffered I/O avec FileOptions
public async Task<string> ReadFileOptimizedAsync(string path, CancellationToken cancellationToken)
{
    const FileOptions options = FileOptions.Asynchronous | FileOptions.SequentialScan;
    
    await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 
        bufferSize: 4096, options);
    using var reader = new StreamReader(stream);
    
    return await reader.ReadToEndAsync(cancellationToken);
}

// ✅ BON - Memory-mapped files pour gros fichiers
public void ProcessLargeFile(string path)
{
    using var mmf = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
    using var accessor = mmf.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
    
    long position = 0;
    while (position < accessor.Capacity)
    {
        byte value = accessor.ReadByte(position);
        ProcessByte(value);
        position++;
    }
}

// ✅ BON - PipeReader pour streaming efficace
public async Task ProcessStreamAsync(Stream stream, CancellationToken cancellationToken)
{
    var reader = PipeReader.Create(stream);
    
    while (true)
    {
        ReadResult result = await reader.ReadAsync(cancellationToken);
        ReadOnlySequence<byte> buffer = result.Buffer;
        
        ProcessBuffer(buffer);
        
        reader.AdvanceTo(buffer.End);
        
        if (result.IsCompleted)
            break;
    }
    
    await reader.CompleteAsync();
}
```

### Sérialisation Performance

```csharp
// ✅ BON - System.Text.Json avec options optimisées
private static readonly JsonSerializerOptions JsonOptions = new()
{
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false  // Compact
};

public string SerializeUser(User user)
{
    return JsonSerializer.Serialize(user, JsonOptions);
}

// ✅ BON - Utf8JsonWriter pour contrôle total
public byte[] SerializeUsers(IEnumerable<User> users)
{
    using var stream = new MemoryStream();
    using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = false });
    
    writer.WriteStartArray();
    foreach (var user in users)
    {
        writer.WriteStartObject();
        writer.WriteString("id", user.Id.ToString());
        writer.WriteString("name", user.Name);
        writer.WriteEndObject();
    }
    writer.WriteEndArray();
    
    writer.Flush();
    return stream.ToArray();
}

// ✅ BON - MemoryPack pour sérialisation binaire ultra-rapide
// PM> Install-Package MemoryPack
[MemoryPackable]
public partial class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

var bytes = MemoryPackSerializer.Serialize(user);
var deserialized = MemoryPackSerializer.Deserialize<User>(bytes);

// ✅ BON - MessagePack pour sérialisation compacte
// PM> Install-Package MessagePack
[MessagePackObject]
public class User
{
    [Key(0)]
    public Guid Id { get; set; }
    
    [Key(1)]
    public string Name { get; set; }
}

var bytes = MessagePackSerializer.Serialize(user);
var deserialized = MessagePackSerializer.Deserialize<User>(bytes);
```

## 📊 Benchmarking avec BenchmarkDotNet

### Configuration Benchmark

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

[MemoryDiagnoser]  // Mesure allocations
[ThreadingDiagnoser]  // Mesure contentions threads
public class StringBenchmarks
{
    private const int Iterations = 1000;
    
    [Benchmark(Baseline = true)]
    public string ConcatenationBaseline()
    {
        string result = "";
        for (int i = 0; i < Iterations; i++)
        {
            result += i.ToString();
        }
        return result;
    }
    
    [Benchmark]
    public string StringBuilderOptimized()
    {
        var sb = new StringBuilder(capacity: Iterations * 4);
        for (int i = 0; i < Iterations; i++)
        {
            sb.Append(i);
        }
        return sb.ToString();
    }
    
    [Benchmark]
    public string StringCreateOptimized()
    {
        return string.Create(Iterations * 4, Iterations, (chars, iterations) =>
        {
            int pos = 0;
            for (int i = 0; i < iterations; i++)
            {
                i.TryFormat(chars[pos..], out int written);
                pos += written;
            }
        });
    }
}

// Exécuter
public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<StringBenchmarks>();
    }
}
```

### Paramètres et Scénarios

```csharp
[MemoryDiagnoser]
public class CollectionBenchmarks
{
    [Params(10, 100, 1000, 10000)]
    public int Size { get; set; }
    
    private List<int> _list;
    private HashSet<int> _hashSet;
    
    [GlobalSetup]
    public void Setup()
    {
        _list = Enumerable.Range(0, Size).ToList();
        _hashSet = new HashSet<int>(_list);
    }
    
    [Benchmark]
    public bool List_Contains()
    {
        return _list.Contains(Size / 2);  // O(n)
    }
    
    [Benchmark]
    public bool HashSet_Contains()
    {
        return _hashSet.Contains(Size / 2);  // O(1)
    }
}

// Résultats typiques:
// Size=10    : List ~20ns,   HashSet ~15ns   (List plus rapide!)
// Size=100   : List ~150ns,  HashSet ~15ns
// Size=1000  : List ~1500ns, HashSet ~15ns   (HashSet 100x plus rapide)
// Size=10000 : List ~15μs,   HashSet ~15ns   (HashSet 1000x plus rapide)
```

### Mesurer Allocations

```csharp
[MemoryDiagnoser]
public class AllocationBenchmarks
{
    [Benchmark(Baseline = true)]
    public string SubstringAllocation()
    {
        string text = "Hello, World!";
        return text.Substring(0, 5);  // Allocation
    }
    
    [Benchmark]
    public string SpanNoAllocation()
    {
        string text = "Hello, World!";
        return text.AsSpan()[..5].ToString();  // Moins d'allocations
    }
    
    [Benchmark]
    public void ArrayNewAllocation()
    {
        var array = new byte[1024];  // Allocation
        ProcessArray(array);
    }
    
    [Benchmark]
    public void ArrayPoolNoAllocation()
    {
        var array = ArrayPool<byte>.Shared.Rent(1024);
        try
        {
            ProcessArray(array);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }
}

// Résultats typiques:
// SubstringAllocation    : 30ns, 40 bytes allocated
// SpanNoAllocation       : 25ns, 32 bytes allocated
// ArrayNewAllocation     : 80ns, 1024 bytes allocated
// ArrayPoolNoAllocation  : 15ns, 0 bytes allocated (réutilisation)
```

## 🔍 Profiling et Diagnostic

### dotnet-counters - Métriques en Temps Réel

```bash
# Installer
dotnet tool install --global dotnet-counters

# Lister processus
dotnet-counters ps

# Monitorer métriques
dotnet-counters monitor --process-id <PID> System.Runtime

# Métriques personnalisées
dotnet-counters monitor --process-id <PID> MyApp.Metrics

# Export vers fichier
dotnet-counters collect --process-id <PID> --format json -o metrics.json
```

### dotnet-trace - Profiling Performance

```bash
# Installer
dotnet tool install --global dotnet-trace

# Collecter trace
dotnet-trace collect --process-id <PID> --providers Microsoft-Windows-DotNETRuntime

# Analyser avec PerfView
dotnet-trace collect --process-id <PID> --profile cpu-sampling
```

### Code avec Métriques Personnalisées

```csharp
using System.Diagnostics;
using System.Diagnostics.Metrics;

public class UserService
{
    private static readonly Meter Meter = new("MyApp.UserService", "1.0.0");
    
    private static readonly Counter<long> UserCreatedCounter = 
        Meter.CreateCounter<long>("users.created", description: "Number of users created");
    
    private static readonly Histogram<double> UserCreationDuration = 
        Meter.CreateHistogram<double>("users.creation.duration", unit: "ms", 
            description: "Duration of user creation");
    
    public async Task<User> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        
        try
        {
            var user = await CreateUserInternalAsync(request, cancellationToken);
            
            UserCreatedCounter.Add(1, 
                new KeyValuePair<string, object?>("status", "success"));
            
            return user;
        }
        catch (Exception)
        {
            UserCreatedCounter.Add(1, 
                new KeyValuePair<string, object?>("status", "error"));
            throw;
        }
        finally
        {
            UserCreationDuration.Record(sw.Elapsed.TotalMilliseconds);
        }
    }
}

// Exporter vers Prometheus, OpenTelemetry, etc.
```

### Activity et Distributed Tracing

```csharp
using System.Diagnostics;

public class OrderService
{
    private static readonly ActivitySource ActivitySource = new("MyApp.OrderService");
    
    public async Task<Order> ProcessOrderAsync(OrderRequest request, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("ProcessOrder");
        activity?.SetTag("order.id", request.Id);
        activity?.SetTag("order.amount", request.TotalAmount);
        
        try
        {
            var order = await CreateOrderAsync(request, cancellationToken);
            
            using var paymentActivity = ActivitySource.StartActivity("ProcessPayment");
            paymentActivity?.SetTag("payment.method", request.PaymentMethod);
            
            await ProcessPaymentAsync(order, cancellationToken);
            
            activity?.SetTag("order.status", "completed");
            return order;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }
}

// Configuration dans Program.cs
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddSource("MyApp.OrderService")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());
```

## 🎯 Optimisations Spécifiques

### Database Access - EF Core

```csharp
// ✅ BON - AsNoTracking pour queries read-only
var users = await _context.Users
    .AsNoTracking()  // Pas de change tracking
    .Where(u => u.IsActive)
    .ToListAsync(cancellationToken);

// ✅ BON - Projection pour éviter charger entités complètes
var userNames = await _context.Users
    .Where(u => u.IsActive)
    .Select(u => new { u.Id, u.Name })  // Seulement 2 colonnes
    .ToListAsync(cancellationToken);

// ✅ BON - Split query pour éviter cartesian explosion
var users = await _context.Users
    .Include(u => u.Orders)
    .Include(u => u.Addresses)
    .AsSplitQuery()  // 3 requêtes au lieu d'1 avec JOIN multiple
    .ToListAsync(cancellationToken);

// ✅ BON - Compiled queries pour requêtes fréquentes
private static readonly Func<AppDbContext, Guid, Task<User?>> GetUserByIdQuery =
    EF.CompileAsyncQuery((AppDbContext context, Guid id) =>
        context.Users.FirstOrDefault(u => u.Id == id));

public async Task<User?> GetUserAsync(Guid id)
{
    return await GetUserByIdQuery(_context, id);
}

// ✅ BON - Batch updates avec ExecuteUpdateAsync (EF Core 7+)
await _context.Users
    .Where(u => u.LastLoginDate < DateTime.UtcNow.AddYears(-1))
    .ExecuteUpdateAsync(setters => setters
        .SetProperty(u => u.IsActive, false),
        cancellationToken);
// Une seule requête SQL UPDATE au lieu de charger + modifier + save
```

### HTTP Client Performance

```csharp
// ✅ BON - IHttpClientFactory avec pooling
public class UserApiService
{
    private readonly HttpClient _httpClient;
    
    public UserApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("UserApi");
    }
    
    public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/users/{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<User>(cancellationToken);
    }
}

// Configuration
builder.Services.AddHttpClient("UserApi", client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(5),
    PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
    MaxConnectionsPerServer = 10
});

// ✅ BON - HTTP/2 ou HTTP/3 pour multiplexing
builder.Services.AddHttpClient("UserApi")
    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
    {
        EnableMultipleHttp2Connections = true
    });
```

### Caching Strategies

```csharp
// ✅ BON - MemoryCache avec options
public class CachedUserService
{
    private readonly IMemoryCache _cache;
    private readonly IUserRepository _repository;
    
    public async Task<User> GetUserAsync(Guid id, CancellationToken cancellationToken)
    {
        string cacheKey = $"user:{id}";
        
        if (_cache.TryGetValue(cacheKey, out User? cachedUser))
        {
            return cachedUser!;
        }
        
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
            .SetSlidingExpiration(TimeSpan.FromMinutes(2))
            .SetSize(1)  // Pour limiter taille cache
            .RegisterPostEvictionCallback((key, value, reason, state) =>
            {
                _logger.LogDebug("Cache evicted: {Key}, Reason: {Reason}", key, reason);
            });
        
        _cache.Set(cacheKey, user, cacheOptions);
        
        return user;
    }
}

// ✅ BON - Distributed cache avec Redis
public class DistributedCachedUserService
{
    private readonly IDistributedCache _cache;
    
    public async Task<User?> GetUserAsync(Guid id, CancellationToken cancellationToken)
    {
        string cacheKey = $"user:{id}";
        
        var cachedBytes = await _cache.GetAsync(cacheKey, cancellationToken);
        if (cachedBytes is not null)
        {
            return JsonSerializer.Deserialize<User>(cachedBytes);
        }
        
        var user = await _repository.GetByIdAsync(id, cancellationToken);
        if (user is not null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            };
            
            var bytes = JsonSerializer.SerializeToUtf8Bytes(user);
            await _cache.SetAsync(cacheKey, bytes, options, cancellationToken);
        }
        
        return user;
    }
}
```

## ✅ Checklist Performance

Avant d'optimiser, VÉRIFIER :

### Mesures et Profiling
- [ ] Benchmark avec BenchmarkDotNet pour comparer approches
- [ ] Profiler avec dotnet-trace pour identifier hotspots
- [ ] Mesurer allocations avec dotnet-counters
- [ ] Établir baseline de performance

### Allocations Mémoire
- [ ] Utiliser Span<T>/Memory<T> pour éviter allocations
- [ ] ArrayPool pour buffers temporaires
- [ ] Object pooling pour objets coûteux
- [ ] Structs pour petites données immutables (< 16 bytes)
- [ ] Pas de boxing/unboxing dans hot paths

### Collections
- [ ] Capacity préallouée si taille connue
- [ ] Collection appropriée (List vs HashSet vs Dictionary)
- [ ] Éviter ToList() inutile dans LINQ
- [ ] Any() au lieu de Count() > 0

### Strings
- [ ] StringBuilder pour concaténations en boucle
- [ ] String.Create pour constructions complexes
- [ ] Ordinal comparison au lieu de ToLower()
- [ ] AsSpan() pour éviter substring

### Algorithmes
- [ ] Éliminer branches inutiles (branchless)
- [ ] Lookup tables pour if/else multiples
- [ ] SIMD pour opérations vectorielles
- [ ] Éviter divisions coûteuses

### I/O et Réseau
- [ ] Async I/O avec buffers appropriés
- [ ] IHttpClientFactory avec pooling
- [ ] Compression pour gros payloads
- [ ] Batching pour multiples opérations

### Database
- [ ] AsNoTracking pour queries read-only
- [ ] Projections pour colonnes spécifiques
- [ ] Compiled queries pour requêtes fréquentes
- [ ] Batch operations avec ExecuteUpdateAsync

### Caching
- [ ] Cache pour données fréquemment lues
- [ ] Expiration appropriée (absolute + sliding)
- [ ] Invalidation correcte
- [ ] Distributed cache si multi-instance

## 📚 Ressources

### Documentation Officielle
- [Performance Tips - Microsoft](https://learn.microsoft.com/en-us/dotnet/framework/performance/)
- [Span<T> and Memory<T>](https://learn.microsoft.com/en-us/dotnet/standard/memory-and-spans/)
- [System.Buffers](https://learn.microsoft.com/en-us/dotnet/api/system.buffers)
- [SIMD in .NET](https://learn.microsoft.com/en-us/dotnet/standard/simd)

### Outils
- [BenchmarkDotNet](https://benchmarkdotnet.org/)
- [dotnet-counters](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-counters)
- [dotnet-trace](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/dotnet-trace)
- [PerfView](https://github.com/microsoft/perfview)

### Blogs et Articles
- **Nick Chapsas** - .NET Performance Tips
- **Stephen Toub** - Performance Improvements in .NET
- **Adam Sitnik** - BenchmarkDotNet Creator
- **Marc Gravell** - StackOverflow Performance Expert
