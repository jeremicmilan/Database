using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Threading;

namespace Database
{
    public abstract class Service<TServiceAction, TServiceRequest, TServiceResponseResult>
        where TServiceAction : Enum
        where TServiceRequest : ServiceRequest<TServiceAction>
        where TServiceResponseResult : ServiceResponseResult
    {
        public Process Process = null;
        public ServiceConfiguration ServiceConfiguration;
        protected ServiceConfiguration DefaultServiceConfiguration => new ServiceConfiguration { ServiceType = this.GetType().ToString() };

        protected abstract string ServicePipeName { get; }

        protected Service(ServiceConfiguration serviceConfiguration = null)
        {
            ServiceConfiguration = serviceConfiguration ?? DefaultServiceConfiguration;
        }

        public abstract void SnapWindow();

        public void Start()
        {
            StartInternal();

            // Block on processing a request
            //
            RegisterPipeServer(ServicePipeName, ProcessRequest);
        }

        protected abstract TServiceResponseResult ProcessRequest(TServiceRequest serviceMessage);

        protected abstract void StartInternal();

        public virtual void Stop()
        {
            Process?.Kill();
        }

        public void StartAsProcess()
        {
            Process currentProcess = Process.GetCurrentProcess();

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = true
            };

            string serviceConfigurationString = JsonSerializer.Serialize(ServiceConfiguration);
            string arguments = "\"" + serviceConfigurationString.Replace("\"", "\\\"") + "\"";
            string processName = currentProcess.ProcessName;
            processStartInfo.FileName = processName;
            processStartInfo.Arguments = arguments;

            Process = Process.Start(processStartInfo);

            Utility.TraceDebugMessage(string.Format("Process {0} started with arguments {1}", processName, arguments));
        }

        public static void RegisterPipeServer(string pipeName, Func<TServiceRequest, TServiceResponseResult> ProcessMessage)
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
                    TServiceRequest serviceRequest = JsonSerializer.Deserialize<TServiceRequest>(streamReader.ReadLine());
                    if (serviceRequest != null)
                    {
                        try
                        {
                            TServiceResponseResult serviceResponse = ProcessMessage(serviceRequest);

                            if (serviceResponse != null)
                            {
                                WriteToPipeStream(pipeServer, new ServiceResponseSuccessWithResults<TServiceResponseResult>(serviceResponse));
                            }
                            else
                            {
                                WriteToPipeStream(pipeServer, new ServiceResponseSuccess());
                            }
                        }
                        catch (Exception exception)
                        {
                            Utility.TraceDebugMessage(string.Format("While processing message {0} hit exception {1}", serviceRequest, exception.ToString()));
                            WriteToPipeStream(pipeServer, new ServiceResponseFailure(exception.Message));
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

        private static NamedPipeClientStream RegisterPipeClient(string pipeName)
        {
            NamedPipeClientStream PipeClient = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            Utility.TraceDebugMessage("Attempting to connect to pipe...");
            PipeClient.Connect();

            Utility.TraceDebugMessage("Connected to pipe.");
            Utility.TraceDebugMessage("There are currently {0} pipe server instances open.", PipeClient.NumberOfServerInstances);

            return PipeClient;
        }

        private static readonly Dictionary<string, NamedPipeClientStream> PipeClients = new Dictionary<string, NamedPipeClientStream>();

        protected static void SendMessageToPipe(TServiceRequest serviceRequest, string servicePipeName)
        {
            PipeClients[servicePipeName] = PipeClients.GetValueOrDefault(servicePipeName) ?? RegisterPipeClient(servicePipeName);

            Utility.ExecuteWithRetry(
                action: () =>
                {
                    WriteToPipeStream(PipeClients[servicePipeName], serviceRequest);
                    string serviceResponseString = ReadResponseFromPipeStream(PipeClients[servicePipeName]);

                    ServiceResponse serviceResponse = JsonSerializer.Deserialize<ServiceResponse>(serviceResponseString);
                    switch (serviceResponse.ServiceAction)
                    {
                        case ServiceResponseStatus.Success:
                            break;

                        case ServiceResponseStatus.SuccessWithResult:
                            ServiceResponseSuccessWithResults<TServiceResponseResult> serviceResponseSuccessWithResults =
                                JsonSerializer.Deserialize<ServiceResponseSuccessWithResults<TServiceResponseResult>>(serviceResponseString);
                            serviceResponseSuccessWithResults.ServiceResponseResult.ProcessResult();
                            break;

                        case ServiceResponseStatus.Failure:
                            ServiceResponseFailure serviceResponseFailure =
                                JsonSerializer.Deserialize<ServiceResponseFailure>(serviceResponseString);
                            throw new Exception(serviceResponseFailure.ExceptionMessage);
                    }
                },
                correctiveActionPredicate: (exception) =>
                    exception.Message == "Pipe is broken." || exception.Message == "Pipe hasn't been connected yet.",
                correctiveAction: () => PipeClients[servicePipeName] = RegisterPipeClient(servicePipeName)
                );
        }

        private static void WriteToPipeStream(PipeStream pipeStream, TServiceRequest serviceRequest)
        {
            WriteToPipeStreamInternal<TServiceAction, TServiceRequest>(pipeStream, serviceRequest);
        }

        private static void WriteToPipeStream<TServiceResponse>(PipeStream pipeStream, TServiceResponse serviceResponseResult)
            where TServiceResponse : ServiceResponse
        {
            WriteToPipeStreamInternal<ServiceResponseStatus, TServiceResponse>(pipeStream, serviceResponseResult);
        }

        private static void WriteToPipeStreamInternal<TServiceActionWriteToPipeStream, TServiceMessage>(
            PipeStream pipeStream,
            TServiceMessage serviceMessage)
            where TServiceActionWriteToPipeStream : Enum
            where TServiceMessage : ServiceMessage<TServiceActionWriteToPipeStream>
        {
            StreamWriter streamWriter = new StreamWriter(pipeStream);
            streamWriter.WriteLine(JsonSerializer.Serialize(serviceMessage));
            streamWriter.Flush();
        }

        private static string ReadResponseFromPipeStream(PipeStream pipeStream)
        {
            StreamReader streamReader = new StreamReader(pipeStream);
            return Utility.WaitUntil(
                func: () => streamReader.ReadLine(),
                predicate: (result) => result != null && result != "");
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
