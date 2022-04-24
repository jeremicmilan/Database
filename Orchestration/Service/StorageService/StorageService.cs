using System;
using System.Diagnostics;

namespace Database
{
    public class StorageService : Service<StorageServiceAction, StorageServiceRequest, StorageServiceResponseResult>
    {
        private const string StorageServicePipeName = "StorageServicePipe";
        protected override string ServicePipeName => StorageServicePipeName;

        public StorageService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        { }

        protected override void StartInternal()
        {
            // TODO: start a thread for applying the log from log service
        }

        protected override StorageServiceResponseResult ProcessRequest(StorageServiceRequest message)
        {
            throw new NotImplementedException();
        }

        public override void SnapWindow()
        {
            Window.SnapBottomLeft(Process.GetCurrentProcess());
        }

        public static void SendMessageToPipe<TStorageServiceRequest>(TStorageServiceRequest storageServiceRequest)
            where TStorageServiceRequest : StorageServiceRequest
        {
            SendMessageToPipe(storageServiceRequest, StorageServicePipeName);
        }
    }
}
