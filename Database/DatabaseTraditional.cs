using System.IO;

namespace Database
{
    public class DatabaseTraditional : Database
    {
        public string DataFilePath { get; private set; }

        private DatabaseTraditional(DatabaseService databaseService, string logPath, string dataPath)
            : base(
                  databaseService,
                  new LogManagerTraditional(logPath),
                  new StorageManagerTraditional(dataPath))
        {
            DataFilePath = dataPath ?? Utility.DefaultDataFilePath;
        }

        public static DatabaseTraditional Create(
            DatabaseService databaseService,
            string logPath = null,
            string dataPath = null)
        {
            return new DatabaseTraditional(databaseService, dataPath, logPath);
        }
    }
}
