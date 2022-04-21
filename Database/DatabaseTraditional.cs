using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

        // Note that we are currently using a suboptimal implementation of writing to file.
        // In our current implementation Table object maps to Table, Column and Page in a conventional database.
        // Table would have multiple logical columns, while on the physical level Table would have multiple Pages.
        // Pages are of a fixed size (8KB for example) making random access writes in database files optimal.
        // However, in our implementation we are reading the whole file and only changing the desired table
        // which is a line in the database file.
        //
        public override void PersistTable(Table table)
        {
            Utility.FileCreateIfNeeded(DataFilePath);

            string tableString = table.ToString();
            string[] lines = Utility.FileReadAllLines(DataFilePath);

            int index = Array.FindIndex(lines, (line) => Table.Parse(line).TableName == table.TableName);
            if (index == -1)
            {
                lines = lines.Append(tableString).ToArray();
            }
            else
            {
                lines[index] = tableString;
            }

            Utility.FileWriteAllLines(DataFilePath, lines);
        }
    }
}
