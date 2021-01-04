using AltV.Net.DependencyInjection.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace AltV.Net.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddScriptService<T>(this IServiceCollection services) where T : class, IScriptService
        {
            return services.AddTransient<IScriptService, T>();
        }
    }
}