using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class Database
    {
        List<Table> Tables;

        private const string DefaultLogFilePath = "..\\..\\..\\DatabaseFiles\\database.log";
        public LogManager LogManager;

        private Database(string logPath = null)
        {
            Tables = new List<Table>();
            LogManager = new LogManager(logPath ?? DefaultLogFilePath);
        }

        private static Database _Database = null;
        public static Database Get() => _Database;

        public static Database Create(string logPath = null)
        {
            if (_Database != null)
            {
                throw new Exception("There can be only one database per process.");
            }

            return _Database = new Database(logPath);
        }

        public static void Destroy()
        {
            if (_Database == null)
            {
                throw new Exception("No database to destroy.");
            }

            _Database = null;
        }

        public void StartUp()
        {
            if (File.Exists(LogManager.LogFilePath))
            {
                LogManager.ReadFromDisk();
                LogManager.RedoLog();
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

        public Table GetExistingTable(string tableName)
        {
            Table table = GetTable(tableName);
            if (table == null)
            {
                throw new Exception(string.Format("Table with name {0} does not exists.", tableName));
            }

            return table;
        }

        public string ProcessQuery(string query)
        {
            Console.WriteLine("Received query: " + query);

            const string CreateTableStatement = "CREATE TABLE ";
            const string InsertIntoTableStatementStart = "INSERT INTO ";
            const string InsertIntoTableStatementValues = "VALUES";
            const string SelectFromTableStatement = "SELECT FROM ";

            string tableName = null;
            string result = null;

            switch (query)
            {
                case string s when s.StartsWith(CreateTableStatement):
                    tableName = query.Substring(CreateTableStatement.Length).Trim();
                    if (!tableName.All(char.IsLower))
                    {
                        throw new Exception("Invalid table name.");
                    }

                    CreateTable(tableName);
                    break;

                case string s when s.StartsWith(InsertIntoTableStatementStart):
                    string insertIntoStatementPart = query.Substring(InsertIntoTableStatementStart.Length).Trim();

                    tableName = insertIntoStatementPart.Substring(0, insertIntoStatementPart.IndexOf(" "));
                    Table table = GetExistingTable(tableName);

                    insertIntoStatementPart = insertIntoStatementPart.Substring(tableName.Length).Trim();
                    if (!insertIntoStatementPart.StartsWith(InsertIntoTableStatementValues))
                    {
                        throw new Exception("Syntax error. VALUES expected.");
                    }

                    string valuesPart = insertIntoStatementPart.Substring(InsertIntoTableStatementValues.Length);

                    string[] values = valuesPart.Split(',');
                    foreach (string value in values)
                    {
                        table.Insert(int.Parse(value.Trim()));
                    }

                    break;

                case string s when s.StartsWith(SelectFromTableStatement):
                    tableName = query.Substring(SelectFromTableStatement.Length).Trim();
                    if (!tableName.All(char.IsLower))
                    {
                        throw new Exception("Invalid table name.");
                    }

                    result = GetExistingTable(tableName).Serialize();
                    break;
            }

            return result;
        }
    }
}
