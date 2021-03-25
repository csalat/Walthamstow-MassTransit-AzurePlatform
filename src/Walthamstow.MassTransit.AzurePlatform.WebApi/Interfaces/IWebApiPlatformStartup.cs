using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Walthamstow.MassTransit.AzurePlatform.WebApi.Interfaces
{
    public interface IWebApiPlatformStartup
    {
        void ConfigurePlatform(IServiceCollectionBusConfigurator configurator, IServiceCollection services,
            IConfiguration configuration);

        void ConfigureBus<TEndpointConfigurator>(IBusFactoryConfigurator<TEndpointConfigurator> configurator, IBusRegistrationContext context)
            where TEndpointConfigurator : IReceiveEndpointConfigurator;

        void Configure(IApplicationBuilder app, IWebHostEnvironment env);
        
    }
}