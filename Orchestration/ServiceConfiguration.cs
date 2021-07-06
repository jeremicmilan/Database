using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Database
{
    public class ServiceConfiguration
    {
        [XmlElement]
        public string ServiceType;

        [XmlElement]
        public string LogFilePath;

        [XmlElement]
        public string DataFilePath;

        [XmlElement]
        public bool LoggingEnabled = true;

        public ServiceConfiguration() { }

        public ServiceConfiguration(
            string serviceType)
        {
            ServiceType = serviceType;
        }

        public void Override(ServiceConfiguration serviceConfiguration)
        {
            ServiceType = serviceConfiguration.ServiceType ?? ServiceType;
            LogFilePath = serviceConfiguration.LogFilePath ?? LogFilePath;
            DataFilePath = serviceConfiguration.DataFilePath ?? DataFilePath;
            LoggingEnabled = serviceConfiguration.LoggingEnabled;
        }

        public string Serialize()
        {
            XmlSerializer serviceConfigurationSerializer = new XmlSerializer(typeof(ServiceConfiguration));
            StringWriter stringWriter = new StringWriter();
            serviceConfigurationSerializer.Serialize(stringWriter, this);
            return new string(stringWriter.ToString().Where(c => !Environment.NewLine.Contains(c)).ToArray());
        }

        public static ServiceConfiguration Deserialize(string serviceConfigurationString)
        {
            XmlSerializer serviceConfigurationSerializer = new XmlSerializer(typeof(ServiceConfiguration));
            return (ServiceConfiguration)serviceConfigurationSerializer.Deserialize(new StringReader(serviceConfigurationString));
        }
    }
}
