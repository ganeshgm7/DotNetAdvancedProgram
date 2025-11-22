using System.Text;
using System.Text.Json;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using CatalogService.Application.Events;
using CatalogService.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace CatalogService.Infrastructure.Messaging;

public class ServiceBusProductEventPublisher : IProductEventPublisher, IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusProductEventPublisher> _logger;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public ServiceBusProductEventPublisher(IOptions<ServiceBusPublisherSettings> options,
                                           ILogger<ServiceBusProductEventPublisher> logger)
    {
        _logger = logger;

        ServiceBusPublisherSettings serviceBusPublisherSettings = options.Value;

        _client = new ServiceBusClient(serviceBusPublisherSettings.ConnectionString);

        _sender = _client.CreateSender(serviceBusPublisherSettings.TopicName);
    }


    public async Task PublishAsync(ProductUpdatedEvent evt, CancellationToken cancellationToken = default)
    {
        string json = JsonSerializer.Serialize(evt, _jsonOptions);

        ServiceBusMessage message = new(Encoding.UTF8.GetBytes(json))
        {
            MessageId = evt.EventId.ToString(),
            Subject = "catalog.product.updated"
        };

        message.ApplicationProperties["productId"] = evt.ProductId;
        message.ApplicationProperties["isDeleted"] = evt.IsDeleted;

        var retryPolicy = Policy
            .Handle<ServiceBusException>(ex => ex.IsTransient)
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

        try
        {
            await retryPolicy.ExecuteAsync(() => _sender.SendMessageAsync(message, cancellationToken));
            _logger.LogInformation("Published ProductUpdatedEvent ProductId={ProductId} EventId={EventId}", evt.ProductId, evt.EventId);
        }
        catch (ServiceBusException sbEx)
        {
            _logger.LogError(sbEx, "Service Bus error sending ProductUpdatedEvent ProductId={ProductId}", evt.ProductId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error publishing ProductUpdatedEvent ProductId={ProductId}", evt.ProductId);
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}