namespace RabbitMqEventConsumer;

public class RabbitMqConfig
{
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
