namespace Database
{
    public class ServiceConfiguration
    {
        public string ServiceType { get; set; }

        public string LogFilePath { get; set; }

        public string DataFilePath { get; set; }

        public bool? LoggingDisabled { get; set; }
        public bool LoggingEnabled() => !(LoggingDisabled.HasValue && LoggingDisabled.Value);

        public ServiceConfiguration()
        { }

        public void Override(ServiceConfiguration serviceConfiguration)
        {
            ServiceType = serviceConfiguration.ServiceType ?? ServiceType;
            LogFilePath = serviceConfiguration.LogFilePath ?? LogFilePath;
            DataFilePath = serviceConfiguration.DataFilePath ?? DataFilePath;
            LoggingDisabled = serviceConfiguration.LoggingDisabled ?? LoggingDisabled;
        }
    }
}
