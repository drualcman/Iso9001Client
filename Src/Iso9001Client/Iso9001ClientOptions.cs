namespace Iso9001Client;

public class Iso9001ClientOptions
{
    public const string SectionKey = "Iso9001ClientOptions";

    public string ConnectionString { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public int EmailCompanyId { get; set; } = 0;
    /// <summary>Display name shown to users in ISO9001 emails (subject tag and signature). Falls back to CompanyId when empty.</summary>
    public string CompanyName { get; set; } = string.Empty;
    public string AuditLogQueue { get; set; } = Iso9001QueueNames.AuditLogs;
    public string IncidentQueue { get; set; } = Iso9001QueueNames.Incidents;
    public string FeedbackQueue { get; set; } = Iso9001QueueNames.CustomerFeedbacks;
    public string NonConformityQueue { get; set; } = Iso9001QueueNames.NonConformities;
    public string UserDataQueue { get; set; } = Iso9001QueueNames.UserData;
}
