using System.IO;

namespace Database
{
    public class DatabaseTraditional : Database
    {
        private DatabaseTraditional(DatabaseService databaseService, string logPath, string dataPath)
            : base(
                  databaseService,
                  new LogManagerTraditional(logPath ?? Utility.DefaultLogFilePath),
                  new DataManagerTraditional())
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

        public string DataFilePath { get; private set; }
    }
}
