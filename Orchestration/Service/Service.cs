using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;

namespace Database
{
    public abstract class Service
    {
        protected Process Process = null;
        public ServiceConfiguration ServiceConfiguration;
        protected ServiceConfiguration DefaultServiceConfiguration => new ServiceConfiguration { ServiceType = this.GetType().ToString() };

        protected abstract string ServicePipeName { get; }

        protected Service(ServiceConfiguration serviceConfiguration = null)
        {
            ServiceConfiguration = serviceConfiguration ?? DefaultServiceConfiguration;
        }

        private static Service _Service = null;
        protected static TService Get<TService>()
            where TService : Service
        {
            return (TService)_Service;
        }

        public abstract void SnapWindow();

        public void Start()
        {
            if (_Service != null)
            {
                throw new Exception("Only one service can be started per process.");
            }
            else
            {
                _Service = this;
            }

            StartInternal();

            // Block on processing a request
            //
            RegisterPipeServer();
        }

        protected abstract void StartInternal();

        public void Stop()
        {
            Kill();
        }

        public void Kill()
        {
            Process?.Kill();
        }

        public void WaitForExit()
        {
            Process?.WaitForExit();
        }

        public void StartAsProcess()
        {
            Process currentProcess = Process.GetCurrentProcess();

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = true
            };

            string serviceConfigurationString = Utility.Serialize(ServiceConfiguration);
            string arguments = "\"" + serviceConfigurationString.Replace("\"", "\\\"") + "\"";
            string processName = currentProcess.ProcessName;
            processStartInfo.FileName = processName;
            processStartInfo.Arguments = arguments;

            Process = Process.Start(processStartInfo);

            Utility.TraceDebugMessage(string.Format("Process {0} started with arguments {1}", processName, arguments));
        }

        private ServiceResponseResult ProcessRequest(IServiceRequest serviceRequest)
        {
            return serviceRequest.Process();
        }

        protected void RegisterPipeServer()
        {
            using NamedPipeServerStream pipeServer = new NamedPipeServerStream(ServicePipeName, PipeDirection.InOut);
            Utility.TraceDebugMessage("NamedPipeServerStream object created.");

            Utility.TraceDebugMessage("Waiting for client connection...");
            pipeServer.WaitForConnection();
            Utility.TraceDebugMessage("Client connected.");

            try
            {
                using StreamReader streamReader = new StreamReader(pipeServer);
                while (true)
                {
                    IServiceRequest serviceRequest = Utility.Deserialize<IServiceRequest>(streamReader.ReadLine());
                    if (serviceRequest != null)
                    {
                        try
                        {
                            ServiceResponseResult serviceResponseResult = ProcessRequest(serviceRequest);

                            if (serviceResponseResult != null)
                            {
                                new ServiceResponseSuccessWithResults<ServiceResponseResult>(serviceResponseResult).WriteToPipeStream(pipeServer);
                            }
                            else
                            {
                                new ServiceResponseSuccess().WriteToPipeStream(pipeServer);
                            }
                        }
                        catch (Exception exception)
                        {
                            Utility.TraceDebugMessage(string.Format("While processing request {0} hit exception {1}", serviceRequest, exception.ToString()));
                            new ServiceResponseFailure(exception.Message).WriteToPipeStream(pipeServer);
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
