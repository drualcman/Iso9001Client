# Iso9001Client

A .NET library for publishing ISO 9001 quality management events to **Azure Storage Queues**. It provides a simple interface to register audit logs, report incidents/errors, capture customer feedback, and record non-conformities — all asynchronously via queues.

## Installation

```bash
dotnet add package Iso9001Client
```

Or via the NuGet Package Manager:

```
Install-Package Iso9001Client
```

## Requirements

- .NET 9.0+
- An Azure Storage Account (connection string with queue access)

## Configuration

Add the following section to your `appsettings.json`:

```json
{
  "Iso9001ClientOptions": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net",
    "CompanyId": "your-company-id",
    "AuditLogQueue": "iso9001-auditlogs",
    "IncidentQueue": "iso9001-incidents",
    "FeedbackQueue": "iso9001-feedbacks",
    "NonConformityQueue": "iso9001-nonconformities"
  }
}
```

The queue names default to the values shown above — you only need to specify them if you want to override them.

## Registration

In your `Program.cs` or wherever you configure services:

```csharp
builder.Services.AddIso9001Client(options =>
{
    builder.Configuration.GetSection(Iso9001ClientOptions.SectionKey).Bind(options);
});
```

Or configure inline:

```csharp
builder.Services.AddIso9001Client(options =>
{
    options.ConnectionString = "your-connection-string";
    options.CompanyId = "your-company-id";
});
```

## Usage

Inject `IIso9001` wherever you need it:

```csharp
public class OrderService(IIso9001 iso9001)
{
    public async Task CreateOrder(Order order)
    {
        // business logic...

        // Register an audit log entry
        await iso9001.Register<OrderCreated, Order>(
            reference: order.Id,
            action: new OrderCreated(),
            description: "Order created successfully",
            data: order);
    }
}
```

### Register an audit log (without data payload)

```csharp
await iso9001.Register<OrderShipped>(
    reference: orderId,
    action: new OrderShipped(),
    description: "Order shipped to customer");
```

### Register an audit log (with data payload)

```csharp
await iso9001.Register<OrderUpdated, OrderDto>(
    reference: orderId,
    action: new OrderUpdated(),
    description: "Order status updated",
    data: orderDto);
```

### Report an error from an exception

```csharp
try
{
    await ProcessPayment(order);
}
catch (Exception ex)
{
    await iso9001.Error<PaymentProcessing>(
        reference: order.Id,
        action: new PaymentProcessing(),
        ex: ex);
    throw;
}
```

### Report an error with a description

```csharp
await iso9001.Error<InventoryCheck>(
    reference: productId,
    action: new InventoryCheck(),
    description: "Insufficient stock to fulfill order");
```

### Report an error with a description and data payload

```csharp
await iso9001.Error<InventoryCheck, StockSnapshot>(
    reference: productId,
    action: new InventoryCheck(),
    description: "Insufficient stock",
    data: currentStockSnapshot);
```

### Register customer feedback

```csharp
await iso9001.RegisterFeedback(
    entityId: orderId,
    customerId: customer.Id,
    customerName: customer.Name,
    customerEmail: customer.Email,
    customerAntiPhishing: customer.AntiPhishingCode,
    rating: 5,
    comments: "Excellent service!");
```

## Queue message schemas

The library publishes the following message types to each queue:

### Audit Log (`iso9001-auditlogs`)

| Field | Type | Description |
|---|---|---|
| `Reference` | string | Entity/operation identifier |
| `CompanyId` | string | Company identifier from options |
| `Action` | string | Name of the action type (`typeof(T).Name`) |
| `PerformedBy` | string | User who performed the action |
| `Timestamp` | DateTime | UTC timestamp |
| `Description` | string | Human-readable description |
| `Data` | string | JSON-serialized payload (if provided) |

### Incident Report (`iso9001-incidents`)

| Field | Type | Description |
|---|---|---|
| `Reference` | string | Entity/operation identifier |
| `CompanyId` | string | Company identifier from options |
| `ReportedAt` | DateTime | UTC timestamp |
| `UserId` | string | User involved |
| `Description` | string | Error description or exception message |
| `AffectedProcess` | string | Name of the process type (`typeof(T).Name`) |
| `Severity` | string | Severity level (e.g. `"Error"`) |
| `Data` | string | JSON-serialized payload (if provided) |
| `Exception` | string | Full exception string (if applicable) |

### Customer Feedback (`iso9001-feedbacks`)

| Field | Type | Description |
|---|---|---|
| `EntityId` | string | Identifier of the rated entity (e.g. order ID) |
| `CompanyId` | string | Company identifier from options |
| `CustomerId` | string | Customer identifier |
| `CustomerName` | string | Customer full name |
| `CustomerEmail` | string | Customer email |
| `CustomerAntiPhishing` | string | Anti-phishing code for the customer |
| `Rating` | int | Numeric rating |
| `Comments` | string | Customer comments |
| `ReportedAt` | DateTime | UTC timestamp |

### Non-Conformity (`iso9001-nonconformities`)

| Field | Type | Description |
|---|---|---|
| `EntityId` | string | Identifier of the affected entity |
| `CompanyId` | string | Company identifier from options |
| `ReportedAt` | DateTime | UTC timestamp |
| `ReportedBy` | string | Who reported the non-conformity |
| `Description` | string | Description of the non-conformity |
| `AffectedProcess` | string | Process name affected |
| `Cause` | string | Root cause |
| `Status` | string | Current status |

## Error handling

All methods catch internal exceptions silently and fall back to logging via `ILogger`. This means publishing failures never propagate to your application — the operation is best-effort. Ensure your Azure Storage connection string is valid to avoid silent data loss.

## License

See [LICENSE.txt](LICENSE.txt).
