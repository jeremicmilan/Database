using Database.Tests;
using System;
using System.Linq;

namespace Database
{
    public class DatabaseClient
    {
        protected DatabaseClient() { }

        private static DatabaseClient _DatabaseClient = null;
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
            const string KillStatement = "KILL";

            const string ConfigureStatement = "CONFIGURE ";
            const string LoggingStatementPart = "LOGGING ";
            const string DatabaseStatementPart = "DATABASE ";

            switch (line.Trim())
            {
                case string s when s.StartsWith(KillStatement):
                    string killStatementPart = line[KillStatement.Length..].Trim();
                    switch (killStatementPart.Trim())
                    {
                        case "":
                        case "DATABASE":
                            Orchestrator.KillDatabaseService();
                            break;

                        case "LOG":
                            ((OrchestratorHyperscale)Orchestrator).KillLogService();
                            break;

                        case "STORAGE":
                            ((OrchestratorHyperscale)Orchestrator).KillStorageService();
                            break;

                        case "ALL":
                            Orchestrator.KillAllServices();
                            break;

                        default:
                            throw new Exception("Unsupported KILL statement.");
                    }
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
                    DatabaseServiceResponseResultQuery databaseServiceResponseResult =
                        new DatabaseServiceRequestQuery(line).Send();
                    databaseServiceResponseResult?.Table?.Print();
                    break;
            }
        }
    }
}
