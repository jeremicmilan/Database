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
            StartUpDatabaseDependencies();

            _Database.StartUp();

            // Block on waiting for input from clients
            //
            RegisterPipeServer(DatabasePipeName, (message) => ProcessQuery(message));
        }

        public virtual void StartUpDatabaseDependencies() { }

        public string ProcessQuery(string message)
        {
            return _Database.ProcessQuery(query: message);
        }
    }
}
