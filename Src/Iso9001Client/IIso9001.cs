namespace Iso9001Client;

public interface IIso9001
{
    Task Error<T, TData>(string reference, T action, string description, TData data);
    Task Error<T>(string reference, T action, Exception ex);
    Task Error<T>(string reference, T action, string description);
    Task Register<T, TData>(string reference, T action, string description, TData data);
    Task Register<T>(string reference, T action, string description);
    Task RegisterFeedback(string entityId, string customerId, string customerName,
        string customerEmail, string customerAntiPhishing, int rating, string comments);
}
