using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Definition;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Walthamstow.MassTransit.AzurePlatform.Configs;
using Walthamstow.MassTransit.AzurePlatform.WebApi.Implementations;
using Walthamstow.MassTransit.AzurePlatform.WebApi.Interfaces;

namespace Walthamstow.MassTransit.AzurePlatform.WebApi
{
    public class MassTransitStartup
    {
        public MassTransitStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Information("Configuring MassTransit Services");
            services.AddHealthChecks();
            services.Configure<PlatformOptions>(Configuration.GetSection("Platform"));

            ServiceBusStartupBusFactory.Configure(services, Configuration);

            var configurationServiceProvider = services.BuildServiceProvider();
            
            services.ConfigureSagaDbs(Configuration);
            
            List<IWebApiPlatformStartup> platformStartups = configurationServiceProvider.
                GetService<IEnumerable<IWebApiPlatformStartup>>()?.ToList();

            ConfigureApplicationInsights(services);

            services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
            services.AddMassTransit(cfg =>
            {
                foreach (var platformStartup in platformStartups)
                    platformStartup.ConfigurePlatform(cfg, services, Configuration);

                CreateBus(cfg, configurationServiceProvider);
            });

            services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Delay = TimeSpan.FromSeconds(2);
            });
            
            services.AddMassTransitHostedService();
        }

        void ConfigureApplicationInsights(IServiceCollection services)
        {
            if (string.IsNullOrWhiteSpace(Configuration.GetSection("ApplicationInsights")?.GetValue<string>("InstrumentationKey")))
                return;

            Log.Information("Configuring Application Insights");

            services.AddApplicationInsightsTelemetry();

            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.IncludeDiagnosticSourceActivities.Add("MassTransit");
            });
        }

        void CreateBus(IServiceCollectionBusConfigurator busConfigurator, IServiceProvider provider)
        {
            var platformOptions = provider.GetRequiredService<IOptions<PlatformOptions>>().Value;
            var configurator = new StartupBusConfigurator();
            switch (platformOptions.Transport.ToLower(CultureInfo.InvariantCulture))
            {
                case PlatformOptions.AzureServiceBus:
                case PlatformOptions.ASB:
                    new ServiceBusStartupBusFactory().CreateBus(busConfigurator, configurator);
                    break;
                
                case PlatformOptions.Mediator:
                    break;
                default:
                    throw new ConfigurationException($"Unknown transport type: {platformOptions.Transport}");
            }
        }
        
        private static void SetupAzureServiceBus(IServiceProvider provider, IServiceCollectionBusConfigurator cfg,
            List<IWebApiPlatformStartup> platformStartups)
        {
            if (!IsUsingAzureServiceBus(provider))
                return;

            cfg.UsingAzureServiceBus((context, configure) =>
            {
                var options = context.GetRequiredService<IOptions<ServiceBusOptions>>().Value;
                if (string.IsNullOrWhiteSpace(options.ConnectionString))
                    throw new ConfigurationException("The Azure Service Bus ConnectionString must not be empty.");
                
                configure.Host(options.ConnectionString);
                configure.UseHealthCheck(context);
                
                if (platformStartups != null)
                    foreach (var platformStartup in platformStartups)
                        platformStartup.ConfigureBus(configure, context);

                configure.ConfigureEndpoints(context);
            });
        }
        
        private static bool IsUsingAzureServiceBus(IServiceProvider provider)
        {
            var platformOptions = provider.GetRequiredService<IOptions<PlatformOptions>>().Value;
            var transport = platformOptions.Transport.ToLower(CultureInfo.InvariantCulture);
            return transport != PlatformOptions.AzureServiceBus &&
                   transport != PlatformOptions.ASB;
        }
        
        public void Configure(IApplicationBuilder app)
        {
            // here we execute our own startup

            var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            List<IWebApiPlatformStartup> platformStartups = app.ApplicationServices.GetRequiredService<IEnumerable<IWebApiPlatformStartup>>()?.ToList();
            foreach (var platformStartup in platformStartups)
                platformStartup.Configure(app,env);

            app.UseHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = HealthCheckResponseWriter
            });
            app.UseHealthChecks("/health/live", new HealthCheckOptions {ResponseWriter = HealthCheckResponseWriter});

        }

        static Task HealthCheckResponseWriter(HttpContext context, HealthReport result)
        {
            var json = new JObject(
                new JProperty("status", result.Status.ToString()),
                new JProperty("results", new JObject(result.Entries.Select(entry => new JProperty(entry.Key, new JObject(
                    new JProperty("status", entry.Value.Status.ToString()),
                    new JProperty("description", entry.Value.Description),
                    new JProperty("data", JObject.FromObject(entry.Value.Data))))))));

            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(json.ToString(Formatting.Indented));
        }
    }

}