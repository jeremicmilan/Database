using System.Diagnostics;
using System.Threading;

namespace Database
{
    public class OrchestratorHyperscale : Orchestrator
    {
        public OrchestratorHyperscale() { }

        StorageService StorageService = null;
        LogService LogService = null;

        protected override void StartServices()
        {
            DatabaseService = new DatabaseServiceHyperscale();
            new Thread(() => KeepServiceUp(DatabaseService)).Start();
            Services.Add(DatabaseService);

            StorageService = new StorageService();
            new Thread(() => KeepServiceUp(StorageService)).Start();
            Services.Add(StorageService);

            LogService = new LogService();
            new Thread(() => KeepServiceUp(LogService)).Start();
            Services.Add(LogService);
        }

        protected override void SnapWindow()
        {
            Window.SnapTopLeft(Process.GetCurrentProcess());
        }
    }
}
