using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Database.Tests
{
    class Test
    {
        public string TestName { get; private set; }

        public Test (string testName)
        {
            TestName = testName;
        }

        public static void RunAll()
        {
            foreach (DirectoryInfo directoryInfo in TestsDirectoryInfo.GetDirectories())
            {
                Test test = new Test(directoryInfo.Name);
                test.Run();
            }
        }

        public void Run()
        {
            LogTestMessage("Running test: " + TestName);
            Startup();

            try
            {
                using (StreamReader streamReader = new StreamReader(TestFile))
                {
                    while (streamReader.Peek() >= 0)
                    {
                        string line = streamReader.ReadLine();
                        LogTestMessage(TestExecutionPrefix + line);
                        DatabaseStarter.Get().ProcessDatabaseCommand(line);
                    }
                }
            }
            catch (Exception exception)
            {
                Utility.TraceFailure(exception.ToString());
            }

            Cleanup();
            LogTestMessage("Finnished test: " + TestName);
        }

        private void Startup()
        {
            RecreateWorkingDirectory();
            DatabaseStarter.Get().SetLogFilePath(TestLogFile);
        }

        private void Cleanup()
        {
            DatabaseStarter.Get().SetLogFilePath(null);
        }

        private void RecreateWorkingDirectory()
        {
            CleanupWorkingDirectoryIfExists();
            Directory.CreateDirectory(WorkingDirectory);
        }

        private void CleanupWorkingDirectoryIfExists()
        {
            if (Directory.Exists(WorkingDirectory))
            {
                Directory.Delete(WorkingDirectory, recursive: true);
            }
        }

        private static string TestsDirectory => Utility.RootDirectory + Path.DirectorySeparatorChar + "Tests";

        private static DirectoryInfo TestsDirectoryInfo => new DirectoryInfo(TestsDirectory);

        private string TestDirectory => TestsDirectory + Path.DirectorySeparatorChar + TestName;

        private string TestFile => TestDirectory + Path.DirectorySeparatorChar + "test.txt";

        private string WorkingDirectory => TestDirectory + Path.DirectorySeparatorChar + "WorkingDirectory";

        private string TestLogFile => WorkingDirectory + Path.DirectorySeparatorChar + "test.log";

        private const string TestExecutionPrefix = "-- ";

        private void LogTestMessage(string message)
        {
            Console.WriteLine(TestExecutionPrefix + message);
        }
    }
}
