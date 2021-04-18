using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Database.Database
{
    class DatabaseStarter
    {
        public void StartUp ()
        {
            while (true)
            {
                Console.WriteLine("Database starter loop initiated");

                Console.WriteLine("Starting up database service...");
                Database databaseService = new Database();
                databaseService.StartUpAsProcess();

                Console.WriteLine("Waiting for database service to stop...");
                databaseService.Process.WaitForExit();
                Console.WriteLine("Database service exited.");

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
