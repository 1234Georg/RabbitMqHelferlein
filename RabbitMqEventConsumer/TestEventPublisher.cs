using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace RabbitMqEventConsumer;

public static class TestEventPublisher
{
    public static async Task PublishTestEvents()
    {
        Console.WriteLine("RabbitMQ Event Publisher - Publishing test events...");
        
        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var rabbitMqConfig = new RabbitMqConfig();
        configuration.GetSection("RabbitMq").Bind(rabbitMqConfig);

        var factory = new ConnectionFactory
        {
            HostName = rabbitMqConfig.HostName,
            Port = rabbitMqConfig.Port,
            UserName = rabbitMqConfig.Username,
            Password = rabbitMqConfig.Password,
            VirtualHost = rabbitMqConfig.VirtualHost
        };

        try
        {
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            // Declare the queue (creates it if it doesn't exist)
            await channel.QueueDeclareAsync(
                queue: rabbitMqConfig.QueueName,
                durable: rabbitMqConfig.Durable,
                exclusive: rabbitMqConfig.Exclusive,
                autoDelete: rabbitMqConfig.AutoDelete,
                arguments: null);

            // Sample events to publish
            var jsonEvents = new object[]
            {
                new { EventType = "UserRegistered", UserId = 123, Email = "user@example.com", Timestamp = DateTime.UtcNow },
                new { EventType = "OrderCreated", OrderId = 456, Amount = 99.99, Currency = "USD", Timestamp = DateTime.UtcNow },
                new { EventType = "PaymentProcessed", PaymentId = 789, OrderId = 456, Status = "Completed", Timestamp = DateTime.UtcNow },
                new { EventType = "SystemAlert", Level = "Warning", Message = "High CPU usage detected", Timestamp = DateTime.UtcNow }
            };

            var stringEvents = new string[]
            {
                "Simple string message",
                "Another plain text event",
                "System startup notification"
            };

            // Publish JSON events
            foreach (var eventObj in jsonEvents)
            {
                string message = JsonSerializer.Serialize(eventObj);
                var body = Encoding.UTF8.GetBytes(message);

                var properties = new BasicProperties
                {
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString(),
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                    Headers = new Dictionary<string, object>
                    {
                        ["source"] = Encoding.UTF8.GetBytes("test-publisher"),
                        ["version"] = Encoding.UTF8.GetBytes("1.0"),
                        ["event-type"] = Encoding.UTF8.GetBytes("json-event")
                    }
                };

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: rabbitMqConfig.QueueName,
                    mandatory: false,
                    basicProperties: properties,
                    body: body);

                Console.WriteLine($"📤 Published JSON: {message}");
                await Task.Delay(1000); // Wait 1 second between messages
            }

            // Publish string events
            foreach (var eventMsg in stringEvents)
            {
                var body = Encoding.UTF8.GetBytes(eventMsg);

                var properties = new BasicProperties
                {
                    ContentType = "text/plain",
                    MessageId = Guid.NewGuid().ToString(),
                    Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
                    Headers = new Dictionary<string, object>
                    {
                        ["source"] = Encoding.UTF8.GetBytes("test-publisher"),
                        ["version"] = Encoding.UTF8.GetBytes("1.0"),
                        ["event-type"] = Encoding.UTF8.GetBytes("text-event")
                    }
                };

                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: rabbitMqConfig.QueueName,
                    mandatory: false,
                    basicProperties: properties,
                    body: body);

                Console.WriteLine($"📤 Published Text: {eventMsg}");
                await Task.Delay(1000); // Wait 1 second between messages
            }

            Console.WriteLine("✓ All test events published successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error publishing events: {ex.Message}");
        }
    }
}
