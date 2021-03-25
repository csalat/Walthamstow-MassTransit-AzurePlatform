using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Walthamstow.MassTransit.AzurePlatform.WebApi
{
    public static class MassTransitHost
    {
        public static IHostBuilder CreateBuilder(string[] args)
        {
            var builder = new HostBuilder();

            var currentDirectory = Directory.GetCurrentDirectory();
            builder.UseContentRoot(currentDirectory);
            builder.ConfigureHostConfiguration(config =>
            {
                var baseSettingsPath = Path.Combine(currentDirectory, "appsettings.json");
                config.AddJsonFile(baseSettingsPath, true, true);
                config.AddEnvironmentVariables();

                if (args != null)
                    config.AddCommandLine(args);
            });

            builder.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    var envSettingsPath = Path.Combine(currentDirectory, $"appsettings.{env.EnvironmentName}.json");
                    config.AddJsonFile(envSettingsPath, true, true);
                })
                .UseSerilog()
                .UseDefaultServiceProvider((context, options) =>
                {
                    var isDevelopment = context.HostingEnvironment.IsDevelopment();
                    options.ValidateScopes = isDevelopment;
                    options.ValidateOnBuild = isDevelopment;
                });

            return builder;
        }
    }
}