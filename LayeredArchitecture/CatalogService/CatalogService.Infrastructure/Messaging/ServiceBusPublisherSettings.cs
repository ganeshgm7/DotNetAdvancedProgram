namespace CatalogService.Infrastructure.Messaging;

public class ServiceBusPublisherSettings
{
    public string TopicName { get; set; } = string.Empty;

    public string? ConnectionString { get; set; }
}
