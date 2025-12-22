using LLMProxy.Domain.Common;
using LLMProxy.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace LLMProxy.Infrastructure.PostgreSQL.Interceptors;

/// <summary>
/// Intercepteur EF Core qui convertit automatiquement les Domain Events en messages Outbox.
/// </summary>
/// <remarks>
/// <para>
/// Conforme à l'ADR-040 Outbox Pattern et ADR-025 Domain Events.
/// </para>
/// <para>
/// Cet intercepteur s'exécute AVANT SaveChangesAsync pour capturer tous les Domain Events
/// des entités modifiées et les convertir en messages Outbox qui seront sauvegardés dans
/// la MÊME transaction que les données métier.
/// </para>
/// <para>
/// Cela garantit l'atomicité : soit les données ET les événements sont sauvegardés,
/// soit rien n'est sauvegardé.
/// </para>
/// </remarks>
public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    private readonly ILogger<OutboxInterceptor> _logger;

    public OutboxInterceptor(ILogger<OutboxInterceptor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Intercepte SaveChangesAsync pour convertir les Domain Events en messages Outbox.
    /// </summary>
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;

        if (context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // Collecter tous les Domain Events des entités implémentant IHasDomainEvents
        var entitiesWithEvents = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity)
            .ToList();

        if (!entitiesWithEvents.Any())
        {
            _logger.LogDebug("No domain events found in ChangeTracker");
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        // Extraire tous les événements
        var allDomainEvents = entitiesWithEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        _logger.LogDebug(
            "Found {EventCount} domain events from {EntityCount} entities",
            allDomainEvents.Count,
            entitiesWithEvents.Count);

        // Convertir en messages Outbox
        var outboxMessages = allDomainEvents
            .Select(domainEvent => OutboxMessage.Create(domainEvent))
            .ToList();

        // Ajouter au contexte (même transaction)
        context.Set<OutboxMessage>().AddRange(outboxMessages);

        _logger.LogInformation(
            "Created {MessageCount} outbox messages for events: {EventTypes}",
            outboxMessages.Count,
            string.Join(", ", allDomainEvents.Select(e => e.GetType().Name).Distinct()));

        // Nettoyer les événements des entités (évite re-publication)
        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Intercepte SaveChanges synchrone (même logique que SaveChangesAsync).
    /// </summary>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context;

        if (context is null)
        {
            return base.SavingChanges(eventData, result);
        }

        // Même logique que SavingChangesAsync mais en synchrone
        var entitiesWithEvents = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Any())
            .Select(entry => entry.Entity)
            .ToList();

        if (!entitiesWithEvents.Any())
        {
            _logger.LogDebug("No domain events found in ChangeTracker");
            return base.SavingChanges(eventData, result);
        }

        var allDomainEvents = entitiesWithEvents
            .SelectMany(entity => entity.DomainEvents)
            .ToList();

        _logger.LogDebug(
            "Found {EventCount} domain events from {EntityCount} entities",
            allDomainEvents.Count,
            entitiesWithEvents.Count);

        var outboxMessages = allDomainEvents
            .Select(domainEvent => OutboxMessage.Create(domainEvent))
            .ToList();

        context.Set<OutboxMessage>().AddRange(outboxMessages);

        _logger.LogInformation(
            "Created {MessageCount} outbox messages for events: {EventTypes}",
            outboxMessages.Count,
            string.Join(", ", allDomainEvents.Select(e => e.GetType().Name).Distinct()));

        foreach (var entity in entitiesWithEvents)
        {
            entity.ClearDomainEvents();
        }

        return base.SavingChanges(eventData, result);
    }
}
