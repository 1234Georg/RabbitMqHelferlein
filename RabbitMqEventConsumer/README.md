# RabbitMQ Event Consumer

A C# console application that consumes events from RabbitMQ, stores them in memory, and provides detailed formatting and analysis capabilities.

## Features

- ✅ Connects to RabbitMQ server
- ✅ Consumes messages from a configurable queue
- ✅ **Stores consumed events in memory for analysis**
- ✅ **Interactive commands for viewing statistics and history**
- ✅ **Real-time event counting and performance metrics**
- ✅ Pretty-prints JSON messages with proper formatting
- ✅ Displays message properties, headers, and metadata
- ✅ Configurable via `appsettings.json`
- ✅ Proper error handling and connection management
- ✅ Manual acknowledgment support
- ✅ Includes a test event publisher
- ✅ **Memory-efficient event storage (keeps last 1000 events)**

## Prerequisites

- .NET 9.0 or later
- RabbitMQ server running (default: localhost:5672)

## Installation

1. Clone or download the project
2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

## Configuration

Edit `appsettings.json` to configure your RabbitMQ connection:

```json
{
  "RabbitMq": {
    "HostName": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "QueueName": "events_queue",
    "VirtualHost": "/",
    "AutoAck": false,
    "Durable": true,
    "Exclusive": false,
    "AutoDelete": false
  }
}
```

### Configuration Options

- **HostName**: RabbitMQ server hostname
- **Port**: RabbitMQ server port (default: 5672)
- **Username/Password**: Authentication credentials
- **QueueName**: Name of the queue to consume from
- **VirtualHost**: RabbitMQ virtual host (default: "/")
- **AutoAck**: Whether to auto-acknowledge messages (false = manual ack for reliability)
- **Durable**: Whether the queue should survive server restarts
- **Exclusive**: Whether the queue is exclusive to this connection
- **AutoDelete**: Whether to delete the queue when no consumers are connected

## Usage

### Running the Consumer

```bash
dotnet run
```

The application will:
1. Connect to RabbitMQ
2. Create/verify the queue exists
3. Start consuming messages
4. Store each event in memory
5. Print each received event with formatting
6. Accept interactive commands

### Interactive Commands

While the consumer is running, you can use these keyboard commands:

- **S** - Show detailed statistics
- **H** - Show event history (last 10 events)
- **C** - Clear event history from memory
- **ENTER** - Exit the application

### Publishing Test Events

To test the consumer, you can publish some sample events:

```bash
dotnet run publish
```

This will publish several test events to the queue, which the consumer can then process.

### Show Event History

```bash
dotnet run history
```

This will show the stored event history without starting the consumer.

## Example Output

### Event Display
```
📨 [#1] [2024-01-15 10:30:45.123] Event Received:
   Exchange: (default)
   Routing Key: events_queue
   Delivery Tag: 1
   Message Size: 156 bytes
   Content:
     {
       "EventType": "UserRegistered",
       "UserId": 123,
       "Email": "user@example.com",
       "Timestamp": "2024-01-15T10:30:45.123Z"
     }
   Properties:
     Content-Type: application/json
     Message-Id: 12345678-1234-5678-9abc-123456789012
     Headers:
       source: test-publisher
       version: 1.0
   ────────────────────────────────────────────────────────────
```

### Statistics Display (Press 'S')
```
📊 Event Statistics:
   Total Events Consumed: 25
   Successful: 24
   Failed: 1
   JSON Events: 20
   Text Events: 5
   First Event: 2024-01-15 10:30:45.123
   Last Event: 2024-01-15 10:32:15.456
   Average Rate: 0.34 events/second
   Content Types:
     application/json: 20
     text/plain: 5
   ────────────────────────────────────────────────────────────
```

### Event History (Press 'H')
```
📚 Event History:
   #21 ✅ [10:32:10.123] JSON - 156 bytes
      Preview: {"EventType":"UserRegistered","UserId":123,"Email":"user@example.com"}
   #22 ✅ [10:32:11.234] TEXT - 25 bytes
      Preview: Simple string message
   #23 ❌ [10:32:12.345] JSON - 89 bytes
      Preview: {"InvalidJson": true
      Error: Unexpected end of JSON input
   ────────────────────────────────────────────────────────────
```

## Event Storage

The application stores each consumed event as a `ConsumedEvent` object containing:

- **Timestamp** - When the event was received
- **Exchange & Routing Key** - RabbitMQ routing information
- **Message Content** - The actual message payload
- **Properties** - Content type, message ID, correlation ID, etc.
- **Headers** - Custom headers from the message
- **Processing Status** - Success/failure information
- **Performance Data** - Message size, processing time, etc.

Events are stored in memory and limited to the last 1000 events to prevent memory issues.

## Error Handling

The application includes comprehensive error handling:

- Connection failures are caught and reported
- Message processing errors are handled gracefully
- Failed messages are rejected (not requeued by default)
- Failed events are still stored with error information
- Helpful troubleshooting tips are provided

## Common Issues

1. **Connection refused**: Make sure RabbitMQ is running
2. **Authentication failed**: Check username/password in configuration
3. **Queue errors**: Verify queue permissions and settings

## Development

The project uses:
- **RabbitMQ.Client** (v7.0.0) for RabbitMQ connectivity
- **Microsoft.Extensions.Configuration** for configuration management
- **System.Text.Json** for JSON formatting
- **Thread-safe collections** for concurrent event storage

## Architecture

- `Program.cs` - Main consumer application with event storage
- `ConsumedEvent.cs` - Event data model
- `TestEventPublisher.cs` - Test event publisher utility
- `RabbitMqConfig.cs` - Configuration model
- `appsettings.json` - Configuration file

## Performance

- **Memory Usage**: Stores up to 1000 events in memory (auto-cleanup)
- **Thread Safety**: Uses locks for safe concurrent access to event storage
- **Event Processing**: Async/await pattern for non-blocking operations
- **Statistics**: Real-time calculation of throughput and performance metrics

## License

This project is open source and available under the MIT License.
