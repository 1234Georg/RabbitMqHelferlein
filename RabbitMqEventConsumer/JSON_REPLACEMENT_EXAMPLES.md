# JSON Replacement Demo Examples

This file contains example JSON messages and corresponding replacement rules to demonstrate the functionality.

## Example 1: User Registration Event

### Original JSON:
```json
{
  "EventType": "UserRegistered",
  "UserId": 12345,
  "Email": "john.doe@example.com",
  "Profile": {
    "FirstName": "John",
    "LastName": "Doe",
    "Phone": "+1-555-123-4567"
  },
  "Timestamp": "2024-01-15T10:30:45.123Z"
}
```

### Replacement Rules:
```json
[
  {
    "JsonPath": "UserId",
    "Placeholder": "{user_id}",
    "Enabled": true,
    "Description": "Replace user ID for privacy"
  },
  {
    "JsonPath": "Email",
    "Placeholder": "{email}",
    "Enabled": true,
    "Description": "Replace email for privacy"
  },
  {
    "JsonPath": "Profile.Phone",
    "Placeholder": "{phone_number}",
    "Enabled": true,
    "Description": "Replace phone number for privacy"
  }
]
```

### Result after Replacement:
```json
{
  "EventType": "UserRegistered",
  "UserId": "{user_id}",
  "Email": "{email}",
  "Profile": {
    "FirstName": "John",
    "LastName": "Doe",
    "Phone": "{phone_number}"
  },
  "Timestamp": "2024-01-15T10:30:45.123Z"
}
```

---

## Example 2: Order Event with Array Items

### Original JSON:
```json
{
  "EventType": "OrderCreated",
  "OrderId": "ORD-789123",
  "Customer": {
    "Id": 98765,
    "Email": "customer@example.com"
  },
  "Items": [
    {
      "ProductId": "PROD-001",
      "Price": 99.99,
      "Quantity": 2
    },
    {
      "ProductId": "PROD-002", 
      "Price": 149.50,
      "Quantity": 1
    }
  ],
  "Total": 349.48
}
```

### Replacement Rules:
```json
[
  {
    "JsonPath": "OrderId",
    "Placeholder": "{order_id}",
    "Enabled": true,
    "Description": "Replace order ID"
  },
  {
    "JsonPath": "Customer.Id",
    "Placeholder": "{customer_id}",
    "Enabled": true,
    "Description": "Replace customer ID"
  },
  {
    "JsonPath": "Customer.Email",
    "Placeholder": "{customer_email}",
    "Enabled": true,
    "Description": "Replace customer email"
  },
  {
    "JsonPath": "Items[0].Price",
    "Placeholder": "{first_item_price}",
    "Enabled": true,
    "Description": "Replace first item price"
  },
  {
    "JsonPath": "Total",
    "Placeholder": "{total_amount}",
    "Enabled": true,
    "Description": "Replace total amount"
  }
]
```

### Result after Replacement:
```json
{
  "EventType": "OrderCreated",
  "OrderId": "{order_id}",
  "Customer": {
    "Id": "{customer_id}",
    "Email": "{customer_email}"
  },
  "Items": [
    {
      "ProductId": "PROD-001",
      "Price": "{first_item_price}",
      "Quantity": 2
    },
    {
      "ProductId": "PROD-002", 
      "Price": 149.50,
      "Quantity": 1
    }
  ],
  "Total": "{total_amount}"
}
```

---

## Example 3: Payment Event with Nested Arrays

### Original JSON:
```json
{
  "EventType": "PaymentProcessed",
  "PaymentId": "PAY-456789",
  "CreditCard": {
    "Number": "**** **** **** 1234",
    "ExpiryDate": "12/25",
    "CVV": "123"
  },
  "TransactionDetails": {
    "Amount": 299.99,
    "Currency": "USD",
    "Fees": [
      {
        "Type": "ProcessingFee",
        "Amount": 8.50
      },
      {
        "Type": "ConvenienceFee", 
        "Amount": 2.99
      }
    ]
  }
}
```

### Replacement Rules:
```json
[
  {
    "JsonPath": "PaymentId",
    "Placeholder": "{payment_id}",
    "Enabled": true,
    "Description": "Replace payment ID"
  },
  {
    "JsonPath": "CreditCard.Number",
    "Placeholder": "{masked_card}",
    "Enabled": true,
    "Description": "Mask credit card number"
  },
  {
    "JsonPath": "CreditCard.CVV",
    "Placeholder": "{cvv}",
    "Enabled": true,
    "Description": "Mask CVV"
  },
  {
    "JsonPath": "TransactionDetails.Amount",
    "Placeholder": "{transaction_amount}",
    "Enabled": true,
    "Description": "Replace transaction amount"
  },
  {
    "JsonPath": "TransactionDetails.Fees[0].Amount",
    "Placeholder": "{processing_fee}",
    "Enabled": true,
    "Description": "Replace processing fee amount"
  }
]
```

### Result after Replacement:
```json
{
  "EventType": "PaymentProcessed",
  "PaymentId": "{payment_id}",
  "CreditCard": {
    "Number": "{masked_card}",
    "ExpiryDate": "12/25",
    "CVV": "{cvv}"
  },
  "TransactionDetails": {
    "Amount": "{transaction_amount}",
    "Currency": "USD",
    "Fees": [
      {
        "Type": "ProcessingFee",
        "Amount": "{processing_fee}"
      },
      {
        "Type": "ConvenienceFee", 
        "Amount": 2.99
      }
    ]
  }
}
```

---

## Quick Commands to Test

### 1. Publish test events and see replacements in action:
```bash
dotnet run publish
```
Then in another terminal:
```bash
dotnet run
```

### 1. Show replacement rules while running:
Press `R` key while the consumer is running to see active rules and statistics.

## JSONPath Patterns Supported

| Pattern | Example | Description |
|---------|---------|-------------|
| `property` | `UserId` | Root-level property |
| `nested.property` | `User.Email` | Nested object property |
| `array[index]` | `Items[0]` | Array element by index |
| `nested.array[index]` | `User.Orders[0]` | Array within nested object |
| `array[index].property` | `Items[0].Price` | Property of array element |
| `deep.nested.array[index].property` | `Data.Items[0].Details.Price` | Complex nested path |

## Use Case Scenarios

1. **PII Masking**: Replace sensitive personal information
2. **Template Generation**: Create reusable message templates
3. **Environment Adaptation**: Replace environment-specific values
4. **Test Data Generation**: Convert real data to test placeholders
5. **Audit Log Anonymization**: Remove sensitive data from logs
6. **A/B Testing**: Replace values for different test scenarios
