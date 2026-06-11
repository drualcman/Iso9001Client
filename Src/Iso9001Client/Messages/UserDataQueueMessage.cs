namespace Iso9001Client.Messages;

public record UserDataQueueMessage(
    string CompanyId,
    string CompanyName,
    IReadOnlyList<string> Identifiers,
    string ReceiverName,
    string ReceiverEmail,
    string ReceiverAntiPhishing,
    string Language);
