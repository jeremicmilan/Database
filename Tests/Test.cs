﻿using System;
using System.IO;

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
            Utility.LogTestBegin(TestExecutionPrefix + "Running test: " + TestName);

            try
            {
                Start();

                ExecuteTestFile();
                CheckExpectedOutputFiles();
            }
            catch (Exception exception)
            {
                Utility.LogFailure(exception.ToString());
            }
            finally
            {
                Cleanup();
            }

            Utility.LogTestEnd(TestExecutionPrefix + "Finnished test: " + TestName);
        }

        private void Start()
        {
            RecreateWorkingDirectory();

            ServiceConfiguration serviceConfiguration = new ServiceConfiguration
            {
                DataFilePath = TestDataFile,
                LogFilePath = TestLogFile
            };
            DatabaseClient.Get().Orchestrator.OverrideServicesConfiguration(serviceConfiguration);
        }

        private void Cleanup()
        {
            DatabaseClient.Get().Orchestrator.OverrideServicesConfiguration(null);
        }

        private void ExecuteTestFile()
        {
            using StreamReader streamReader = new StreamReader(TestFile);
            while (streamReader.Peek() >= 0)
            {
                string line = streamReader.ReadLine();
                Utility.LogTestMessage(TestExecutionPrefix + TestExecutionPrefix + line);
                DatabaseClient.Get().ProcessUserInput(line);
            }
        }

        private void CheckExpectedOutputFiles()
        {
            if (File.Exists(TestDataFile) && File.Exists(TestExpectedDataFile))
            {
                CheckSameFileContent(TestDataFile, TestExpectedDataFile);
            }
            else if (File.Exists(TestExpectedDataFile))
            {
                throw new Exception("Test data file not generated when it should have been.");
            }

            if (File.Exists(TestLogFile) && File.Exists(TestExpectedLogFile))
            {
                CheckSameFileContent(TestLogFile, TestExpectedLogFile);
            }
            else if (File.Exists(TestExpectedLogFile))
            {
                throw new Exception("Test log file not generated when it should have been.");
            }
        }

        private void CheckSameFileContent(string file1, string file2)
        {
            string[] file1lines = File.ReadAllLines(file1);
            string[] file2lines = File.ReadAllLines(file2);

            if (file1lines.Length != file2lines.Length)
            {
                throw new Exception(string.Format(
                    "File {0} line count does not match the expected file {1} line count.",
                    file1, file2));
            }

            for (int i = 0; i < file1lines.Length; i++)
            {
                if (file1lines[i] != file2lines[i])
                {
                    throw new Exception(string.Format(
                        "File {0} does not match the expected file {1} on line: {2}",
                        file1, file2, i + 1));
                }
            }
        }
        private void RecreateWorkingDirectory()
        {
            CleanupWorkingDirectoryIfExists();
            Directory.CreateDirectory(TestWorkingDirectory);
        }

        private void CleanupWorkingDirectoryIfExists()
        {
            if (Directory.Exists(TestWorkingDirectory))
            {
                Directory.Delete(TestWorkingDirectory, recursive: true);
            }
        }

        private static string TestsDirectory => Utility.RootDirectory + Path.DirectorySeparatorChar + "Tests";

        private static DirectoryInfo TestsDirectoryInfo => new DirectoryInfo(TestsDirectory);

        private string TestDirectory => TestsDirectory + Path.DirectorySeparatorChar + TestName;

        private string TestFile => TestDirectory + Path.DirectorySeparatorChar + "test.txt";

        private string TestExpectedLogFile => TestDirectory + Path.DirectorySeparatorChar + "expected.datalog";

        private string TestExpectedDataFile => TestDirectory + Path.DirectorySeparatorChar + "expected.data";

        private string TestWorkingDirectory => TestDirectory + Path.DirectorySeparatorChar + "WorkingDirectory";

        private string TestDataFile => TestWorkingDirectory + Path.DirectorySeparatorChar + "test.data";

        private string TestLogFile => TestWorkingDirectory + Path.DirectorySeparatorChar + "test.datalog";

        private const string TestExecutionPrefix = "-- ";
    }
}
