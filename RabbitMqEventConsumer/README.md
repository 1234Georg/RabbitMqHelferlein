# RabbitMQ Event Consumer with JSON Value Replacement

A C# console application that consumes events from RabbitMQ, stores them in memory, provides **JSON value replacement with JSONPath syntax** for data masking and templating and generates Jmeter Testplans with the consumed events as post-payload

## 🎯 Key Features

- ✅ Connects to RabbitMQ server
- ✅ Consumes messages from a configurable queue
- ✅ Stores consumed events in memory for analysis
- ✅ JSON Value Replacement using JSONPath syntax (e.g., `user.profile.email={email}`)
- ✅ Support for nested objects and array elements (e.g., `items[0].price={price}`)
- ✅ Generate JMeter template for testing
- ✅ Interactive commands for viewing statistics and history
- ✅ Real-time event counting and performance metrics
- ✅ Pretty-prints JSON messages with proper formatting
- ✅ Displays message properties, headers, and metadata
- ✅ Configurable via `appsettings.json`
- ✅ Proper error handling and connection management
- ✅ Manual acknowledgment support
- ✅ Includes a test event publisher

## 🔄 JSON Replacement Examples

### Basic Property Replacement
```json
Original: {"UserId": 12345, "Email": "john@example.com"}
Rule:     {"JsonPath": "UserId", "Placeholder": "{user_id}"}
Result:   {"UserId": "{user_id}", "Email": "john@example.com"}
```

### Nested Object Replacement
```json
Original: {"user": {"profile": {"email": "john@example.com"}}}
Rule:     {"JsonPath": "user.profile.email", "Placeholder": "{email}"}
Result:   {"user": {"profile": {"email": "{email}"}}}
```

### Array Element Replacement
```json
Original: {"items": [{"price": 99.99}, {"price": 150.00}]}
Rule:     {"JsonPath": "items[0].price", "Placeholder": "{first_price}"}
Result:   {"items": [{"price": "{first_price}"}, {"price": 150.00}]}
```

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

Edit `appsettings.json` to configure both RabbitMQ connection and JSON replacement rules:

```json
{
  "RabbitMq": {
    "Enabled": true,
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
  },
  "JsonReplacement": {
    "EnableReplacements": true,
    "ShowOriginalMessage": true,
    "ShowProcessedMessage": true,
    "Rules": [
      {
        "JsonPath": "UserId",
        "Placeholder": "{user_id}",
        "Enabled": true,
        "Description": "Replace user ID with placeholder"
      },
      {
        "JsonPath": "Email",
        "Placeholder": "{email}",
        "Enabled": true,
        "Description": "Replace email with placeholder"
      },
      {
        "JsonPath": "user.profile.email",
        "Placeholder": "{profile_email}",
        "Enabled": false,
        "Description": "Example: nested object path"
      },
      {
        "JsonPath": "items[0].price",
        "Placeholder": "{first_item_price}",
        "Enabled": false,
        "Description": "Example: array element path"
      }
    ]
  }
}
```

### JSON Replacement Configuration Options

- **EnableReplacements**: Whether to enable JSON value replacement
- **ShowOriginalMessage**: Display the original message content
- **ShowProcessedMessage**: Display the processed message with replacements
- **Rules**: Array of replacement rules with:
  - **JsonPath**: JSONPath expression (supports dot notation and array indices)
  - **Placeholder**: The replacement value (typically `{variable_name}`)
  - **Enabled**: Whether this rule is active
  - **Description**: Human-readable description of the rule

### JSONPath Syntax Support

| Pattern | Description | Example |
|---------|-------------|---------|
| `property` | Root property | `UserId` |
| `object.property` | Nested property | `user.email` |
| `array[index]` | Array element | `items[0]` |
| `object.array[index].property` | Complex path | `user.orders[0].total` |

## Usage

### Running the Consumer

```bash
dotnet run
```

The application will:
1. Connect to RabbitMQ
2. Load JSON replacement rules
3. Create/verify the queue exists
4. Start consuming messages
5. Apply JSON replacements if configured
6. Store each event in memory
7. Print each received event with formatting
8. Accept interactive commands

### Interactive Commands

While the consumer is running, you can use these keyboard commands:

- **S** - Show detailed statistics
- **H** - Show event history (last 10 events)
- **C** - Clear event history from memory
- **R** - Show JSON replacement rules and status
- **J** - Generate JMeter template for load testing
- **ENTER** - Exit the application

### Publishing Test Events

To test the consumer, you can publish some sample events:

```bash
dotnet run publish
```

This will publish several test events to the queue, which the consumer can then process and apply replacements.

## JMX File Generation Feature

The JMX file generation feature createa JMeter test plans from captured RabbitMQ events. This enhancement allows for automatic generation of HTTP test steps based on real event data.

### 1. Event-Driven Test Generation
- **Automatic Test Step Creation**: Each captured event becomes a separate HTTP test step
- **Payload Integration**: Event messages (original or processed) are XML-escaped and embedded as request payloads
- **Dynamic Test Names**: Test steps are automatically named with event sequence and timestamp

### 2. Template-Based Architecture
- **Main Template**: Uses `JMeterTemplate.jmx` as the base structure, add this file in the same directory as the executable app. Use jmx-Parameters to fit your needs (e.g. server-url, generated Ids)
- **Test Step Template**: Uses `Teststep.jmx` for individual HTTP request steps, add this file to fit your needs
- **Placeholder Replacement**:
  - `<!--#Teststeps#-->` in `JMeterTemplate.jmx` is replaced with all generated test steps
  - `<!--#payload#-->` in `Teststep.jmx` is replaced with XML-escaped event content

### 3. Smart Payload Selection
- **Processed Messages First**: If JSON replacements were applied, uses the processed message
- **Fallback to Original**: If no processing occurred, uses the original event message
- **XML Escaping**: All content is properly XML-escaped to ensure valid JMX structure

## License

This project is open source and available under the MIT License.

## Testing

The project includes comprehensive unit tests for the JsonReplacementService:

```bash
# Run all tests
cd RabbitMqEventConsumer.Tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

See `RabbitMqEventConsumer.Tests/README.md` for detailed test documentation.
