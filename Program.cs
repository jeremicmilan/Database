﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
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
                    ServiceConfiguration serviceConfiguration = JsonSerializer.Deserialize<ServiceConfiguration>(args[0]);
                    Type type = Type.GetType(serviceConfiguration.ServiceType);
                    Console.Title = type.Name;
                    object o = Activator.CreateInstance(type, serviceConfiguration);
                    dynamic service = Convert.ChangeType(o, type);

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
