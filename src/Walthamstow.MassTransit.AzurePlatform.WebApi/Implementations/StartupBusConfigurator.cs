using System.Collections.Generic;
using System.Linq;
using MassTransit;
using Walthamstow.MassTransit.AzurePlatform.Configs;
using Walthamstow.MassTransit.AzurePlatform.WebApi.Interfaces;

namespace Walthamstow.MassTransit.AzurePlatform.WebApi.Implementations
{
    public class StartupBusConfigurator :
        IStartupBusConfigurator
    {
        public void ConfigureBus<TEndpointConfigurator>(IBusFactoryConfigurator<TEndpointConfigurator> configurator, IBusRegistrationContext context)
            where TEndpointConfigurator : IReceiveEndpointConfigurator
        {
            configurator.UseHealthCheck(context);

            List<IWebApiPlatformStartup> hostingConfigurators = context.GetService<IEnumerable<IWebApiPlatformStartup>>()?.ToList();

            foreach (var hostingConfigurator in hostingConfigurators)
                hostingConfigurator.ConfigureBus(configurator, context);

            configurator.ConfigureEndpoints(context);
        }
    }
}