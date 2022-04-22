using Database.Tests;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Database
{
    public class DatabaseClient
    {
        protected DatabaseClient() { }

        protected static DatabaseClient _DatabaseClient = null;
        public static DatabaseClient Get() => _DatabaseClient;

        public static DatabaseClient Create()
        {
            if (_DatabaseClient != null)
            {
                throw new Exception("There can be only one database starter.");
            }

            return _DatabaseClient = new DatabaseClient();
        }

        private CancellationTokenSource KeepServicesUpThreadCancellationTokenSource;
        protected DatabaseService DatabaseService = null;

        public void StartUp()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
                {
                    KeepServicesUpThreadCancellationTokenSource.Cancel();
                    KillStartedProcesses();
                };

            SnapWindow();

            KeepServicesUpThreadCancellationTokenSource = new CancellationTokenSource();
            StartServices();
            WaitForServicesBoot();

            WaitForUserInput();
        }

        protected virtual void StartServices()
        {
            DatabaseService = new DatabaseServiceTraditional();
            new Thread(() => KeepServiceUp(DatabaseService)).Start();
        }

        protected virtual void WaitForServicesBoot()
        {
            Utility.WaitUntil(() => DatabaseService != null);
        }

        protected virtual void KillStartedProcesses()
        {
            DatabaseService?.Process?.Kill();
        }

        protected virtual void SnapWindow()
        {
            Window.SnapLeft(Process.GetCurrentProcess());
        }

        protected void WaitForUserInput()
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
                    Utility.TraceFailure(exception.ToString());
                }
            }
        }

        public void ProcessUserInput(string line)
        {
            const string RunTestStatement = "RUN ";

            const string ConfigureStatement = "CONFIGURE ";
            const string LoggingStatementPart = "LOGGING ";

            switch (line.Trim())
            {
                case "KILL":
                    KillDatabase();
                    break;

                case ConfigureStatement + LoggingStatementPart + "OFF":
                    OverrideDatabaseServiceConfiguration(new ServiceConfiguration { LoggingEnabled = false });
                    break;

                case ConfigureStatement + LoggingStatementPart + "ON":
                    OverrideDatabaseServiceConfiguration(new ServiceConfiguration { LoggingEnabled = true });
                    break;

                case string s when s.StartsWith(RunTestStatement):
                    string testName = line[RunTestStatement.Length..].Trim();
                    if (!testName.All(c => char.IsLetterOrDigit(c) || c == '.'))
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

                    break;

                case "":
                    break;

                default:
                    DatabaseService.SendMessageToPipe(DatabaseService.DatabasePipeName, message: line);
                    break;
            }
        }

        protected void KillDatabase()
        {
            DatabaseService.Process.Kill();
        }

        public void OverrideDatabaseServiceConfiguration(ServiceConfiguration serviceConfiguration)
        {
            DatabaseService.OverrideConfiguration(serviceConfiguration);

            // Restart database so new configuration is picked up.
            //
            KillDatabase();
        }

        protected void KeepServiceUp(Service service)
        {
            Thread.CurrentThread.Name = service.GetType() + "_" + MethodBase.GetCurrentMethod().Name;

            while (true)
            {
                Utility.TraceDebugMessage(string.Format("Starting up {0}...", service.GetType()));
                service.StartUpAsProcess();
                service.Process.WaitForExit();
                Utility.TraceDebugMessage(string.Format("{0} exited.", service.GetType()));

                if (KeepServicesUpThreadCancellationTokenSource != null &&
                    KeepServicesUpThreadCancellationTokenSource.IsCancellationRequested)
                {
                    break;
                }

                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }
    }
}
