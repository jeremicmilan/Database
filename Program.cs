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
            Console.WriteLine("Argument count: " + args.Count());

            if (args.Count() == 0)
            {
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
                    string type = args[0];
                    Console.WriteLine("Process type: " + type);
                    Service service = (Service)Activator.CreateInstance(Type.GetType(type));

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
