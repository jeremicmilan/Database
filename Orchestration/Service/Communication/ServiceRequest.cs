using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;

namespace Database
{
    public abstract class ServiceRequest<TServiceResponseResult> : ServiceMessage, IServiceRequest
        where TServiceResponseResult : ServiceResponseResult
    {
        public ServiceRequest()
        { }

        public abstract ServiceResponseResult Process();

        protected abstract Type GetServiceType();

        public TServiceResponseResult Send(Type issuerType = null)
        {
            Utility.LogServiceRequestBegin("Initiating request {0}.", GetType().ToString()[9..]);

            issuerType ??= Service.Get().GetType();

            List<Tuple<Type, Type>> keys = Service.ServiceToServicePipeNames.Keys
                .Where(key => (key.Item1 == issuerType       || key.Item1.IsSubclassOf(issuerType)) &&
                              (key.Item2 == GetServiceType() || key.Item2.IsSubclassOf(GetServiceType())))
                .ToList();
            if (keys.Count > 1)
            {
                throw new Exception("We found more than one pipe, which should not be possible.");
            }

            string pipeName = Service.ServiceToServicePipeNames[keys.First()];
            TServiceResponseResult serviceResponseResult = WriteToPipe(pipeName);

            Utility.LogServiceRequestEnd("Request {0} finished.", GetType().ToString()[9..]);

            return serviceResponseResult;
        }

        private TServiceResponseResult WriteToPipe(string servicePipeName)
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
            Utility.TraceDebugMessage("Attempting to connect to pipe {0}...", pipeName);
            PipeClient.Connect();

            Utility.TraceDebugMessage("Connected to pipe {0}.", pipeName);

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
