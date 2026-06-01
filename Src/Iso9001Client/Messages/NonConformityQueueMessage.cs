namespace Iso9001Client.Messages;

public record NonConformityQueueMessage(
    string EntityId,
    string CompanyId,
    DateTime ReportedAt,
    string ReportedBy,
    string Description,
    string AffectedProcess,
    string Cause,
    string Status);
