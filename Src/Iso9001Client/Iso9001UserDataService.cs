namespace Iso9001Client;

internal sealed class Iso9001UserDataService(
    Iso9001QueuePublisher publisher,
    IOptions<Iso9001ClientOptions> options) : IIso9001UserData
{
    public Task RequestUserData(string[] identifiers, string receiverName, string receiverEmail,
        string language, string receiverAntiPhishing = "")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(receiverEmail);
        List<string> ids = (identifiers ?? [])
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (ids.Count == 0)
            throw new ArgumentException("At least one identifier is required.", nameof(identifiers));
        return publisher.PublishUserDataRequest(new UserDataQueueMessage(
            CompanyId: options.Value.CompanyId,
            EmailCompanyId: options.Value.EmailCompanyId,
            CompanyName: string.IsNullOrWhiteSpace(options.Value.CompanyName) ? options.Value.CompanyId : options.Value.CompanyName,
            Identifiers: ids,
            ReceiverName: receiverName ?? string.Empty,
            ReceiverEmail: receiverEmail,
            ReceiverAntiPhishing: receiverAntiPhishing ?? string.Empty,
            Language: string.IsNullOrWhiteSpace(language) ? "en" : language));
    }
}
