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

            // Block on user input
            //
            Service.RegisterPipeClient(DatabaseService.DatabasePipeName, WaitForUserInput);
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

        private void WaitForUserInput(Action<string> sendMessageToDatabase)
        {
            while (true)
            {
                Console.Write("> ");
                string line = Console.ReadLine();

                if (line == "EXIT")
                {
                    return;
                }

                ProcessUserInput(line.Trim(), sendMessageToDatabase);
            }
        }

        private void ProcessUserInput(string line, Action<string> sendMessageToDatabase)
        {
            Action<string> ParseLine = (line) =>
            {
                switch (line.Trim())
                {
                    case "KILL":
                        DatabaseService.Process.Kill();
                        break;

                    case "":
                        break;

                    default:
                        sendMessageToDatabase(line);
                        break;
                }
            };

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
                    Test.RunAll(ParseLine);
                }
                else
                {
                    Test test = new Test(testName, ParseLine);
                    test.Run();
                }
            }
            else
            {
                ParseLine(line);
            }
        }

        private void KeepDatabaseServiceUp()
        {
            Thread.CurrentThread.Name = MethodBase.GetCurrentMethod().Name;

            DatabaseService = new DatabaseService();
            Console.WriteLine("Database service created");

            while (true)
            {
                Console.WriteLine("Starting up database service...");
                DatabaseService.StartUpAsProcess();
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
    }
}
