using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using CatalogService.Application.Events;
using CartService.API.BusinessLogic.Interfaces;
using Microsoft.Extensions.Options;

namespace CartService.API.Messaging;

public class ProductEventListener : BackgroundService
{
    private readonly ILogger<ProductEventListener> _logger;
    private readonly ServiceBusProcessor _processor;
    private readonly ICartRepository _cartRepository;
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public ProductEventListener(IOptions<ServiceBusSubscriptionSettings> options,
                                ILogger<ProductEventListener> logger,
                                ICartRepository cartRepository)
    {
        _logger = logger;
        _cartRepository = cartRepository;

        ServiceBusSubscriptionSettings serviceBusSubscriptionSettings = options.Value;

        ServiceBusClient client = new(serviceBusSubscriptionSettings.ConnectionString);

        _processor = client.CreateProcessor(serviceBusSubscriptionSettings.TopicName, serviceBusSubscriptionSettings.SubscriptionName, new ServiceBusProcessorOptions
        {
            MaxConcurrentCalls = 2,
            AutoCompleteMessages = false
        });

        _processor.ProcessMessageAsync += OnMessageAsync;
        _processor.ProcessErrorAsync += OnErrorAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting ProductEventListener...");
        await _processor.StartProcessingAsync(stoppingToken);
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            string json = Encoding.UTF8.GetString(args.Message.Body);

            ProductUpdatedEvent? evt = JsonSerializer.Deserialize<ProductUpdatedEvent>(json, _jsonOptions);

            if (evt is null)
            {
                _logger.LogWarning("Received invalid ProductUpdatedEvent. MessageId={MessageId}", args.Message.MessageId);

                await args.AbandonMessageAsync(args.Message);
                return;
            }

            if (evt.IsDeleted)
            {
                await _cartRepository.RemoveProductFromAllCartsAsync(evt.ProductId);
            }
            else
            {
                await _cartRepository.UpdateProductMetadataAsync(evt.ProductId, evt.Name, evt.Price);
            }

            await args.CompleteMessageAsync(args.Message);

            _logger.LogInformation("Processed ProductUpdatedEvent ProductId={ProductId} EventId={EventId}", evt.ProductId, evt.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message MessageId={MessageId}", args.Message.MessageId);

            if (args.Message.DeliveryCount >= 5)
            {
                await args.DeadLetterMessageAsync(args.Message, "ProcessingFailed", ex.Message);
            }
            else
            {
                await args.AbandonMessageAsync(args.Message);
            }
        }
    }

    private Task OnErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "ServiceBus error Source={Source}", args.ErrorSource);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _processor.ProcessMessageAsync -= OnMessageAsync;
        _processor.ProcessErrorAsync -= OnErrorAsync;
        _ = _processor.DisposeAsync();
        base.Dispose();
    }
}