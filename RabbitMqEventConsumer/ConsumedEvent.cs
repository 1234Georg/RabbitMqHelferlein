namespace RabbitMqEventConsumer;

public class ConsumedEvent
{
    public DateTime Timestamp { get; set; }
    public string Exchange { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public ulong DeliveryTag { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ProcessedMessage { get; set; }
    public bool HasReplacements { get; set; }
    public int MessageSize { get; set; }
    public string? ContentType { get; set; }
    public string? MessageId { get; set; }
    public string? CorrelationId { get; set; }
    public DateTimeOffset? MessageTimestamp { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public bool IsJson { get; set; }
    public bool ProcessedSuccessfully { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public List<string> AppliedReplacements { get; set; } = new();
}
