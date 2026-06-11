namespace Iso9001Client;

internal class Iso9001Service(
    Iso9001QueuePublisher publisher,
    IOptions<Iso9001ClientOptions> options,
    ILogger<Iso9001Service> logger) : IIso9001
{
    private string CompanyId => options.Value.CompanyId;

    public async Task Register<T, TData>(string reference, T action, string description, TData data)
    {
        try
        {
            await publisher.PublishAuditLog(new AuditLogQueueMessage(
                Reference: reference,
                CompanyId: CompanyId,
                Action: typeof(T).Name,
                PerformedBy: string.Empty,
                Timestamp: DateTime.UtcNow,
                Description: description,
                Data: JsonSerializer.Serialize(data)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{reference}] {action}: {description}", reference, typeof(T).Name, description);
            logger.LogError(JsonSerializer.Serialize(data));
        }
    }

    public async Task Register<T>(string reference, T action, string description)
    {
        try
        {
            await publisher.PublishAuditLog(new AuditLogQueueMessage(
                Reference: reference,
                CompanyId: CompanyId,
                Action: typeof(T).Name,
                PerformedBy: string.Empty,
                Timestamp: DateTime.UtcNow,
                Description: description,
                Data: string.Empty));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[{reference}] {action}: {description}", reference, typeof(T).Name, description);
        }
    }

    public async Task Error<T>(string reference, T action, Exception ex)
    {
        ex.Data["IsLogged"] = true;
        try
        {
            await publisher.PublishIncident(new IncidentReportQueueMessage(
                Reference: reference,
                CompanyId: CompanyId,
                ReportedAt: DateTime.UtcNow,
                UserId: string.Empty,
                Description: ex.Message,
                AffectedProcess: typeof(T).Name,
                Severity: "Error",
                Data: string.Empty,
                Exception: ex.ToString()));
        }
        catch (Exception internalEx)
        {
            logger.LogError(internalEx, "[{reference}] {action}: {description}", reference, typeof(T).Name, ex.Message);
        }
    }

    public async Task Error<T>(string reference, T action, string description)
    {
        try
        {
            await publisher.PublishIncident(new IncidentReportQueueMessage(
                Reference: reference,
                CompanyId: CompanyId,
                ReportedAt: DateTime.UtcNow,
                UserId: string.Empty,
                Description: description,
                AffectedProcess: typeof(T).Name,
                Severity: "Error",
                Data: string.Empty,
                Exception: string.Empty));
        }
        catch (Exception ex)
        {
            ex.Data["IsLogged"] = true;
            logger.LogError(ex, "[{reference}] {action}: {description}", reference, typeof(T).Name, description);
        }
    }

    public async Task Error<T, TData>(string reference, T action, string description, TData data)
    {
        try
        {
            await publisher.PublishIncident(new IncidentReportQueueMessage(
                Reference: reference,
                CompanyId: CompanyId,
                ReportedAt: DateTime.UtcNow,
                UserId: string.Empty,
                Description: description,
                AffectedProcess: typeof(T).Name,
                Severity: "Error",
                Data: JsonSerializer.Serialize(data),
                Exception: string.Empty));
        }
        catch (Exception ex)
        {
            ex.Data["IsLogged"] = true;
            logger.LogError(ex, "[{reference}] {action}: {description}", reference, typeof(T).Name, description);
            logger.LogError(JsonSerializer.Serialize(data));
        }
    }

    public async Task RegisterFeedback(string entityId, string customerId, string customerName,
        string customerEmail, string customerAntiPhishing, int rating, string comments,
        string language)
    {
        try
        {
            await publisher.PublishFeedback(new CustomerFeedbackQueueMessage(
                EntityId: entityId ?? string.Empty,
                CompanyId: CompanyId,
                EmailCompanyId: options.Value.EmailCompanyId,
                CustomerId: customerId ?? string.Empty,
                CustomerName: customerName ?? string.Empty,
                CustomerEmail: customerEmail ?? string.Empty,
                CustomerAntiPhishing: customerAntiPhishing ?? string.Empty,
                Rating: rating,
                Comments: comments ?? string.Empty,
                ReportedAt: DateTime.UtcNow,
                Language: string.IsNullOrWhiteSpace(language) ? "en" : language));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish feedback for customer {CustomerId}", customerId);
        }
    }
}
