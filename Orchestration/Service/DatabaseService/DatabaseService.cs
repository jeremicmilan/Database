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

        public abstract Database CreateDatabase(ServiceConfiguration serviceConfiguration);

        protected override void StartInternal()
        {
            Database = CreateDatabase(ServiceConfiguration);
            Database.Start();
        }
    }
}
