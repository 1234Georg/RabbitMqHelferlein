using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace RabbitMqEventConsumer;

public class Program
{
    private static readonly List<ConsumedEvent> ConsumedEvents = new();
    private static readonly object EventsLock = new();

    static async Task Main(string[] args)
    {
        // Check if user wants to publish test events
        if (args.Length > 0 && args[0] == "publish")
        {
            await TestEventPublisher.PublishTestEvents();
            return;
        }

        // Check if user wants to show event history
        if (args.Length > 0 && args[0] == "history")
        {
            ShowEventHistory();
            return;
        }

        Console.WriteLine("RabbitMQ Event Consumer Starting...");
        Console.WriteLine("Commands:");
        Console.WriteLine("  - Press [S] to show statistics");
        Console.WriteLine("  - Press [H] to show event history");
        Console.WriteLine("  - Press [C] to clear event history");
        Console.WriteLine("  - Press [ENTER] to exit");
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
            Console.WriteLine($"Waiting for events...");
            Console.WriteLine();

            var consumer = new AsyncEventingBasicConsumer(channel);
            
            consumer.ReceivedAsync += async (model, eventArgs) =>
            {
                var consumedEvent = new ConsumedEvent
                {
                    Timestamp = DateTime.Now,
                    Exchange = eventArgs.Exchange ?? "(default)",
                    RoutingKey = eventArgs.RoutingKey ?? "(none)",
                    DeliveryTag = eventArgs.DeliveryTag
                };

                try
                {
                    var body = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    
                    // Store event details
                    consumedEvent.Message = message;
                    consumedEvent.MessageSize = body.Length;
                    consumedEvent.IsJson = IsJsonContent(message);

                    // Extract properties
                    if (eventArgs.BasicProperties != null)
                    {
                        consumedEvent.ContentType = eventArgs.BasicProperties.ContentType;
                        consumedEvent.MessageId = eventArgs.BasicProperties.MessageId;
                        consumedEvent.CorrelationId = eventArgs.BasicProperties.CorrelationId;
                        
                        if (eventArgs.BasicProperties.Timestamp.UnixTime > 0)
                        {
                            consumedEvent.MessageTimestamp = DateTimeOffset.FromUnixTimeSeconds(eventArgs.BasicProperties.Timestamp.UnixTime);
                        }

                        // Extract headers
                        if (eventArgs.BasicProperties.Headers != null)
                        {
                            foreach (var header in eventArgs.BasicProperties.Headers)
                            {
                                var value = header.Value switch
                                {
                                    byte[] bytes => Encoding.UTF8.GetString(bytes),
                                    string str => str,
                                    _ => header.Value?.ToString() ?? "null"
                                };
                                consumedEvent.Headers[header.Key] = value;
                            }
                        }
                    }

                    // Add to stored events list (thread-safe)
                    lock (EventsLock)
                    {
                        ConsumedEvents.Add(consumedEvent);
                        
                        // Limit stored events to prevent memory issues (keep last 1000)
                        if (ConsumedEvents.Count > 10000)
                        {
                            throw new Exception("Limit reached")
                        }
                    }

                    // Display the event
                    DisplayEvent(consumedEvent);

                    // Acknowledge the message if not auto-ack
                    if (!rabbitMqConfig.AutoAck)
                    {
                        await channel.BasicAckAsync(deliveryTag: eventArgs.DeliveryTag, multiple: false);
                    }
                }
                catch (Exception ex)
                {
                    consumedEvent.ProcessedSuccessfully = false;
                    consumedEvent.ErrorMessage = ex.Message;
                    
                    // Still add the event to the list even if processing failed
                    lock (EventsLock)
                    {
                        ConsumedEvents.Add(consumedEvent);
                        
                        if (ConsumedEvents.Count > 1000)
                        {
                            ConsumedEvents.RemoveAt(0);
                        }
                    }

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

            // Handle user input
            await HandleUserInput();
            
            Console.WriteLine("Shutting down consumer...");
            DisplayFinalStatistics();
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

    private static async Task HandleUserInput()
    {
        while (true)
        {
            var keyInfo = Console.ReadKey(true);
            
            switch (char.ToUpper(keyInfo.KeyChar))
            {
                case 'S':
                    ShowStatistics();
                    break;
                case 'H':
                    ShowEventHistory();
                    break;
                case 'C':
                    ClearEventHistory();
                    break;
                case '\r': // Enter key
                case '\n':
                    return;
            }
            
            await Task.Delay(100); // Small delay to prevent high CPU usage
        }
    }

    private static void DisplayEvent(ConsumedEvent eventData)
    {
        var timestamp = eventData.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var eventNumber = ConsumedEvents.Count;
        
        Console.WriteLine($"📨 [#{eventNumber}] [{timestamp}] Event Received:");
        Console.WriteLine($"   Exchange: {eventData.Exchange}");
        Console.WriteLine($"   Routing Key: {eventData.RoutingKey}");
        Console.WriteLine($"   Delivery Tag: {eventData.DeliveryTag}");
        Console.WriteLine($"   Message Size: {eventData.MessageSize} bytes");
        Console.WriteLine($"   Content:");
        
        // Pretty print message with indentation
        if (eventData.IsJson)
        {
            try
            {
                var formatted = System.Text.Json.JsonSerializer.Serialize(
                    System.Text.Json.JsonSerializer.Deserialize<object>(eventData.Message),
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine($"     {formatted.Replace("\n", "\n     ")}");
            }
            catch
            {
                Console.WriteLine($"     {eventData.Message}");
            }
        }
        else
        {
            Console.WriteLine($"     {eventData.Message}");
        }
        
        // Print message properties
        Console.WriteLine($"   Properties:");
        if (!string.IsNullOrEmpty(eventData.ContentType))
            Console.WriteLine($"     Content-Type: {eventData.ContentType}");
        if (!string.IsNullOrEmpty(eventData.MessageId))
            Console.WriteLine($"     Message-Id: {eventData.MessageId}");
        if (!string.IsNullOrEmpty(eventData.CorrelationId))
            Console.WriteLine($"     Correlation-Id: {eventData.CorrelationId}");
        if (eventData.MessageTimestamp.HasValue)
            Console.WriteLine($"     Timestamp: {eventData.MessageTimestamp.Value}");
        
        if (eventData.Headers.Any())
        {
            Console.WriteLine($"     Headers:");
            foreach (var header in eventData.Headers)
            {
                Console.WriteLine($"       {header.Key}: {header.Value}");
            }
        }
        
        Console.WriteLine($"   {new string('─', 60)}");
        Console.WriteLine();
    }

    private static void ShowStatistics()
    {
        lock (EventsLock)
        {
            Console.WriteLine();
            Console.WriteLine("📊 Event Statistics:");
            Console.WriteLine($"   Total Events Consumed: {ConsumedEvents.Count}");
            
            if (ConsumedEvents.Any())
            {
                var successful = ConsumedEvents.Count(e => e.ProcessedSuccessfully);
                var failed = ConsumedEvents.Count(e => !e.ProcessedSuccessfully);
                var jsonEvents = ConsumedEvents.Count(e => e.IsJson);
                var textEvents = ConsumedEvents.Count(e => !e.IsJson);
                
                Console.WriteLine($"   Successful: {successful}");
                Console.WriteLine($"   Failed: {failed}");
                Console.WriteLine($"   JSON Events: {jsonEvents}");
                Console.WriteLine($"   Text Events: {textEvents}");
                
                var firstEvent = ConsumedEvents.First().Timestamp;
                var lastEvent = ConsumedEvents.Last().Timestamp;
                var duration = lastEvent - firstEvent;
                
                Console.WriteLine($"   First Event: {firstEvent:yyyy-MM-dd HH:mm:ss.fff}");
                Console.WriteLine($"   Last Event: {lastEvent:yyyy-MM-dd HH:mm:ss.fff}");
                
                if (duration.TotalSeconds > 0)
                {
                    var eventsPerSecond = ConsumedEvents.Count / duration.TotalSeconds;
                    Console.WriteLine($"   Average Rate: {eventsPerSecond:F2} events/second");
                }
                
                // Content type statistics
                var contentTypes = ConsumedEvents
                    .Where(e => !string.IsNullOrEmpty(e.ContentType))
                    .GroupBy(e => e.ContentType)
                    .ToDictionary(g => g.Key!, g => g.Count());
                
                if (contentTypes.Any())
                {
                    Console.WriteLine($"   Content Types:");
                    foreach (var ct in contentTypes.OrderByDescending(x => x.Value))
                    {
                        Console.WriteLine($"     {ct.Key}: {ct.Value}");
                    }
                }
            }
            
            Console.WriteLine($"   {new string('─', 60)}");
            Console.WriteLine();
        }
    }

    private static void ShowEventHistory()
    {
        lock (EventsLock)
        {
            Console.WriteLine();
            Console.WriteLine("📚 Event History:");
            
            if (!ConsumedEvents.Any())
            {
                Console.WriteLine("   No events consumed yet.");
                Console.WriteLine($"   {new string('─', 60)}");
                Console.WriteLine();
                return;
            }
            
            // Show last 10 events
            var recentEvents = ConsumedEvents.TakeLast(10).ToList();
            
            foreach (var eventData in recentEvents)
            {
                var index = ConsumedEvents.IndexOf(eventData) + 1;
                var status = eventData.ProcessedSuccessfully ? "✅" : "❌";
                var contentType = eventData.IsJson ? "JSON" : "TEXT";
                
                Console.WriteLine($"   #{index} {status} [{eventData.Timestamp:HH:mm:ss.fff}] {contentType} - {eventData.MessageSize} bytes");
                
                // Show first 50 chars of message
                var preview = eventData.Message.Length > 50 
                    ? eventData.Message[..50] + "..." 
                    : eventData.Message;
                Console.WriteLine($"      Preview: {preview.Replace('\n', ' ').Replace('\r', ' ')}");
                
                if (!eventData.ProcessedSuccessfully)
                {
                    Console.WriteLine($"      Error: {eventData.ErrorMessage}");
                }
            }
            
            if (ConsumedEvents.Count > 10)
            {
                Console.WriteLine($"   ... and {ConsumedEvents.Count - 10} more events");
            }
            
            Console.WriteLine($"   {new string('─', 60)}");
            Console.WriteLine();
        }
    }

    private static void ClearEventHistory()
    {
        lock (EventsLock)
        {
            var count = ConsumedEvents.Count;
            ConsumedEvents.Clear();
            Console.WriteLine();
            Console.WriteLine($"🗑️  Cleared {count} events from history.");
            Console.WriteLine($"   {new string('─', 60)}");
            Console.WriteLine();
        }
    }

    private static void DisplayFinalStatistics()
    {
        Console.WriteLine();
        Console.WriteLine("📈 Final Session Statistics:");
        ShowStatistics();
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
