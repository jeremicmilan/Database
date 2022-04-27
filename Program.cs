using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Database
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() == 0)
            {
                Utility.CleanupTraces();

                Console.Title = "DatabaseClient";

                DatabaseClient databaseClient = DatabaseClient.Create();
                databaseClient.Start<OrchestratorHyperscale>();
            }
            else
            {
                // Debugger.Launch();

                try
                {
                    ServiceConfiguration serviceConfiguration = Utility.Deserialize<ServiceConfiguration>(args[0]);
                    Type type = Type.GetType(serviceConfiguration.ServiceType);
                    Console.Title = type.Name;
                    Service service = (Service)Activator.CreateInstance(type, serviceConfiguration);

                    Utility.TraceDebugMessage("Starting service...");
                    service.SnapWindow();
                    service.Start();
                    Utility.TraceDebugMessage("Service ended.");
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
