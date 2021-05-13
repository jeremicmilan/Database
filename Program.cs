using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace Database
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() == 0)
            {
                Console.Title = "DatabaseStarter";
                Window.SnapLeft(Process.GetCurrentProcess());

                DatabaseStarter databaseStarter = new DatabaseStarter();
                databaseStarter.StartUp();
            }
            else
            {
                //Debugger.Launch();

                Window.SnapRight(Process.GetCurrentProcess());

                try
                {
                    XmlSerializer serviceConfigurationSerializer = new XmlSerializer(typeof(ServiceConfiguration));
                    ServiceConfiguration serviceConfiguration = (ServiceConfiguration)serviceConfigurationSerializer.Deserialize(new StringReader(args[0]));
                    Type type = Type.GetType(serviceConfiguration.ServiceType);
                    Console.Title = type.Name;
                    Service service = (Service)Activator.CreateInstance(type, serviceConfiguration);

                    Console.WriteLine("Starting service...");
                    service.StartUp();
                    Console.WriteLine("Service ended.");
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.ToString());
                }

                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }
    }
}
