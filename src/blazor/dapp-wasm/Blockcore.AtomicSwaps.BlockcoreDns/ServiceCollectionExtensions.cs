using Microsoft.Extensions.DependencyInjection;

namespace Blockcore.AtomicSwaps.BlockcoreDns
{
    public static class ServiceCollectionExtensions
    {
        public static void AddBlockcoreDns(this IServiceCollection services)
        {
            services.AddScoped<IBlockcoreDnsService, BlockcoreDnsService>();
        }
    }
}
