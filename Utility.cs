using System;
using System.IO;
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

        public static string DefaultLogFilePath = WorkingDirectory + Path.DirectorySeparatorChar + "database.log";

        public static string DefaultTraceFilePath = WorkingDirectory + Path.DirectorySeparatorChar + Console.Title + ".trace";

        public static void TraceDebugMessage(string message)
        {
            ExecuteWithRetry(() =>
                {
                    if (!File.Exists(DefaultTraceFilePath))
                    {
                        Directory.CreateDirectory(Directory.GetParent(DefaultTraceFilePath).FullName);
                        File.Create(DefaultTraceFilePath);
                    }

                    using (StreamWriter streamWriter = File.AppendText(DefaultTraceFilePath))
                    {
                        streamWriter.WriteLine(DateTime.Now.ToString(format: "yyyy-MM-dd HH:mm:ss.ffff") + "  ::  " + message);
                    }
                },
                correctiveActionPredicate: (exception) => exception.Message.EndsWith("because it is being used by another process."),
                correctiveAction: () => { }
            );
        }

        public static void TraceDebugMessage(string format, params object[] objects)
        {
            TraceDebugMessage(string.Format(format, objects));
        }

        public static void TraceFailure(string message)
        {
            ConsoleColor previousConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("ERROR: " + message);

            Console.ForegroundColor = previousConsoleColor;
        }
    }
}
