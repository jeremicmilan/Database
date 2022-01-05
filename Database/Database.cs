using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Database
{
    public class Database
    {
        #region Database Lifecycle

        public LogManager LogManager;

        public TransactionManager TransactionManager;

        public DatabaseService DatabaseService { get; private set; }

        public static ServiceConfiguration ServiceConfiguration => Get().DatabaseService.ServiceConfiguration;

        private Database(
            DatabaseService databaseService,
            string dataPath = null,
            string logPath = null)
        {
            DatabaseService = databaseService;
            Tables = new List<Table>();
            DataFilePath = dataPath ?? Utility.DefaultDataFilePath;
            LogManager = new LogManager(logPath ?? Utility.DefaultLogFilePath);
            TransactionManager = new TransactionManager();
        }

        private static Database _Database = null;
        public static Database Get() => _Database;

        public static Database Create(
            DatabaseService databaseService,
            string dataPath = null,
            string logPath = null)
        {
            if (_Database != null)
            {
                throw new Exception("There can be only one database per process.");
            }

            return _Database = new Database(databaseService, dataPath, logPath);
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
            if (File.Exists(DataFilePath))
            {
                foreach (string line in File.ReadAllLines(DataFilePath))
                {
                    Table table = Table.Parse(line);
                    Tables.Add(table);
                }
            }

            if (File.Exists(LogManager.LogFilePath))
            {
                LogManager.ReadFromDisk();
                LogManager.RedoLogFromLastCheckpoint();
            }
        }

        #endregion Database Lifecycle

        #region Checkpoint and Disk Operations

        public string DataFilePath { get; private set; }

        // In a production system Checkpoint would have been performed automatically as part of some
        // background thread. In our case, we will only allow user to perform Checkpoint due to simplicity.
        // This way we can get away for doing a lot of licking as everything should be single threaded.
        //
        private void Checkpoint()
        {
            foreach (Table table in Tables)
            {
                if (table.IsDirty)
                {
                    WriteTableToDisk(table);
                    table.Clean();
                }
            }

            LogRecord logRecord = new LogRecordCheckpoint(TransactionManager.isTransactionActive);
            LogManager.WriteLogRecordToDisk(logRecord);
        }

        // Note that we are currently using a suboptimal implementation of writing to file.
        // In our current implementation Table object maps to Table, Column and Page in a conventional database.
        // Table would have multiple logical columns, while on the physical level Table would have multiple Pages.
        // Pages are of a fixed size (8KB for example) making random access writes in database files optimal.
        // However, in our implementation we are reading the whole file and only changing the desired table
        // which is a line in the database file.
        //
        public void WriteTableToDisk(Table table)
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

        #endregion Checkpoint and Disk Operations

        #region Table Operations

        public List<Table> Tables;

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

        #endregion Table Operations

        #region Query Processing

        private const string CreateTableStatement = "CREATE TABLE ";
        private const string SelectFromTableStatement = "SELECT FROM ";

        private const string InsertIntoTableStatementStart = "INSERT INTO ";
        private const string CheckTableStatement = "CHECK ";
        private const string ValuesStatementPart = "VALUES";

        private const string CheckpointStatement = "CHECKPOINT";

        private const string TransactionStatementPart = " TRANSACTION";
        private const string TransactionStatementBegin = "BEGIN" + TransactionStatementPart;
        private const string TransactionStatementEnd = "END" + TransactionStatementPart;

        public string ProcessQuery(string query)
        {
            Utility.TraceDebugMessage("Received query: " + query);

            Table table = null;
            List<int> values = null;
            string tableName = null;
            string result = null;

            switch (query.Trim())
            {
                case string s when s.StartsWith(CreateTableStatement):
                    tableName = query.Substring(CreateTableStatement.Length).Trim();
                    if (!tableName.All(char.IsLower))
                    {
                        throw new Exception("Invalid table name.");
                    }

                    CreateTable(tableName);
                    Console.WriteLine("Created table: " + tableName);
                    break;

                case string s when s.StartsWith(InsertIntoTableStatementStart):
                    (table, values) = ParseInsertOrCheckTableStatement(query, InsertIntoTableStatementStart);

                    foreach (int value in values)
                    {
                        table.Insert(value);
                    }

                    Console.WriteLine(string.Format("Added [{0}] to table {1}", string.Join(", ", values), tableName));
                    break;

                case string s when s.StartsWith(CheckTableStatement):
                    (table, values) = ParseInsertOrCheckTableStatement(query, CheckTableStatement);

                    List<int> missingValues = new List<int>();
                    foreach (int tableValue in table.Values)
                    {
                        if (values.Contains(tableValue))
                        {
                            values.Remove(tableValue);
                        }
                        else
                        {
                            missingValues.Add(tableValue);
                        }
                    }

                    if (values.Any())
                    {
                        throw new Exception("Table missing elements: " + string.Join(", ", values));
                    }

                    if (missingValues.Any())
                    {
                        throw new Exception("Table contains extra elements: " + string.Join(", ", missingValues));
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

                case CheckpointStatement:
                    Checkpoint();
                    break;

                case TransactionStatementBegin:
                    TransactionManager.BeginTransaction();
                    break;

                case TransactionStatementEnd:
                    TransactionManager.EndTransaction();
                    break;

                default:
                    throw new Exception("Syntax error.");
            }

            return result;
        }

        private (Table, List<int>) ParseInsertOrCheckTableStatement(string query, string prefix)
        {
            string statementPart = query.Substring(prefix.Length).Trim();

            string tableName = statementPart.Substring(0, statementPart.IndexOf(" "));
            Table table = GetExistingTable(tableName);

            statementPart = statementPart.Substring(tableName.Length).Trim();
            string valuesPart = ParseOutValues(statementPart);

            return (table, Table.ParseValues(valuesPart));
        }

        private string ParseOutValues(string partWithValues)
        {
            if (!partWithValues.StartsWith(ValuesStatementPart))
            {
                throw new Exception("Syntax error. VALUES expected.");
            }

            return partWithValues.Substring(ValuesStatementPart.Length).Trim();
        }

        #endregion Query Processing
    }
}
