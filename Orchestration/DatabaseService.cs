using System.Diagnostics;

namespace Database
{
    public abstract class DatabaseService : Service
    {
        private readonly Database _Database = null;

        public const string DatabasePipeName = "DatabasePipe";

        public DatabaseService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        {
            _Database = CreateDatabase(serviceConfiguration);
        }

        public abstract Database CreateDatabase(ServiceConfiguration serviceConfiguration);

        public override void StartUp()
        {
            StartUpDependencies();

            _Database.StartUp();

            // Block on waiting for input from clients
            //
            RegisterPipeServer(DatabasePipeName, (message) => ProcessQuery(message));
        }

        public virtual void StartUpDependencies() { }

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
