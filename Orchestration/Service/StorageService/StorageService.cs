using System.Diagnostics;

namespace Database
{
    public class StorageService : Service
    {
        public const string StorageServicePipeName = "StorageServicePipe";
        protected override string ServicePipeName => StorageServicePipeName;

        public DataManagerTraditional DataManager;

        public StorageService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        {
            DataManager = new DataManagerTraditional(serviceConfiguration?.DataFilePath);
        }

        public static StorageService Get()
        {
            return Get<StorageService>();
        }

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
