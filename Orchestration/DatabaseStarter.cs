using Database.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Database
{
    class DatabaseStarter
    {

        private DatabaseStarter() { }

        private static DatabaseStarter _DatabaseStarter = null;
        public static DatabaseStarter Get() => _DatabaseStarter;

        public static DatabaseStarter Create()
        {
            if (_DatabaseStarter != null)
            {
                throw new Exception("There can be only one database starter.");
            }

            return _DatabaseStarter = new DatabaseStarter();
        }

        DatabaseService DatabaseService = null;
        CancellationTokenSource KeepDatabaseServiceUpThreadCancellationTokenSource;

        public void StartUp()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => KillStartedProcesses();

            StartProcesses();

            DatabaseService.RegisterPipeClient(DatabaseService.DatabasePipeName);

            WaitForUserInput();
        }

        private void StartProcesses()
        {
            KeepDatabaseServiceUpThreadCancellationTokenSource = new CancellationTokenSource();
            new Thread(KeepDatabaseServiceUp).Start();
            Utility.WaitUntil(() => DatabaseService != null);
        }

        private void KillStartedProcesses()
        {
            KeepDatabaseServiceUpThreadCancellationTokenSource.Cancel();
            DatabaseService?.Process?.Kill();
        }

        private void WaitForUserInput()
        {
            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();

                if (line == "EXIT")
                {
                    return;
                }

                try
                {
                    ProcessUserInput(line.Trim());
                }
                catch (Exception exception)
                {
                    Utility.LogFailure(exception.ToString());
                }
            }
        }

        public void ProcessDatabaseCommand(string line)
        {
            switch (line.Trim())
            {
                case "KILL":
                    KillDatabase();
                    break;

                case "":
                    break;

                default:
                    DatabaseService.SendMessageToPipe(line);
                    break;
            }
        }

        private void ProcessUserInput(string line)
        {
            const string RunTestStatement = "RUN ";
            if (line.StartsWith(RunTestStatement))
            {
                string testName = line.Substring(RunTestStatement.Length).Trim();
                if (!testName.All(char.IsLetter))
                {
                    throw new Exception("Test name should contain only letters.");
                }

                if (testName == "ALL")
                {
                    Test.RunAll();
                }
                else
                {
                    Test test = new Test(testName);
                    test.Run();
                }
            }
            else
            {
                ProcessDatabaseCommand(line);
            }
        }

        private void KillDatabase()
        {
            DatabaseService.Process.Kill();
        }

        public void SetLogFilePath(string logFilePath)
        {
            DatabaseService.SetLogFilePath(logFilePath);

            // Restart database so new log file path value is picked up.
            //
            KillDatabase();
        }

        private void KeepDatabaseServiceUp()
        {
            Thread.CurrentThread.Name = MethodBase.GetCurrentMethod().Name;

            DatabaseService = new DatabaseService();
            // Console.WriteLine("Database service created");

            while (true)
            {
                // Console.WriteLine("Starting up database service...");
                DatabaseService.StartUpAsProcess();
                DatabaseService.Process.WaitForExit();
                // Console.WriteLine("Database service exited.");

                if (KeepDatabaseServiceUpThreadCancellationTokenSource != null &&
                    KeepDatabaseServiceUpThreadCancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }
    }
}
