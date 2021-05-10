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
                Console.Title = "DatabaseStarter";
                Window.SnapLeft(Process.GetCurrentProcess());

                DatabaseStarter databaseStarter = new DatabaseStarter();
                databaseStarter.StartUp();
            }
            else
            {
#if (DEBUG)
                Debugger.Launch();
#endif

                Window.SnapRight(Process.GetCurrentProcess());

                try
                {
                    Type type = Type.GetType(args[0]);
                    Console.Title = type.Name;
                    Service service = (Service)Activator.CreateInstance(type);

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
