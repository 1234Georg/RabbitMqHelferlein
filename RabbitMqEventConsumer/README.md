# RabbitMQ Event Consumer with JSON Value Replacement

A C# console application that consumes events from RabbitMQ, stores them in memory, and provides **JSON value replacement with JSONPath syntax** for data masking and templating.

## 🎯 Key Features

- ✅ **JSON Value Replacement using JSONPath syntax** (e.g., `user.profile.email={email}`)
- ✅ **Configurable replacement rules with placeholders**
- ✅ **Support for nested objects and array elements** (e.g., `items[0].price={price}`)
- ✅ **Data masking for sensitive information** (PII protection)
- ✅ Connects to RabbitMQ server
- ✅ Consumes messages from a configurable queue
- ✅ Stores consumed events in memory for analysis
- ✅ Interactive commands for viewing statistics and history
- ✅ Real-time event counting and performance metrics
- ✅ Pretty-prints JSON messages with proper formatting
- ✅ Displays message properties, headers, and metadata
- ✅ Configurable via `appsettings.json`
- ✅ Proper error handling and connection management
- ✅ Manual acknowledgment support
- ✅ Includes a test event publisher
- ✅ Memory-efficient event storage (keeps last 1000 events)

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
- **ENTER** - Exit the application

### Publishing Test Events

To test the consumer, you can publish some sample events:

```bash
dotnet run publish
```

This will publish several test events to the queue, which the consumer can then process and apply replacements.

### Analyze JSON Paths

To discover available JSONPaths in a JSON message:

```bash
dotnet run analyze '{"user":{"id":123,"profile":{"email":"test@example.com"}}}'
```

This will extract all possible JSONPaths and show example replacement rule configurations.

## Example Output

### Event Display with Replacements
```
📨 [#1] [2024-01-15 10:30:45.123] Event Received:
   Exchange: (default)
   Routing Key: events_queue
   Delivery Tag: 1
   Message Size: 156 bytes
   🔄 JSON Replacements Applied: 2
      • UserId → {user_id}
      • Email → {email}
   Original Content:
     {
       "EventType": "UserRegistered",
       "UserId": 123,
       "Email": "user@example.com",
       "Timestamp": "2024-01-15T10:30:45.123Z"
     }
   Processed Content:
     {
       "EventType": "UserRegistered",
       "UserId": "{user_id}",
       "Email": "{email}",
       "Timestamp": "2024-01-15T10:30:45.123Z"
     }
   Properties:
     Content-Type: application/json
     Message-Id: 12345678-1234-5678-9abc-123456789012
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
   Events with Replacements: 18
   First Event: 2024-01-15 10:30:45.123
   Last Event: 2024-01-15 10:32:15.456
   Average Rate: 0.34 events/second
   ────────────────────────────────────────────────────────────
```

### Replacement Rules Display (Press 'R')
```
🔄 JSON Replacement Rules:
   Status: ✅ Enabled
   Show Original: ✅ Yes
   Show Processed: ✅ Yes

   Rules:
   1. ✅ UserId → {user_id}
      Description: Replace user ID with placeholder
   2. ✅ Email → {email}
      Description: Replace email with placeholder
   3. ❌ user.profile.email → {profile_email}
      Description: Example: nested object path
   4. ❌ items[0].price → {first_item_price}
      Description: Example: array element path

   Applied in Session: 18 of 25 events
   ────────────────────────────────────────────────────────────
```

### Event History with Replacement Indicators (Press 'H')
```
📚 Event History:
   #21 ✅🔄 [10:32:10.123] JSON - 156 bytes
      Preview: {"EventType":"UserRegistered","UserId":123,"Email":"user@example.com"}
      Replacements: 2 applied
   #22 ✅ [10:32:11.234] TEXT - 25 bytes
      Preview: Simple string message
   #23 ✅🔄 [10:32:12.345] JSON - 89 bytes
      Preview: {"OrderId":456,"Amount":99.99}
      Replacements: 1 applied
   ────────────────────────────────────────────────────────────
```

## Use Cases

### 1. **Data Masking for Privacy Compliance**
```json
{
  "JsonPath": "customer.personalInfo.ssn",
  "Placeholder": "{masked_ssn}",
  "Description": "Mask SSN for privacy compliance"
}
```

### 2. **Template Generation**
```json
{
  "JsonPath": "order.customerId",
  "Placeholder": "{customer_id}",
  "Description": "Create order template with customer placeholder"
}
```

### 3. **Environment-specific Value Replacement**
```json
{
  "JsonPath": "config.databaseUrl",
  "Placeholder": "{db_connection}",
  "Description": "Replace database URL with environment variable"
}
```

### 4. **Testing Data Anonymization**
```json
{
  "JsonPath": "user.email",
  "Placeholder": "{test_email}",
  "Description": "Replace real emails with test placeholders"
}
```

## Event Storage

The application stores each consumed event as a `ConsumedEvent` object containing:

- **Original Message** - The untouched message content
- **Processed Message** - Message with JSON replacements applied
- **Applied Replacements** - List of which rules were applied
- **Replacement Status** - Whether any replacements occurred
- **Standard Event Data** - Timestamp, routing info, headers, etc.

## Error Handling

- **JSON Parsing Errors**: Invalid JSON won't crash the replacement process
- **Invalid JSONPath**: Malformed paths are skipped with logging
- **Rule Processing**: Failed rules don't affect other rules or message processing
- **Original Data Preservation**: Original messages are always retained

## Performance

- **Memory Efficient**: Only stores processed messages when replacements are applied
- **Thread Safe**: Concurrent processing of replacement rules
- **Fast JSONPath**: Simple dot-notation parser for quick path resolution
- **Rule Optimization**: Disabled rules are skipped during processing

## Architecture

- `Program.cs` - Main consumer with replacement integration
- `JsonReplacementService.cs` - Core replacement logic and JSONPath parser
- `JsonReplacementConfig.cs` - Configuration models for replacement rules
- `ConsumedEvent.cs` - Extended event model with replacement data
- `TestEventPublisher.cs` - Test publisher with sample JSON events
- `appsettings.json` - Configuration with replacement rules

## License

This project is open source and available under the MIT License.
