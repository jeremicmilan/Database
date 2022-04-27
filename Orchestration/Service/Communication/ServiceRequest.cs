using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;

namespace Database
{
    public abstract class ServiceRequest<TServiceResponseResult> : ServiceMessage, IServiceRequest
        where TServiceResponseResult : ServiceResponseResult
    {
        private static readonly Dictionary<string, NamedPipeClientStream> PipeClients = new Dictionary<string, NamedPipeClientStream>();

        public ServiceRequest()
        { }

        // To use TServiceResponseResult here we need https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/covariant-returns
        public abstract ServiceResponseResult Process();

        public abstract TServiceResponseResult Send();

        protected TServiceResponseResult WriteToPipe(string servicePipeName)
        {
            PipeClients[servicePipeName] = PipeClients.GetValueOrDefault(servicePipeName) ?? RegisterPipeClient(servicePipeName);
            TServiceResponseResult serviceResponseResult = null;

            Utility.ExecuteWithRetry(
                action: () =>
                {
                    WriteToPipeStream(PipeClients[servicePipeName]);
                    ServiceResponse serviceResponse = ReadResponseFromPipeStream(PipeClients[servicePipeName]);
                    switch (serviceResponse)
                    {
                        case ServiceResponseSuccess serviceResponseSuccess:
                            break;

                        case ServiceResponseSuccessWithResults<ServiceResponseResult> serviceResponseSuccessWithResults:
                            serviceResponseResult = (TServiceResponseResult)serviceResponseSuccessWithResults.ServiceResponseResult;
                            break;

                        case ServiceResponseFailure serviceResponseFailure:
                            throw serviceResponseFailure.Exception;

                        default:
                            throw new Exception("Unknown service response.");
                    }
                },
                correctiveActionPredicate: (exception) =>
                    exception.Message == "Pipe is broken." || exception.Message == "Pipe hasn't been connected yet.",
                correctiveAction: () => PipeClients[servicePipeName] = RegisterPipeClient(servicePipeName)
                );

            return serviceResponseResult;
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

        private ServiceResponse ReadResponseFromPipeStream(PipeStream pipeStream)
        {
            StreamReader streamReader = new StreamReader(pipeStream);
            string serviceResponseString = Utility.WaitUntil(
                func: () => streamReader.ReadLine(),
                predicate: (result) => result != null && result != "");
            return Utility.Deserialize<ServiceResponse>(serviceResponseString);
        }
    }
}
