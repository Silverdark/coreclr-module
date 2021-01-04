using Microsoft.Extensions.DependencyInjection;

namespace AltV.Net.Async
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseAltAsync(this IServiceCollection services)
        {
            return services.AddSingleton<IServerModule, AsyncModule>();
        }
    }
}