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
                Console.Title = "DatabaseClient";
                Utility.SetShouldTrace();

                Utility.CleanupFiles();
                Utility.CleanupTraces();

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

                    service.SnapWindow();
                    Utility.LoadTracesToConsoleLog();
                    service.Start();
                    Utility.LogServiceEnd("Service ended.");
                }
                catch (Exception exception)
                {
                    Utility.LogFailure(exception.ToString());
                }

                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }
    }
}
