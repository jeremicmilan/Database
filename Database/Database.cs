using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    class Database
    {
        List<Table> Tables;

        private const string DefaultLogFilePath = "..\\..\\..\\DatabaseFiles\\database.log";
        public LogManager LogManager;

        private Database()
        {
            Tables = new List<Table>();
            LogManager = new LogManager(DefaultLogFilePath);
        }

        private static Database _Database = null;
        public static Database Get() => _Database;

        public static Database Create()
        {
            if (_Database != null)
            {
                throw new Exception("There can be only one database per process.");
            }

            _Database = new Database();
            return _Database;
        }

        public void StartUp()
        {
            if (File.Exists(DefaultLogFilePath))
            {
                LogManager.ReadFromDisk();
                LogManager.RedoLog();
            }
            else
            {
                Table testTable = CreateTable("testTable");
                testTable.Insert(1);
                testTable.Insert(2);
                testTable.Insert(3);
            }

            foreach (Table table in Tables)
            {
                table.Print();
            }
        }

        public Table GetTable(string tableName) => Tables.Where(table => table.TableName == tableName).FirstOrDefault();

        public Table CreateTable(string tableName, bool redo = false)
        {
            if (GetTable(tableName) != null)
            {
                throw new Exception(string.Format("Table with name {0} already exists.", tableName));
            }

            Table table = new Table(tableName);
            Tables.Add(table);

            if (!redo)
            {
                LogRecord logRecord = new LogRecordTableCreate(tableName);
                LogManager.WriteLogRecordToDisk(logRecord);
            }

            return table;
        }

        public void ProcessQuery(string query)
        {
            Console.WriteLine("Received query: " + query);
        }
    }
}
