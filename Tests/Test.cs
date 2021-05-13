using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Database.Tests
{
    class Test
    {
        public string TestName { get; private set; }

        private Action<string> SendMessageToDatabase = null;

        public Test (string testName, Action<string> sendMessageToDatabase)
        {
            TestName = testName;
            SendMessageToDatabase = sendMessageToDatabase;
        }

        public static void RunAll(Action<string> sendMessageToDatabase)
        {
            foreach (DirectoryInfo directoryInfo in TestsDirectoryInfo.GetDirectories())
            {
                Test test = new Test(directoryInfo.Name, sendMessageToDatabase);
                test.Run();
            }
        }

        public void Run()
        {
            LogTestMessage("Running test: " + TestName);

            RecreateWorkingDirectory();
            SendMessageToDatabase(DatabaseService.SetLogPathStatement + TestLogFile);

            using (StreamReader streamReader = new StreamReader(TestFile))
            {
                while (streamReader.Peek() >= 0)
                {
                    string line = streamReader.ReadLine();
                    LogTestMessage(TestExecutionPrefix + line);
                    SendMessageToDatabase(line);
                }
            }

            CleanupWorkingDirectoryIfExists();
            LogTestMessage("Finnished test: " + TestName);
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

        private static string RootDirectory => Path.GetFullPath("..\\..\\..");

        private static string TestsDirectory => RootDirectory + Path.DirectorySeparatorChar + "Tests";

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
