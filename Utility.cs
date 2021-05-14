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
                        Console.WriteLine(exception.ToString());
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
                Thread.Sleep(100);
            }
        }
    }
}
