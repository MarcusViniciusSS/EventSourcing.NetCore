using DataAnalytics.Core.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAnalytics.Core
{
    public static class Configuration
    {
        public static IServiceCollection AddCoreServices(
            this IServiceCollection services,
            IConfiguration configuration
        ) =>
            services
                .AddEventBus()
                .AddEventStoreDB(configuration);
    }
}
