namespace Database
{
    public abstract class DatabaseService : Service
    {
        public const string DatabaseServicePipeName = "DatabaseServicePipe";
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
    }
}
