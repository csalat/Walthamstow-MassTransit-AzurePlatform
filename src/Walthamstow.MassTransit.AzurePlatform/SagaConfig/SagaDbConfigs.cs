using System.Collections.Generic;

namespace Walthamstow.MassTransit.AzurePlatform.SagaConfig
{
    public class SagaDbConfigs
    {
        public List<SqlServerDbConfigOptions> SagaSqlServerOptions { get; set; }
    }
}