using System.Diagnostics;
using System.Threading;

namespace Database
{
    public class OrchestratorTraditional : Orchestrator
    {
        public OrchestratorTraditional()
        { }

        protected override void StartServices()
        {
            DatabaseService = new DatabaseServiceTraditional();
            new Thread(() => KeepServiceUp(DatabaseService)).Start();
        }

        protected override void WaitForServicesBoot()
        {
            Utility.WaitUntil(() => DatabaseService != null);
        }

        protected override void StopStartedServices()
        {
            DatabaseService.Stop();
        }

        protected override void SnapWindow()
        {
            Window.SnapLeft(Process.GetCurrentProcess());
        }
    }
}
