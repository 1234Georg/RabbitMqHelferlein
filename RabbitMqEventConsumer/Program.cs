using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMqEventConsumer;

public class Program
{
    static async Task Main(string[] args)
    {
        // Check if user wants to publish test events
        if (args.Length > 0 && args[0] == "publish")
        {
            await TestEventPublisher.PublishTestEvents();
            return;
        }

        Console.WriteLine("RabbitMQ Event Consumer Starting...");
        Console.WriteLine("Press [enter] to exit.");
        Console.WriteLine();

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var rabbitMqConfig = new RabbitMqConfig();
        configuration.GetSection("RabbitMq").Bind(rabbitMqConfig);

        Console.WriteLine($"Connecting to RabbitMQ at {rabbitMqConfig.HostName}:{rabbitMqConfig.Port}");
        Console.WriteLine($"Queue: {rabbitMqConfig.QueueName}");
        Console.WriteLine();

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

            Console.WriteLine($"✓ Connected to RabbitMQ successfully!");
            Console.WriteLine($"✓ Queue '{rabbitMqConfig.QueueName}' is ready for consuming events.");
            Console.WriteLine($"Waiting for events... To exit press ENTER");
            Console.WriteLine();

            var consumer = new AsyncEventingBasicConsumer(channel);
            
            consumer.ReceivedAsync += async (model, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    
                    Console.WriteLine($"📨 [{timestamp}] Event Received:");
                    Console.WriteLine($"   Exchange: {eventArgs.Exchange ?? "(default)"}");
                    Console.WriteLine($"   Routing Key: {eventArgs.RoutingKey ?? "(none)"}");
                    Console.WriteLine($"   Delivery Tag: {eventArgs.DeliveryTag}");
                    Console.WriteLine($"   Message Size: {body.Length} bytes");
                    Console.WriteLine($"   Content:");
                    
                    // Pretty print message with indentation
                    if (IsJsonContent(message))
                    {
                        try
                        {
                            var formatted = System.Text.Json.JsonSerializer.Serialize(
                                System.Text.Json.JsonSerializer.Deserialize<object>(message),
                                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                            Console.WriteLine($"     {formatted.Replace("\n", "\n     ")}");
                        }
                        catch
                        {
                            Console.WriteLine($"     {message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"     {message}");
                    }
                    
                    // Print message properties if they exist
                    if (eventArgs.BasicProperties != null)
                    {
                        Console.WriteLine($"   Properties:");
                        if (!string.IsNullOrEmpty(eventArgs.BasicProperties.ContentType))
                            Console.WriteLine($"     Content-Type: {eventArgs.BasicProperties.ContentType}");
                        if (!string.IsNullOrEmpty(eventArgs.BasicProperties.MessageId))
                            Console.WriteLine($"     Message-Id: {eventArgs.BasicProperties.MessageId}");
                        if (!string.IsNullOrEmpty(eventArgs.BasicProperties.CorrelationId))
                            Console.WriteLine($"     Correlation-Id: {eventArgs.BasicProperties.CorrelationId}");
                        if (eventArgs.BasicProperties.Timestamp.UnixTime > 0)
                            Console.WriteLine($"     Timestamp: {DateTimeOffset.FromUnixTimeSeconds(eventArgs.BasicProperties.Timestamp.UnixTime)}");
                        
                        if (eventArgs.BasicProperties.Headers != null && eventArgs.BasicProperties.Headers.Count > 0)
                        {
                            Console.WriteLine($"     Headers:");
                            foreach (var header in eventArgs.BasicProperties.Headers)
                            {
                                var value = header.Value switch
                                {
                                    byte[] bytes => Encoding.UTF8.GetString(bytes),
                                    string str => str,
                                    _ => header.Value?.ToString() ?? "null"
                                };
                                Console.WriteLine($"       {header.Key}: {value}");
                            }
                        }
                    }
                    
                    Console.WriteLine($"   {new string('─', 60)}");
                    Console.WriteLine();

                    // Acknowledge the message if not auto-ack
                    if (!rabbitMqConfig.AutoAck)
                    {
                        await channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error processing message: {ex.Message}");
                    // Reject and requeue the message
                    if (!rabbitMqConfig.AutoAck)
                    {
                        await channel.BasicNackAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false, requeue: false);
                    }
                }
            };

            // Start consuming
            await channel.BasicConsumeAsync(
                queue: rabbitMqConfig.QueueName,
                autoAck: rabbitMqConfig.AutoAck,
                consumer: consumer);

            Console.ReadLine();
            
            Console.WriteLine("Shutting down consumer...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error occurred: {ex.Message}");
            
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            
            Console.WriteLine();
            Console.WriteLine("💡 Common issues and solutions:");
            Console.WriteLine("   1. Make sure RabbitMQ server is running");
            Console.WriteLine("   2. Check if the connection details in appsettings.json are correct");
            Console.WriteLine("   3. Verify network connectivity and firewall settings");
            Console.WriteLine("   4. Ensure the user has permissions to access the queue");
            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    private static bool IsJsonContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return false;
            
        content = content.Trim();
        return (content.StartsWith("{") && content.EndsWith("}")) || 
               (content.StartsWith("[") && content.EndsWith("]"));
    }
}
