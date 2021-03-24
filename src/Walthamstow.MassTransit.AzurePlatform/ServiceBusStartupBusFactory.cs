using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Walthamstow.MassTransit.AzurePlatform.Configs;

namespace Walthamstow.MassTransit.AzurePlatform
{
    public class ServiceBusStartupBusFactory :
        IStartupBusFactory
    {
        public void CreateBus(IServiceCollectionBusConfigurator busConfigurator, IStartupBusConfigurator configurator)
        {
            busConfigurator.UsingAzureServiceBus((context, cfg) =>
            {
                var options = context.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
                if (string.IsNullOrWhiteSpace(options.ConnectionString))
                    throw new ConfigurationException("The Azure Service Bus ConnectionString must not be empty.");

                cfg.Host(options.ConnectionString);
                configurator.ConfigureBus(cfg, context);
            });
        }

        public static void Configure(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ServiceBusOptions>(configuration.GetSection("ASB"));
        }
    }
}