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
                ExecuteTestFile();
                CheckExpectedOutputFiles();
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
            DatabaseClient.Get().SetLogFilePath(TestLogFile);
        }

        private void Cleanup()
        {
            DatabaseClient.Get().SetLogFilePath(null);
        }

        private void ExecuteTestFile()
        {
            using (StreamReader streamReader = new StreamReader(TestFile))
            {
                while (streamReader.Peek() >= 0)
                {
                    string line = streamReader.ReadLine();
                    LogTestMessage(TestExecutionPrefix + line);
                    DatabaseClient.Get().ProcessDatabaseCommand(line);
                }
            }
        }

        private void CheckExpectedOutputFiles()
        {
            if (File.Exists(TestExpectedLogFile))
            {
                CheckSameFileContent(TestLogFile, TestExpectedLogFile);
            }
        }

        private void CheckSameFileContent(string file1, string file2)
        {
            string[] file1lines = File.ReadAllLines(file1);
            string[] file2lines = File.ReadAllLines(file2);

            if (file1lines.Length != file2lines.Length)
            {
                throw new Exception("Log file line count does not match the expected log file line count.");
            }

            for (int i = 0; i < file1lines.Length; i++)
            {
                if (file1lines[i] != file2lines[i])
                {
                    throw new Exception("Log file does not match the expected log file on line: " + (i + 1));
                }
            }
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

        private string TestExpectedLogFile => TestDirectory + Path.DirectorySeparatorChar + "expected.log";

        private string WorkingDirectory => TestDirectory + Path.DirectorySeparatorChar + "WorkingDirectory";

        private string TestLogFile => WorkingDirectory + Path.DirectorySeparatorChar + "test.log";

        private const string TestExecutionPrefix = "-- ";

        private void LogTestMessage(string message)
        {
            Console.WriteLine(TestExecutionPrefix + message);
        }
    }
}
