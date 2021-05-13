using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Database.Tests
{
    class Test
    {
        public string TestName { get; private set; }

        private Action<string> SendMessageToDatabase;

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
            RecreateWorkingDirectory();

            LogTestMessage("Running test: " + TestName);

            using (StreamReader streamReader = new StreamReader(TestFile))
            {
                while (streamReader.Peek() >= 0)
                {
                    string line = streamReader.ReadLine();
                    LogTestMessage(TestExecutionPrefix + line);
                    SendMessageToDatabase(line);
                }
            }

            LogTestMessage("Finnished test: " + TestName);
        }

        private void RecreateWorkingDirectory()
        {
            if (Directory.Exists(WorkingDirectory))
            {
                Directory.Delete(WorkingDirectory, recursive: true);
            }

            Directory.CreateDirectory(WorkingDirectory);
        }

        private static string RootDirectory => Path.GetFullPath("..\\..\\..");

        private static string WorkingDirectory => RootDirectory + Path.DirectorySeparatorChar + "WorkingDirectory";

        private static string TestsDirectory => RootDirectory + Path.DirectorySeparatorChar + "Tests";

        private static DirectoryInfo TestsDirectoryInfo => new DirectoryInfo(TestsDirectory);

        private string TestDirectory => TestsDirectory + Path.DirectorySeparatorChar + TestName;

        private string TestFile => TestDirectory + Path.DirectorySeparatorChar + "test.txt";

        private void LogTestMessage(string message)
        {
            Console.WriteLine(TestExecutionPrefix + message);
        }

        private const string TestExecutionPrefix = "-- ";
    }
}
