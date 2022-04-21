using System.Diagnostics;

namespace Database
{
    public class DatabaseService : Service
    {
        private readonly Database _Database = null;

        public const string DatabasePipeName = "DatabasePipe";

        public DatabaseService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        {
            _Database = CreateDatabase(serviceConfiguration);
        }

        public virtual Database CreateDatabase (ServiceConfiguration serviceConfiguration = null)
        {
            return DatabaseTraditional.Create(
                this,
                serviceConfiguration?.DataFilePath,
                serviceConfiguration?.LogFilePath);
        }

        public override void StartUp()
        {
            _Database.StartUp();

            // Block on waiting for input from clients
            //
            RegisterPipeServer(DatabasePipeName, (message) => ProcessQuery(message));
        }

        public override void SnapWindow()
        {
            Window.SnapRight(Process.GetCurrentProcess());
        }

        public string ProcessQuery(string message)
        {
            return _Database.ProcessQuery(query: message);
        }

        public void OverrideConfiguration(ServiceConfiguration serviceConfiguration)
        {
            if (serviceConfiguration != null)
            {
                ServiceConfiguration.Override(serviceConfiguration);
            }
            else
            {
                ServiceConfiguration = DefaultServiceConfiguration;
            }
        }
    }
}
