using MassTransit;

namespace Walthamstow.MassTransit.AzurePlatform.WebApi.Interfaces
{
    public interface IStartupBusConfigurator
    {
        void ConfigureBus<TEndpointConfigurator>(IBusFactoryConfigurator<TEndpointConfigurator> configurator, IBusRegistrationContext context)
            where TEndpointConfigurator : IReceiveEndpointConfigurator;
    }
}