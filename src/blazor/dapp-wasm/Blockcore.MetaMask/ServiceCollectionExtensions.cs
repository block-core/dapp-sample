using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace Blockcore.AtomicSwaps.MetaMask
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMetaMask(this IServiceCollection services)
        {
            services.AddScoped<IMetaMaskService>(sp => new MetaMaskService(sp.GetRequiredService<IJSRuntime>()));
        }
    }
}
