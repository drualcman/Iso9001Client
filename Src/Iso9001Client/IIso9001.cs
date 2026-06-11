namespace Iso9001Client;

public interface IIso9001
{
    Task Error<T, TData>(string reference, T action, string description, TData data);
    Task Error<T>(string reference, T action, Exception ex);
    Task Error<T>(string reference, T action, string description);
    Task Register<T, TData>(string reference, T action, string description, TData data);
    Task Register<T>(string reference, T action, string description);
    Task RegisterFeedback(string entityId, string customerId, string customerName,
        string customerEmail, string customerAntiPhishing, int rating, string comments,
        string language);
}

/// <summary>
/// Requests user-data exports from the ISO9001 system. Kept separate from <see cref="IIso9001"/>
/// so hosts that override the fire-and-forget logging implementation still reach the real system.
/// </summary>
public interface IIso9001UserData
{
    /// <summary>
    /// Asks the ISO9001 system to collect all data it holds about a subject (audit logs, incidents
    /// and feedback, matched by any of the given identifiers — e.g. user id, email) and email it
    /// directly to the receiver, in the given language (en, es, fil; defaults to en).
    /// Throws if the request cannot be queued.
    /// </summary>
    Task RequestUserData(string[] identifiers, string receiverName, string receiverEmail,
        string language, string receiverAntiPhishing = "");
}
