namespace RabbitMqEventConsumer;

public class RabbitMqConfig
{
    public bool Enabled { get; set; } = true;
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string QueueName { get; set; } = "events_queue";
    public string VirtualHost { get; set; } = "/";
    public bool AutoAck { get; set; } = false;
    public bool Durable { get; set; } = true;
    public bool Exclusive { get; set; } = false;
    public bool AutoDelete { get; set; } = false;
}

public class JsonReplacementRule
{
    public string JsonPath { get; set; } = string.Empty;
    public string Placeholder { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string Description { get; set; } = string.Empty;
}

public class JsonReplacementConfig
{
    public bool EnableReplacements { get; set; } = false;
    public bool ShowOriginalMessage { get; set; } = true;
    public bool ShowProcessedMessage { get; set; } = true;
    public List<JsonReplacementRule> Rules { get; set; } = new();
}

public class PostUrlMapping
{
    public string EventName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class PostUrlsConfig
{
    public List<PostUrlMapping> PostUrls { get; set; } = new();
}
