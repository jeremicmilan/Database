using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Database
{
    class DatabaseStarter
    {
        public void StartUp()
        {
            DatabaseService databaseService = new DatabaseService();

            while (true)
            {
                Console.WriteLine("Database starter loop initiated");

                Console.WriteLine("Starting up database service...");
                databaseService.StartUpAsProcess();

                Console.WriteLine("Waiting for database service to stop...");
                databaseService.Process.WaitForExit();
                Console.WriteLine("Database service exited.");

                Thread.Sleep(TimeSpan.FromSeconds(1));
            }
        }
    }
}
