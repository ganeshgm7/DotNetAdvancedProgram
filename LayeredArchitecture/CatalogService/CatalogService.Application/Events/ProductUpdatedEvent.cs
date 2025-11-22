namespace CatalogService.Application.Events;

public record ProductUpdatedEvent
    (
        Guid EventId,
        DateTime OccurredUtc,
        int ProductId,
        string Name,
        decimal Price,
        int CategoryId,
        bool IsDeleted
    );