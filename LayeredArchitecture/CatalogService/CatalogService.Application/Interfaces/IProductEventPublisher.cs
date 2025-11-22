using CatalogService.Application.Events;

namespace CatalogService.Application.Interfaces;

public interface IProductEventPublisher
{
    Task PublishAsync(ProductUpdatedEvent evt, CancellationToken cancellationToken = default);
}