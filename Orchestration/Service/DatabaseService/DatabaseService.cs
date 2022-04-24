namespace Database
{
    public abstract class DatabaseService : Service<DatabaseServiceAction, DatabaseServiceRequest, DatabaseServiceResponseResult>
    {
        private const string DatabaseServicePipeName = "DatabaseServicePipe";
        protected override string ServicePipeName => DatabaseServicePipeName;

        private Database Database { get; set; } = null;

        public DatabaseService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        { }

        public abstract Database CreateDatabase(ServiceConfiguration serviceConfiguration);

        protected override void StartInternal()
        {
            Database = CreateDatabase(ServiceConfiguration);
            Database.Start();
        }

        public override void Stop()
        {
            base.Stop();
            Database?.Stop();
        }

        protected override DatabaseServiceResponseResult ProcessRequest(DatabaseServiceRequest databaseServiceRequest)
        {
            return Database.ProcessQuery(query: databaseServiceRequest.Query);
        }

        public static void SendMessageToPipe<TDatabaseServiceRequest>(TDatabaseServiceRequest databaseServiceRequest)
            where TDatabaseServiceRequest : DatabaseServiceRequest
        {
            SendMessageToPipe(databaseServiceRequest, DatabaseServicePipeName);
        }
    }
}
