using Microsoft.Extensions.DependencyInjection;

namespace AltV.Net.DependencyInjection.Contracts
{
    public interface IStartup
    {
        void ConfigureServices(IServiceCollection services);
    }
}