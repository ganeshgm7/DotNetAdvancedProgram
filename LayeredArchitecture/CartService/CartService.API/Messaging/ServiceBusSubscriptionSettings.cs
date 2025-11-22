namespace CartService.API.Messaging;

public class ServiceBusSubscriptionSettings
{
    public string ConnectionString { get; set; } = string.Empty;

    public string TopicName { get; set; } = "product-changes";

    public string SubscriptionName { get; set; } = "cart-updates";
}