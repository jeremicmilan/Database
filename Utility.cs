using System;
using System.Threading;

namespace Database
{
    class Utility
    {
        public static void ExecuteWithRetry(Action action, Predicate<Exception> correctiveActionPredicate, Action correctiveAction)
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

        public static void LogFailure(string message)
        {
            ConsoleColor previousConsoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("ERROR: " + message);

            Console.ForegroundColor = previousConsoleColor;
        }
    }
}
