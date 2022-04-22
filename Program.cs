using System;
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
                Console.Title = "DatabaseClient";

                DatabaseClient databaseClient = DatabaseClient.Create();
                databaseClient.Start<OrchestratorTraditional>();
            }
            else
            {
                // Debugger.Launch();

                try
                {
                    ServiceConfiguration serviceConfiguration = ServiceConfiguration.Deserialize(args[0]);
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
