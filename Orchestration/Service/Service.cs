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
        public ServiceConfiguration ServiceConfiguration { get; private set; }
        protected ServiceConfiguration DefaultServiceConfiguration => new ServiceConfiguration { ServiceType = this.GetType().ToString() };

        protected Process Process = null;

        public static readonly Dictionary<Tuple<Type, Type>, string> ServiceToServicePipeNames = new Dictionary<Tuple<Type, Type>, string>()
        {
            { new Tuple<Type, Type>(typeof(DatabaseClient),            typeof(DatabaseService)), "DatabaseClientPipe"    },
            { new Tuple<Type, Type>(typeof(DatabaseServiceHyperscale), typeof(LogService)),      "DatabaseToLogPipe"     },
            { new Tuple<Type, Type>(typeof(DatabaseServiceHyperscale), typeof(StorageService)),  "DatabaseToStoragePipe" },
            { new Tuple<Type, Type>(typeof(StorageService),            typeof(LogService)),      "StorageToLogPipe"      },
        };

        protected Service(ServiceConfiguration serviceConfiguration = null)
        {
            ServiceConfiguration = serviceConfiguration ?? DefaultServiceConfiguration;
        }

        private static Service _Service = null;
        public static Service Get() => _Service;
        protected static TService Get<TService>()
            where TService : Service
        {
            return (TService)_Service;
        }

        public virtual LogManager GetLogManager() => null;
        public virtual StorageManager GetStorageManager() => null;
        public virtual TransactionManager GetTransactionManager() => null;

        public abstract void SnapWindow();

        public void Start()
        {
            Utility.LogServiceBegin("Starting {0}...", GetType().ToString()[9..]);

            if (_Service != null)
            {
                throw new Exception("Only one service can be started per process.");
            }
            else
            {
                _Service = this;
            }

            StartInternal();

            Utility.LogServiceEnd("{0} started.", GetType().ToString()[9..]);

            RegisterPipeServersAndBlock();
        }

        private void RegisterPipeServersAndBlock()
        {
            List<string> pipeNames = ServiceToServicePipeNames
                .Where(item => this.GetType() == item.Key.Item2 || this.GetType().IsSubclassOf(item.Key.Item2))
                .Select(item => item.Value)
                .ToList();
            List<Thread> threads = new List<Thread>();

            foreach (string pipeName in pipeNames)
            {
                Thread thread = new Thread(() => RegisterPipeServer(pipeName));
                threads.Add(thread);
                thread.Name = this.GetType().ToString()[9..] + "_" + pipeName;
                thread.Start();
            }

            // Block until one of the pipe server fails
            //
            Utility.WaitUntil(() => threads.Where(thread => !thread.IsAlive).FirstOrDefault() != null);
        }

        protected abstract void StartInternal();

        public void Stop()
        {
            Kill();
        }

        public void Kill()
        {
            IsWaitingForExit = false;
            Process?.Kill();
        }

        public bool IsWaitingForExit { get; private set; } = false;
        public void WaitForExit()
        {
            IsWaitingForExit = true;
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

            Utility.LogDebugMessage("Process {0} started with arguments {1}", processName, arguments);
        }

        protected void RegisterPipeServer(string pipeName)
        {
            try
            {
                while (true)
                {
                    using NamedPipeServerStream pipeServer = new NamedPipeServerStream(pipeName, PipeDirection.InOut);

                    Utility.LogDebugMessage("Waiting for client connection on pipe {0}...", pipeName);
                    pipeServer.WaitForConnection();
                    Utility.LogDebugMessage("Client connected on pipe {0}.", pipeName);

                    using StreamReader streamReader = new StreamReader(pipeServer);
                    while (true)
                    {
                        string line = streamReader.ReadLine();
                        if (line == null)
                        {
                            break;
                        }

                        IServiceRequest serviceRequest = Utility.Deserialize<IServiceRequest>(line);
                        if (serviceRequest != null)
                        {
                            try
                            {
                                ServiceResponseResult serviceResponseResult = serviceRequest.Process();

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
                                Utility.LogFailure(string.Format("While processing request {0} hit exception {1}", serviceRequest, exception.ToString()));
                                new ServiceResponseFailure(exception).WriteToPipeStream(pipeServer);
                            }
                        }

                        Utility.WaitDefaultPipeTimeout();
                    }

                    Utility.LogDebugMessage("Client connection closed.");
                }
            }
            catch (Exception exception)
            {
                Utility.LogFailure("Pipe server main loop failed: {0}", exception.ToString());
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
