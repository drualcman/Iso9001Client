namespace Iso9001Client;

internal sealed class Iso9001QueuePublisher(IOptions<Iso9001ClientOptions> options)
{
    // Azure Storage Queue limit: 65536 bytes for the HTTP request body.
    // The SDK wraps the base64 message in XML (~96 bytes overhead), so max base64 = 65440 chars,
    // which means max raw JSON = floor(65440 * 3/4) = 49080 bytes.
    // Using 47 KB (48128 bytes) → base64 ≈ 64172 chars → total body ≈ 64268 bytes, safely under limit.
    private const int MaxJsonBytes = 47 * 1024;
    private const string TruncationSuffix = "...[truncated]";

    public Task PublishAuditLog(AuditLogQueueMessage message)
        => PublishToQueue(options.Value.AuditLogQueue, message);

    public Task PublishIncident(IncidentReportQueueMessage message)
        => PublishToQueue(options.Value.IncidentQueue, message);

    public Task PublishFeedback(CustomerFeedbackQueueMessage message)
        => PublishToQueue(options.Value.FeedbackQueue, message);

    public Task PublishNonConformity(NonConformityQueueMessage message)
        => PublishToQueue(options.Value.NonConformityQueue, message);

    private async Task PublishToQueue<T>(string queueName, T message)
    {
        QueueClient client = new QueueClient(options.Value.ConnectionString, queueName);
        await client.CreateIfNotExistsAsync();
        string json = JsonSerializer.Serialize(message);
        if (Encoding.UTF8.GetByteCount(json) > MaxJsonBytes)
        {
            json = FitToQueueLimit(json);
            // Safety net: JSON escaping overhead can push the output slightly over budget.
            // If still too large, hard-replace large fields with just the truncation marker.
            if (Encoding.UTF8.GetByteCount(json) > MaxJsonBytes)
                json = HardTruncateFields(json);
        }
        await client.SendMessageAsync(json);
    }

    private static string HardTruncateFields(string json)
    {
        using var doc = JsonDocument.Parse(json);
        using var ms = new System.IO.MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteStartObject();
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            if (prop.Name is "Data" or "Exception")
                writer.WriteString(prop.Name, TruncationSuffix);
            else
                prop.WriteTo(writer);
        }
        writer.WriteEndObject();
        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static string FitToQueueLimit(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // 1. Extract the two fields that can grow unboundedly.
        string? dataValue = TryGetStringField(root, "Data");
        string? exceptionValue = TryGetStringField(root, "Exception");

        // 2. Measure the base payload without those fields.
        int baseSize = MeasureJsonWithout(root, "Data", "Exception");

        // 3. How many bytes are left for the large fields?
        //    Subtract the JSON structural overhead for each present field
        //    ("FieldName":"" + comma ≈ 10–17 bytes) and a 15 % margin for
        //    JSON string-escaping of the value itself (e.g. embedded quotes → \").
        bool hasData = dataValue is not null;
        bool hasException = exceptionValue is not null;

        int dataOverhead = "\"Data\":\"\",".Length;         // 10 bytes
        int exceptionOverhead = "\"Exception\":\"\",".Length; // 16 bytes

        int structureOverhead = (hasData ? dataOverhead : 0) + (hasException ? exceptionOverhead : 0);
        int available = (int)((MaxJsonBytes - baseSize - structureOverhead) * 0.85);

        // 4. Distribute the budget and truncate.
        string? newData = null;
        string? newException = null;

        if (hasData && hasException)
        {
            // Split 50/50 so both fields remain partially readable.
            int half = available / 2;
            newData = TruncateToBytes(dataValue!, half);
            newException = TruncateToBytes(exceptionValue!, half);
        }
        else if (hasData)
        {
            newData = TruncateToBytes(dataValue!, available);
        }
        else if (hasException)
        {
            newException = TruncateToBytes(exceptionValue!, available);
        }

        // 5. Rebuild the JSON, injecting the truncated values in the original field order.
        using var ms = new System.IO.MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteStartObject();
        foreach (var prop in root.EnumerateObject())
        {
            if (prop.Name == "Data" && hasData)
                writer.WriteString("Data", newData!);
            else if (prop.Name == "Exception" && hasException)
                writer.WriteString("Exception", newException!);
            else
                prop.WriteTo(writer);
        }
        writer.WriteEndObject();
        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static string? TryGetStringField(JsonElement root, string name)
        => root.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String
            ? prop.GetString() : null;

    private static int MeasureJsonWithout(JsonElement root, params string[] skip)
    {
        using var ms = new System.IO.MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteStartObject();
        foreach (var prop in root.EnumerateObject())
            if (!skip.Contains(prop.Name)) prop.WriteTo(writer);
        writer.WriteEndObject();
        writer.Flush();
        return (int)ms.Length;
    }

    private static string TruncateToBytes(string value, int maxBytes)
    {
        if (maxBytes <= 0) return TruncationSuffix;

        byte[] suffixBytes = Encoding.UTF8.GetBytes(TruncationSuffix);
        int contentBudget = maxBytes - suffixBytes.Length;

        byte[] valueBytes = Encoding.UTF8.GetBytes(value);
        if (valueBytes.Length <= maxBytes)
            return value; // fits as-is, no truncation needed

        if (contentBudget <= 0)
            return TruncationSuffix;

        // Walk forward char-by-char to stay on a valid UTF-8 boundary.
        int byteCount = 0, charIndex = 0;
        while (charIndex < value.Length)
        {
            int charBytes = Encoding.UTF8.GetByteCount(value, charIndex, 1);
            if (byteCount + charBytes > contentBudget) break;
            byteCount += charBytes;
            charIndex++;
        }
        return value[..charIndex] + TruncationSuffix;
    }
}
