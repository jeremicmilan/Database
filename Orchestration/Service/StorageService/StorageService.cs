using System.Diagnostics;

namespace Database
{
    public class StorageService : Service
    {
        public const string StorageServicePipeName = "StorageServicePipe";
        protected override string ServicePipeName => StorageServicePipeName;

        public StorageService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        { }

        protected override void StartInternal()
        {
            // TODO: start a thread for applying the log from log service
        }

        public override void SnapWindow()
        {
            Window.SnapBottomLeft(Process.GetCurrentProcess());
        }
    }
}
