namespace Microsoft.Extensions.DependencyInjection;

public static class Iso9001ClientExtensions
{
    /// <summary>
    /// Registers the full ISO9001 client: publisher + Iso9001Service as IIso9001.
    /// Use for projects that consume the library's IIso9001 interface directly (e.g. CentralBillingService).
    /// </summary>
    public static IServiceCollection AddIso9001Client(
        this IServiceCollection services,
        Action<Iso9001ClientOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<Iso9001QueuePublisher>();
        services.AddScoped<IIso9001, Iso9001Service>();
        services.AddScoped<IIso9001UserData, Iso9001UserDataService>();
        return services;
    }
}
