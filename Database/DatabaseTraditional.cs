using System.IO;

namespace Database
{
    public class DatabaseTraditional : Database
    {
        private DatabaseTraditional(DatabaseService databaseService, string logPath, string dataPath)
            : base(databaseService, logPath)
        {
            DataFilePath = dataPath ?? Utility.DefaultDataFilePath;
        }

        public static Database Create(
            DatabaseService databaseService,
            string logPath = null,
            string dataPath = null)
        {
            Database database = new DatabaseTraditional(databaseService, dataPath, logPath);
            Set(database);
            return database;
        }

        public string DataFilePath { get; private set; }

        protected override void BootData()
        {
            if (File.Exists(DataFilePath))
            {
                foreach (string line in File.ReadAllLines(DataFilePath))
                {
                    Table table = Table.Parse(line);
                    Tables.Add(table);
                }
            }
        }

        protected override void BootLog()
        {
            if (File.Exists(LogManager.LogFilePath))
            {
                LogManager.ReadFromDisk();
                LogManager.Recover();
            }
        }

        public override void PersistTable(Table table)
        {
            table.WriteToFile(DataFilePath);
        }
    }
}
