using Database.Tests;
using System;
using System.Linq;

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

        public Orchestrator Orchestrator { get; set; }

        public void Start<T>() where T : Orchestrator, new()
        {
            Orchestrator = new T();
            Orchestrator.Start();

            WaitForUserInput();
        }

        public void Restart<T>() where T : Orchestrator, new()
        {
            Orchestrator.Stop();
            Start<T>();
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
            const string DatabaseStatementPart = "DATABASE ";

            switch (line.Trim())
            {
                case "KILL":
                    Orchestrator.DatabaseRestart();
                    break;

                case ConfigureStatement + LoggingStatementPart + "OFF":
                    Orchestrator.OverrideDatabaseServiceConfiguration(new ServiceConfiguration { LoggingDisabled = true });
                    break;

                case ConfigureStatement + LoggingStatementPart + "ON":
                    Orchestrator.OverrideDatabaseServiceConfiguration(new ServiceConfiguration { LoggingDisabled = false });
                    break;

                case ConfigureStatement + DatabaseStatementPart + "TRADITIONAL":
                    Restart<OrchestratorTraditional>();
                    break;

                case ConfigureStatement + DatabaseStatementPart + "HYPERSCALE":
                    Restart<OrchestratorHyperscale>();
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
                    DatabaseService.SendMessageToPipe(new DatabaseServiceRequest(DatabaseServiceAction.Query, line));
                    break;
            }
        }
    }
}
