namespace Walthamstow.MassTransit.AzurePlatform.SagaConfig
{
    public class SqlServerDbConfigOptions
    {
        public string SagaName { get; set; }

        public string ConnectionString { get; set; }
    }
}