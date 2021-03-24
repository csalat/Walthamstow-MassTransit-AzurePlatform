using MassTransit.ExtensionsDependencyInjectionIntegration;

namespace Walthamstow.MassTransit.AzurePlatform.Startup
{
    public interface IStartupBusFactory
    {
        void CreateBus(IServiceCollectionBusConfigurator busConfigurator, IStartupBusConfigurator configurator);
    }
}