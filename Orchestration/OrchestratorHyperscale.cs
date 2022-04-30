using System.Diagnostics;
using System.Threading;

namespace Database
{
    public class OrchestratorHyperscale : Orchestrator
    {
        public OrchestratorHyperscale() { }

        public LogService LogService => GetService<LogService>();
        public StorageService StorageService => GetService<StorageService>();

        protected override void StartServices()
        {
            DatabaseService databaseService = new DatabaseServiceHyperscale();
            new Thread(() => KeepServiceUp(databaseService)).Start();
            Services.Add(databaseService);

            StorageService storageService = new StorageService();
            new Thread(() => KeepServiceUp(storageService)).Start();
            Services.Add(storageService);

            LogService logService = new LogService();
            new Thread(() => KeepServiceUp(logService)).Start();
            Services.Add(logService);
        }

        public override void KillAllServices()
        {
            KillDatabaseService();
            KillLogService();
            KillStorageService();
        }

        public void KillLogService()
        {
            LogService.Kill();
        }

        public void KillStorageService()
        {
            StorageService.Kill();
        }

        protected override void SnapWindow()
        {
            Window.SnapTopLeft();
        }
    }
}
