namespace Database
{
    public abstract class DatabaseService : Service
    {
        public Database Database { get; set; } = null;

        public DatabaseService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        { }

        public static new DatabaseService Get()
        {
            return Get<DatabaseService>();
        }

        public override LogManager GetLogManager() => Database.LogManager;
        public override StorageManager GetStorageManager() => Database.StorageManager;
        public override TransactionManager GetTransactionManager() => Database.TransactionManager;

        public abstract Database CreateDatabase(ServiceConfiguration serviceConfiguration);

        protected override void StartInternal()
        {
            Database = CreateDatabase(ServiceConfiguration);
            Database.Start();
        }
    }
}
