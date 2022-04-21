using System.Diagnostics;
using System.Threading;

namespace Database
{
    public class DatabaseClientHyperscale : DatabaseClient
    {
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

        protected override void KillStartedProcesses()
        {
            DatabaseService?.Process?.Kill();
            StorageService?.Process?.Kill();
            LogService?.Process?.Kill();
        }

        protected override void SnapWindow()
        {
            Window.SnapTopLeft(Process.GetCurrentProcess());
        }
    }
}
