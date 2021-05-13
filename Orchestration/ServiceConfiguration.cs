using System;
using System.Collections.Generic;
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

        public ServiceConfiguration() { }

        public ServiceConfiguration(
            string serviceType,
            string logFilePath = null)
        {
            ServiceType = serviceType;
            LogFilePath = logFilePath;
        }
    }
}
