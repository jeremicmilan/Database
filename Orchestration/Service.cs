using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Remoting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Database
{
    public abstract class Service
    {

        public Process Process = null;

        public abstract void StartUp();

        public void StartUpAsProcess()
        {
            Process currentProcess = Process.GetCurrentProcess();

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.UseShellExecute = true;

            string processName = currentProcess.ProcessName;
            string arguments = this.GetType().ToString();
            processStartInfo.FileName = processName;
            processStartInfo.Arguments = arguments;

            Process = Process.Start(processStartInfo);

            Console.WriteLine(string.Format("Process {0} started with arguments {1}", processName, arguments));
        }

        public static void RegisterPipeServer (string pipeName, Action<string> action)
        {
            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut))
            {
                Console.WriteLine("NamedPipeServerStream object created.");

                Console.Write("Waiting for client connection...");
                pipeServer.WaitForConnection();
                Console.WriteLine("Client connected.");

                try
                {
                    using (StreamReader streamReader = new StreamReader(pipeServer))
                    {
                        while (true)
                        {
                            string message = streamReader.ReadLine();
                            if (message != null)
                            {
                                action(message);
                            }

                            Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        }
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("ERROR: {0}", e.Message);
                }
            }
        }

        public static void RegisterPipeClient (string pipeName, Action<Action<string>> action)
        {
            string lastMessage = null;

            while (true)
            {
                try
                {
                    using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut))
                    {
                        Console.Write("Attempting to connect to pipe...");
                        pipeClient.Connect();

                        Console.WriteLine("Connected to pipe.");
                        Console.WriteLine("There are currently {0} pipe server instances open.",
                            pipeClient.NumberOfServerInstances);
                        using (StreamWriter streamWriter = new StreamWriter(pipeClient))
                        {
                            if (lastMessage != null)
                            {
                                // If the pipe breaks we need to send the last message
                                //     when the previous pipe was alive
                                //
                                streamWriter.WriteLine(lastMessage);
                                streamWriter.Flush();
                                lastMessage = null;
                            }

                            action((message) =>
                            {
                                lastMessage = message;
                                streamWriter.WriteLine(message);
                                streamWriter.Flush();
                            });
                        }
                    }
                }
                catch (IOException exception)
                {
                    if (exception.Message == "Pipe is broken.")
                    {
                        continue;
                    }
                    else
                    {
                        Console.WriteLine(exception.ToString());
                    }
                }

                break;
            }
        }
    }
}
