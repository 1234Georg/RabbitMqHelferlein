# RabbitMQ Event Consumer

A C# console application that consumes events from RabbitMQ and prints them to the console with detailed formatting.

## Features

- ✅ Connects to RabbitMQ server
- ✅ Consumes messages from a configurable queue
- ✅ Pretty-prints JSON messages with proper formatting
- ✅ Displays message properties, headers, and metadata
- ✅ Configurable via `appsettings.json`
- ✅ Proper error handling and connection management
- ✅ Manual acknowledgment support
- ✅ Includes a test event publisher

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
4. Print each received event with formatting
5. Run until you press Enter

### Publishing Test Events

To test the consumer, you can publish some sample events:

```bash
dotnet run publish
```

This will publish several test events to the queue, which the consumer can then process.

### Example Output

```
📨 [2024-01-15 10:30:45.123] Event Received:
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
     Timestamp: 2024-01-15T10:30:45+00:00
     Headers:
       source: test-publisher
       version: 1.0
   ────────────────────────────────────────────────────────────
```

## Error Handling

The application includes comprehensive error handling:

- Connection failures are caught and reported
- Message processing errors are handled gracefully
- Failed messages are rejected (not requeued by default)
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

## Architecture

- `Program.cs` - Main consumer application
- `EventPublisher.cs` - Test event publisher utility
- `RabbitMqConfig.cs` - Configuration model
- `appsettings.json` - Configuration file

## License

This project is open source and available under the MIT License.
