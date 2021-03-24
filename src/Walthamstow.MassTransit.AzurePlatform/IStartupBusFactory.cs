using MassTransit.ExtensionsDependencyInjectionIntegration;

namespace Walthamstow.MassTransit.AzurePlatform
{
    public interface IStartupBusFactory
    {
        void CreateBus(IServiceCollectionBusConfigurator busConfigurator, IStartupBusConfigurator configurator);
    }
}