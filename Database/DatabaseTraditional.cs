namespace Database
{
    public class DatabaseTraditional : Database
    {
        private DatabaseTraditional(DatabaseService databaseService, string logPath, string dataPath)
            : base(
                  databaseService,
                  new LogManagerTraditional(logPath),
                  new StorageManagerTraditional(dataPath))
        { }

        public static DatabaseTraditional Create(
            DatabaseService databaseService,
            string logPath = null,
            string dataPath = null)
        {
            return new DatabaseTraditional(databaseService, dataPath, logPath);
        }
    }
}
