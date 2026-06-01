namespace Iso9001Client.Messages;

public record IncidentReportQueueMessage(
    string Reference,
    string CompanyId,
    DateTime ReportedAt,
    string UserId,
    string Description,
    string AffectedProcess,
    string Severity,
    string Data,
    string Exception);
