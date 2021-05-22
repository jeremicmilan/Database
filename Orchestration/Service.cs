using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace Database
{
    public abstract class Service
    {
        public Process Process = null;
        protected ServiceConfiguration ServiceConfiguration;

        protected Service(ServiceConfiguration serviceConfiguration = null)
        {
            ServiceConfiguration = serviceConfiguration ?? new ServiceConfiguration(this.GetType().ToString());
        }

        public abstract void StartUp();

        public void StartUpAsProcess()
        {
            Process currentProcess = Process.GetCurrentProcess();

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.UseShellExecute = true;

            XmlSerializer serviceConfigurationSerializer = new XmlSerializer(typeof(ServiceConfiguration));
            StringWriter stringWriter = new StringWriter();
            serviceConfigurationSerializer.Serialize(stringWriter, ServiceConfiguration);
            string serviceConfigurationString = new string(stringWriter.ToString().Where(c => !Environment.NewLine.Contains(c)).ToArray());
            string arguments = "\"" + serviceConfigurationString.Replace("\"", "\\\"") + "\"";
            string processName = currentProcess.ProcessName;
            processStartInfo.FileName = processName;
            processStartInfo.Arguments = arguments;

            Process = Process.Start(processStartInfo);

            Console.WriteLine(string.Format("Process {0} started with arguments {1}", processName, arguments));
        }

        private enum MessageStatus
        {
            Success,
            Failure,
        }

        public static void RegisterPipeServer(string pipeName, Action<string> action)
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
                                try
                                {
                                    action(message);
                                    WriteMessageToPipeStream(pipeServer, MessageStatus.Success.ToString());
                                }
                                catch (Exception exception)
                                {
                                    Console.WriteLine(string.Format("While processing message {0} hit exception {1}", message, exception.ToString()));
                                    WriteMessageToPipeStream(pipeServer, MessageStatus.Failure.ToString());
                                }
                            }

                            Thread.Sleep(TimeSpan.FromSeconds(0.1));
                        }
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Pipe server main loop failed: {0}", exception.ToString());
                    Thread.Sleep(TimeSpan.FromSeconds(10));
                }
            }
        }

        private NamedPipeClientStream PipeClient;
        private string PipeName;

        public void RegisterPipeClient(string pipeName)
        {
            PipeName = pipeName;

            if (PipeClient != null)
            {
                PipeClient.Dispose();
            }

            PipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            Console.Write("Attempting to connect to pipe...");
            PipeClient.Connect();

            Console.WriteLine("Connected to pipe.");
            Console.WriteLine("There are currently {0} pipe server instances open.", PipeClient.NumberOfServerInstances);
        }

        public void SendMessageToPipe(string message)
        {
            Utility.ExecuteWithRetry(
                action: () =>
                {
                    WriteMessageToPipeStream(PipeClient, message);
                    MessageStatus status = ReadStatusFromPipeStream(PipeClient);

                    if (status == MessageStatus.Failure)
                    {
                        throw new Exception("Received failure from pipe, while sending message: " + message);
                    }
                },
                correctiveActionPredicate: (exception) => exception.Message == "Pipe is broken." || exception.Message == "Cannot access a closed pipe.",
                correctiveAction: () => RegisterPipeClient(PipeName)
                );
        }

        private static void WriteMessageToPipeStream(PipeStream pipeStream, string message)
        {
            StreamWriter streamWriter = new StreamWriter(pipeStream);
            streamWriter.WriteLine(message);
            streamWriter.Flush();
        }

        private static void WriteStatusToPipeStream(PipeStream pipeStream, MessageStatus messageStatus)
        {
            WriteMessageToPipeStream(pipeStream, messageStatus.ToString());
        }

        private static string ReadMessageFromPipeStream(PipeStream pipeStream)
        {
            StreamReader streamReader = new StreamReader(pipeStream);
            return streamReader.ReadLine();
        }

        private static MessageStatus ReadStatusFromPipeStream(PipeStream pipeStream)
        {
             return Enum.Parse<MessageStatus>(ReadMessageFromPipeStream(pipeStream));
        }
    }
}
