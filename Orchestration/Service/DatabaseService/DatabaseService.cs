﻿namespace Database
{
    public abstract class DatabaseService : Service
    {
        public const string DatabaseServicePipeName = "DatabaseServicePipe";
        protected override string ServicePipeName => DatabaseServicePipeName;

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
