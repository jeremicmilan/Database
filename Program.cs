using Database.Database;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Database
{
    class Program
    {
        static void Main (string[] args)
        {
            Console.WriteLine("Argument count: " + args.Count());

            if (args.Count() == 0)
            {
                DatabaseStarter databaseStarter = new DatabaseStarter();
                databaseStarter.StartUp();
            }
            else
            {
#if (DEBUG)
                Debugger.Launch();
#endif

                string type = args[0];
                Console.WriteLine("Process type: " + type);
                Service service = (Service)Activator.CreateInstance(Type.GetType(type));

                Console.WriteLine("Starting service...");
                service.StartUp();
                Console.WriteLine("Service ended.");

                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }
    }
}
