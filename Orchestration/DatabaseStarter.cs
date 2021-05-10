﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Database
{
    class DatabaseStarter
    {
        DatabaseService DatabaseService = null;
        CancellationTokenSource KeepDatabaseServiceUpThreadCancellationTokenSource;

        private void KeepDatabaseServiceUp ()
        {
            Thread.CurrentThread.Name = MethodBase.GetCurrentMethod().Name;

            DatabaseService = new DatabaseService();
            Console.WriteLine("Database service created");

            while (true)
            {
                Console.WriteLine("Starting up database service...");
                DatabaseService.StartUpAsProcess();

                Console.WriteLine("Waiting for database service to stop...");
                DatabaseService.Process.WaitForExit();
                Console.WriteLine("Database service exited.");

                if (KeepDatabaseServiceUpThreadCancellationTokenSource != null &&
                    KeepDatabaseServiceUpThreadCancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }

        public void StartUp()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => KillStartedProcesses();

            StartProcesses();

            // Block on user input
            //
            Service.RegisterPipeClient(DatabaseService.DatabasePipeName, ProcessUserInput);
        }

        private void StartProcesses()
        {
            KeepDatabaseServiceUpThreadCancellationTokenSource = new CancellationTokenSource();
            new Thread(KeepDatabaseServiceUp).Start();
        }

        private void KillStartedProcesses()
        {
            KeepDatabaseServiceUpThreadCancellationTokenSource.Cancel();
            DatabaseService?.Process?.Kill();
        }

        private void ProcessUserInput(Action<string> sendMessageToDatabase)
        {
            while (true)
            {
                string line = Console.ReadLine();

                switch (line.Trim())
                {
                    case "EXIT":
                        return;

                    case "KILL":
                        DatabaseService.Process.Kill();
                        return;

                    case "":
                        break;

                    default:
                        sendMessageToDatabase(line);
                        break;
                }
            }
        }
    }
}
