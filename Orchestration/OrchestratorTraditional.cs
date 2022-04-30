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
            DatabaseService databaseService = new DatabaseServiceTraditional();
            new Thread(() => KeepServiceUp(databaseService)).Start();
            Services.Add(databaseService);
        }

        protected override void SnapWindow()
        {
            Window.SnapLeft();
        }

        public override void KillAllServices()
        {
            KillDatabaseService();
        }
    }
}
