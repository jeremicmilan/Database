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

            StorageService = new StorageService();
            new Thread(() => KeepServiceUp(StorageService)).Start();

            LogService = new LogService();
            new Thread(() => KeepServiceUp(LogService)).Start();
        }

        protected override void WaitForServicesBoot()
        {
            Utility.WaitUntil(() => DatabaseService != null);
            Utility.WaitUntil(() => StorageService != null);
            Utility.WaitUntil(() => LogService != null);
        }

        protected override void StopStartedServices()
        {
            DatabaseService.Stop();
            StorageService.Stop();
            LogService.Stop();
        }

        protected override void SnapWindow()
        {
            Window.SnapTopLeft(Process.GetCurrentProcess());
        }
    }
}
