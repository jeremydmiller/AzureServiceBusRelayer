namespace ServiceBusProxy
{
    public class AzureServiceBusSettings
    {
        public string AzureServiceBusConnectionString { get; set; }
        public string SqlServerConnectionString { get; set; }
        public string AcknowledgementTopic { get; set; }
        
    }
}