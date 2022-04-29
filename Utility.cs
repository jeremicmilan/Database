using System;
using System.IO;
using Newtonsoft.Json;
using System.Threading;

namespace Database
{
    class Utility
    {
        public static void ExecuteWithRetry(
            Action action,
            Predicate<Exception> correctiveActionPredicate,
            Action correctiveAction)
        {

            while (true)
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    if (correctiveActionPredicate(exception))
                    {
                        correctiveAction();
                        WaitDefaultPipeTimeout();
                        continue;
                    }
                    else
                    {
                        throw;
                    }
                }

                break;
            }
        }

        public static void WaitUntil(Func<bool> predicate)
        {
            while (!predicate())
            {
                WaitDefaultPipeTimeout();
            }
        }

        public static T WaitUntil<T>(Func<T> func, Predicate<T> predicate)
        {
            T result = func();
            while (!predicate(result))
            {
                WaitDefaultPipeTimeout();
                result = func();
            }

            return result;
        }


        public static TimeSpan DefaultPipeResultWaitTimespan = TimeSpan.FromMilliseconds(100);

        public static void WaitDefaultPipeTimeout()
        {
            Thread.Sleep(DefaultPipeResultWaitTimespan);
        }

        public static string RootDirectory => Path.GetFullPath(".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "..");

        public static string WorkingDirectory => RootDirectory + Path.DirectorySeparatorChar + "WorkingDirectory";

        public static string DefaultLogFilePath => WorkingDirectory + Path.DirectorySeparatorChar + "database.datalog";

        public static string DefaultDataFilePath => WorkingDirectory + Path.DirectorySeparatorChar + "database.data";

        public static string DefaultTraceFilePath => WorkingDirectory + Path.DirectorySeparatorChar + Console.Title + ".trace";

        public static void CleanupFiles()
        {
            if (File.Exists(DefaultLogFilePath))
            {
                File.WriteAllText(DefaultLogFilePath, "");
            }

            if (File.Exists(DefaultDataFilePath))
            {
                File.WriteAllText(DefaultDataFilePath, "");
            }
        }

        public static void CleanupTraces()
        {
            string[] files = Directory.GetFiles(WorkingDirectory, "*.trace");

            foreach (string file in files)
            {
                if (File.Exists(file))
                {
                    File.WriteAllText(file, "");
                }
            }
        }

        public static string DateTimePrefix => DateTime.Now.ToString(format: "yyyy-MM-dd HH:mm:ss.ffff") + "  ::  ";

        public static void LogServiceRequestBegin(string message, params object[] parameters) =>
            LogMessage(ConsoleColor.DarkCyan, message, parameters);
        public static void LogServiceRequestEnd(string message, params object[] parameters) =>
            LogMessage(ConsoleColor.Cyan, message, parameters);

        public static void LogServiceBegin(string message, params object[] parameters) =>
            LogMessage(ConsoleColor.DarkBlue, message, parameters);
        public static void LogServiceEnd(string message, params object[] parameters) =>
            LogMessage(ConsoleColor.Blue, message, parameters);

        public static void LogFailure(string message, params object[] parameters) =>
            LogMessage(ConsoleColor.Red , "ERROR: " + message, parameters);

        public static void LogTestBegin(string message, params object[] parameters) =>
            LogMessage(ConsoleColor.DarkGreen, "ERROR: " + message, parameters);
        public static void LogTestMessage(string message, params object[] parameters) =>
            LogMessage(ConsoleColor.Yellow, "ERROR: " + message, parameters);
        public static void LogTestEnd(string message, params object[] parameters) =>
            LogMessage(ConsoleColor.Green, "ERROR: " + message, parameters);

        private static readonly object LogMessageLock = new object();
        private static void LogMessage(ConsoleColor color, string message, params object[] parameters)
        {
            lock (LogMessageLock)
            {
                ConsoleColor previousConsoleColor = Console.ForegroundColor;
                Console.ForegroundColor = color;

                LogMessage(message, parameters);

                Console.ForegroundColor = previousConsoleColor;
            }
        }

        public static void LogMessage(string message, params object[] parameters)
        {
            lock (LogMessageLock)
            {
                Console.WriteLine(DateTimePrefix + message, parameters);
                TraceDebugMessage(message, parameters);
            }
        }

        public static void TraceDebugMessage(string message)
        {
            ExecuteFileActionResiliently(() =>
                {
                    if (!File.Exists(DefaultTraceFilePath))
                    {
                        Directory.CreateDirectory(Directory.GetParent(DefaultTraceFilePath).FullName);
                        File.Create(DefaultTraceFilePath);
                    }

                    using StreamWriter streamWriter = File.AppendText(DefaultTraceFilePath);
                    streamWriter.WriteLine(DateTimePrefix + message);
                });
        }

        public static void TraceDebugMessage(string format, params object[] paramaters)
        {
            TraceDebugMessage(string.Format(format, paramaters));
        }

        public static void FileCreateIfNeeded(string filePath)
        {
            ExecuteFileActionResiliently(() =>
            {
                if (!File.Exists(filePath))
                {
                    using (File.Create(filePath)) { }
                }
            });
        }

        public static string[] FileReadAllLines(string filePath)
        {
            string[] lines = null;

            ExecuteFileActionResiliently(() =>
            {
                lines = File.ReadAllLines(filePath);
            });

            return lines;
        }

        public static void FileWriteAllLines(string filePath, string[] lines)
        {
            ExecuteFileActionResiliently(() =>
            {
                File.WriteAllLines(filePath, lines);
            });
        }

        public static void ExecuteFileActionResiliently(Action action)
        {
            ExecuteWithRetry(action,
                correctiveActionPredicate: (exception) => exception.Message.EndsWith("because it is being used by another process."),
                correctiveAction: () => { }
            );
        }

        public static string Serialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj, JsonSerializerSettings);
        }

        public static T Deserialize<T>(string json)
        {
            return (T)JsonConvert.DeserializeObject(json, JsonSerializerSettings);
        }

        public static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
    }
}
