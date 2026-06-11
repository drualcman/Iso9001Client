namespace Iso9001Client.Messages;

public record UserDataQueueMessage(
    string CompanyId,
    string CompanyName,
    IReadOnlyList<string> Identifiers,
    int EmailCompanyId,
    string ReceiverName,
    string ReceiverEmail,
    string ReceiverAntiPhishing,
    string Language);
