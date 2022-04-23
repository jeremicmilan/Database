using System.Diagnostics;

namespace Database
{
    public abstract class DatabaseService : Service
    {
        private readonly Database _Database = null;

        public DatabaseService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        {
            _Database = CreateDatabase(serviceConfiguration);
        }

        public abstract Database CreateDatabase(ServiceConfiguration serviceConfiguration);

        protected override void StartInternal()
        {
            _Database.Start();
        }

        public override void Stop()
        {
            base.Stop();
            _Database?.Stop();
        }

        protected override string ServicePipeName => "DatabasePipe";

        protected override string ProcessRequest(string message)
        {
            return _Database.ProcessQuery(query: message);
        }
    }
}
