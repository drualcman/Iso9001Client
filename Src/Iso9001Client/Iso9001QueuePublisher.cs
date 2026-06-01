namespace Iso9001Client;

internal sealed class Iso9001QueuePublisher(IOptions<Iso9001ClientOptions> options)
{
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
        await client.SendMessageAsync(Convert.ToBase64String(Encoding.UTF8.GetBytes(json)));
    }
}
