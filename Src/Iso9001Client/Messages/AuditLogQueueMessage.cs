namespace Iso9001Client.Messages;

public record AuditLogQueueMessage(
    string Reference,
    string CompanyId,
    string Action,
    string PerformedBy,
    DateTime Timestamp,
    string Description,
    string Data);
