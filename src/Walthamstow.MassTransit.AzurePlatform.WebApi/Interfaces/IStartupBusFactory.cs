using MassTransit.ExtensionsDependencyInjectionIntegration;

namespace Walthamstow.MassTransit.AzurePlatform.WebApi.Interfaces
{
    public interface IStartupBusFactory
    {
        void CreateBus(IServiceCollectionBusConfigurator busConfigurator, IStartupBusConfigurator configurator);
    }
}