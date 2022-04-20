using Database.Tests;
using System;
using System.Diagnostics;
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

            return _DatabaseClient = Utility.ServiceConfiguration.IsHyperScale ? new DatabaseClientHyperscale() : new DatabaseClient();
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

            DatabaseService.RegisterPipeClient(DatabaseService.DatabasePipeName);

            WaitForUserInput();
        }

        protected virtual void StartServices()
        {
            DatabaseService = new DatabaseService();
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

        protected void ProcessUserInput(string line)
        {
            const string RunTestStatement = "RUN ";
            if (line.StartsWith(RunTestStatement))
            {
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
            }
            else
            {
                ProcessDatabaseCommand(line);
            }
        }

        protected void KillDatabase()
        {
            DatabaseService.Process.Kill();
        }

        public void OverrideDatabaseServiceConfirguration(ServiceConfiguration serviceConfiguration)
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
