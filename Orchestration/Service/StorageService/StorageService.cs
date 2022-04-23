using System;
using System.Diagnostics;

namespace Database
{
    public class StorageService : Service<StorageServiceAction, StorageServiceRequest, StorageServiceResponseResult>
    {
        protected override string ServicePipeName => "StorageServicePipe";

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
    }
}
