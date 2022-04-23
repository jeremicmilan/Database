using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Database
{
    public abstract class Database
    {
        #region Database Lifecycle

        public LogManager LogManager;

        public TransactionManager TransactionManager;

        public DatabaseService DatabaseService { get; private set; }

        public static ServiceConfiguration ServiceConfiguration => Get().DatabaseService.ServiceConfiguration;

        protected Database(
            DatabaseService databaseService,
            string logPath)
        {
            DatabaseService = databaseService;
            Tables = new List<Table>();
            LogManager = new LogManager(logPath ?? Utility.DefaultLogFilePath);
            TransactionManager = new TransactionManager();
        }

        protected static Database _Database = null;
        public static Database Get() => _Database;
        protected static void Set(Database database)
        {
            if (_Database != null)
            {
                throw new Exception("There can be only one database per process.");
            }

            _Database = database;
        }

        public void Start()
        {
            BootData();
            BootLog();
        }

        public void Stop()
        {
            if (_Database == null)
            {
                throw new Exception("No database to destroy.");
            }

            _Database = null;
        }

        protected abstract void BootData();

        protected abstract void BootLog();

        #endregion Database Lifecycle

        #region Checkpoint and Disk Operations

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
                    PersistTable(table);
                    table.Clean();
                }
            }

            LogRecord logRecord = new LogRecordCheckpoint(TransactionManager.IsTransactionActive);
            LogManager.PersistLogRecord(logRecord);
        }

        public abstract void PersistTable(Table table);

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
                LogManager.PersistLogRecord(logRecord);
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
        private const string DeleteFromTableStatementStart = "DELETE FROM ";
        private const string CheckTableStatement = "CHECK ";
        private const string ValuesStatementPart = "VALUES";

        private const string CheckpointStatement = "CHECKPOINT";

        private const string TransactionStatementPart = " TRANSACTION";
        private const string TransactionStatementBegin = "BEGIN" + TransactionStatementPart;
        private const string TransactionStatementEnd = "END" + TransactionStatementPart;

        public DatabaseServiceResponseResult ProcessQuery(string query)
        {
            Utility.TraceDebugMessage("Received query: " + query);

            Table table = null;
            List<int> values = null, extraElementsInTable = null, extraElementsInValues = null;
            string tableName = null;
            DatabaseServiceResponseResult databaseServiceResponseResult = null;

            switch (query.Trim())
            {
                case string s when s.StartsWith(CreateTableStatement):
                    tableName = query[CreateTableStatement.Length..].Trim();
                    if (!tableName.All(char.IsLower))
                    {
                        throw new Exception("Invalid table name.");
                    }

                    CreateTable(tableName);
                    Console.WriteLine("Created table: " + tableName);
                    break;

                case string s when s.StartsWith(InsertIntoTableStatementStart):
                    (table, values) = ParseTableRowStatement(query, InsertIntoTableStatementStart);

                    (extraElementsInTable, extraElementsInValues) = CompareTableValues(table, values);

                    List<int> valuesExistingInTableAlready = values.Except(extraElementsInValues).ToList();

                    if (valuesExistingInTableAlready.Any())
                    {
                        throw new Exception(string.Format("Table {0} already contains elements: {1}",
                            table.TableName, string.Join(", ", valuesExistingInTableAlready)));
                    }

                    foreach (int value in values)
                    {
                        table.InsertRow(value);
                    }

                    Console.WriteLine(string.Format("Added [{0}] to table {1}", string.Join(", ", values), tableName));
                    break;

                case string s when s.StartsWith(DeleteFromTableStatementStart):
                    (table, values) = ParseTableRowStatement(query, DeleteFromTableStatementStart);

                    (extraElementsInTable, extraElementsInValues) = CompareTableValues(table, values);

                    if (extraElementsInValues.Any())
                    {
                        throw new Exception(string.Format("Table {0} missing values for deletion: {1}",
                            table.TableName, string.Join(", ", extraElementsInValues)));
                    }

                    foreach (int value in values)
                    {
                        table.DeleteRow(value);
                    }

                    Console.WriteLine(string.Format("Added [{0}] to table {1}", string.Join(", ", values), tableName));
                    break;

                case string s when s.StartsWith(CheckTableStatement):
                    (table, values) = ParseTableRowStatement(query, CheckTableStatement);

                    (extraElementsInTable, extraElementsInValues) = CompareTableValues(table, values);

                    if (extraElementsInValues.Any())
                    {
                        throw new Exception(string.Format("Table {0} missing elements: {1}",
                            table.TableName, string.Join(", ", extraElementsInValues)));
                    }

                    if (extraElementsInTable.Any())
                    {
                        throw new Exception(string.Format("Table {0} contains extra elements: {1}",
                            table.TableName, string.Join(", ", extraElementsInTable)));
                    }

                    break;

                case string s when s.StartsWith(SelectFromTableStatement):
                    tableName = query[SelectFromTableStatement.Length..].Trim();
                    if (!tableName.All(char.IsLower))
                    {
                        throw new Exception("Invalid table name.");
                    }

                    databaseServiceResponseResult = new DatabaseServiceResponseResult(GetExistingTable(tableName));
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

            return databaseServiceResponseResult;
        }

        private (Table, List<int>) ParseTableRowStatement(string query, string prefix)
        {
            string statementPart = query[prefix.Length..].Trim();

            string tableName = statementPart[..statementPart.IndexOf(" ")];
            Table table = GetExistingTable(tableName);

            statementPart = statementPart[tableName.Length..].Trim();
            string valuesPart = ParseOutValues(statementPart);

            return (table, Table.ParseValues(valuesPart));
        }

        private string ParseOutValues(string partWithValues)
        {
            if (!partWithValues.StartsWith(ValuesStatementPart))
            {
                throw new Exception("Syntax error. VALUES expected.");
            }

            return partWithValues[ValuesStatementPart.Length..].Trim();
        }

        private (List<int>, List<int>) CompareTableValues(Table table, List<int> values)
        {
            List<int> extraElementsInTable = new List<int>();
            List<int> extraElementsInValues = new List<int>(values);

            foreach (int tableValue in table.Values)
            {
                if (extraElementsInValues.Contains(tableValue))
                {
                    extraElementsInValues.Remove(tableValue);
                }
                else
                {
                    extraElementsInTable.Add(tableValue);
                }
            }

            return (extraElementsInTable, extraElementsInValues);
        }

        #endregion Query Processing
    }
}
