using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace Database
{
    public abstract class Service
    {
        public Process Process = null;
        public ServiceConfiguration ServiceConfiguration;
        protected ServiceConfiguration DefaultServiceConfiguration => new ServiceConfiguration(this.GetType().ToString());

        protected Service(ServiceConfiguration serviceConfiguration = null)
        {
            ServiceConfiguration = serviceConfiguration ?? DefaultServiceConfiguration;
        }

        public abstract void SnapWindow();
        public abstract void StartUp();

        public void StartUpAsProcess()
        {
            Process currentProcess = Process.GetCurrentProcess();

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = true
            };

            string serviceConfigurationString = ServiceConfiguration.Serialize();
            string arguments = "\"" + serviceConfigurationString.Replace("\"", "\\\"") + "\"";
            string processName = currentProcess.ProcessName;
            processStartInfo.FileName = processName;
            processStartInfo.Arguments = arguments;

            Process = Process.Start(processStartInfo);

            Utility.TraceDebugMessage(string.Format("Process {0} started with arguments {1}", processName, arguments));
        }

        private enum Status
        {
            Success,
            SuccessWithResult,
            Failure,
        }

        public static void RegisterPipeServer(string pipeName, Func<string, string> ProcessMessage)
        {
            using NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut);
            Utility.TraceDebugMessage("NamedPipeServerStream object created.");

            Utility.TraceDebugMessage("Waiting for client connection...");
            pipeServer.WaitForConnection();
            Utility.TraceDebugMessage("Client connected.");

            try
            {
                using StreamReader streamReader = new StreamReader(pipeServer);
                while (true)
                {
                    string message = streamReader.ReadLine();
                    if (message != null)
                    {
                        try
                        {
                            string result = ProcessMessage(message);

                            if (result != null)
                            {
                                WriteStatusToPipeStream(pipeServer, Status.SuccessWithResult);
                                WriteMessageToPipeStream(pipeServer, result);
                            }
                            else
                            {
                                WriteStatusToPipeStream(pipeServer, Status.Success);
                            }
                        }
                        catch (Exception exception)
                        {
                            Utility.TraceDebugMessage(string.Format("While processing message {0} hit exception {1}", message, exception.ToString()));
                            WriteStatusToPipeStream(pipeServer, Status.Failure);
                            WriteMessageToPipeStream(pipeServer, exception.Message);
                        }
                    }

                    Utility.WaitDefaultPipeTimeout();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Pipe server main loop failed: {0}", exception.ToString());
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
        }

        private NamedPipeClientStream RegisterPipeClient(string pipeName)
        {
            NamedPipeClientStream PipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            Utility.TraceDebugMessage("Attempting to connect to pipe...");
            PipeClient.Connect();

            Utility.TraceDebugMessage("Connected to pipe.");
            Utility.TraceDebugMessage("There are currently {0} pipe server instances open.", PipeClient.NumberOfServerInstances);

            return PipeClient;
        }

        private readonly Dictionary<string, NamedPipeClientStream> PipeClients = new Dictionary<string, NamedPipeClientStream>();

        public void SendMessageToPipe(string pipeName, string message)
        {
            PipeClients[pipeName] = PipeClients.GetValueOrDefault(pipeName) ?? RegisterPipeClient(pipeName);

            Utility.ExecuteWithRetry(
                action: () =>
                {
                    WriteMessageToPipeStream(PipeClients[pipeName], message);
                    Status status = ReadStatusFromPipeStream(PipeClients[pipeName]);

                    switch (status)
                    {
                        case Status.Success:
                            break;

                        case Status.SuccessWithResult:
                            string result = ReadMessageFromPipeStream(PipeClients[pipeName]);
                            Table table = Table.Deserialize(result);
                            table.Print();
                            break;

                        case Status.Failure:
                            string errorMessage = ReadMessageFromPipeStream(PipeClients[pipeName]);
                            throw new Exception(errorMessage);
                    }
                },
                correctiveActionPredicate: (exception) =>
                    exception.Message == "Pipe is broken." || exception.Message == "Pipe hasn't been connected yet.",
                correctiveAction: () => PipeClients[pipeName] = RegisterPipeClient(pipeName)
                );
        }

        private static void WriteMessageToPipeStream(PipeStream pipeStream, string message)
        {
            StreamWriter streamWriter = new StreamWriter(pipeStream);
            streamWriter.WriteLine(message);
            streamWriter.Flush();
        }

        private static void WriteStatusToPipeStream(PipeStream pipeStream, Status status)
        {
            WriteMessageToPipeStream(pipeStream, status.ToString());
        }

        private static string ReadMessageFromPipeStream(PipeStream pipeStream)
        {
            StreamReader streamReader = new StreamReader(pipeStream);
            return Utility.WaitUntil(
                func: () => streamReader.ReadLine(),
                predicate: (result) => result != null && result != "");
        }

        private static Status ReadStatusFromPipeStream(PipeStream pipeStream)
        {
             return Enum.Parse<Status>(ReadMessageFromPipeStream(pipeStream));
        }

        public void OverrideConfiguration(ServiceConfiguration serviceConfiguration)
        {
            if (serviceConfiguration != null)
            {
                ServiceConfiguration.Override(serviceConfiguration);
            }
            else
            {
                ServiceConfiguration = DefaultServiceConfiguration;
            }
        }
    }
}
