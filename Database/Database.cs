using System;
using System.Collections.Generic;
using System.Linq;

namespace Database
{
    public abstract class Database
    {
        #region Database Lifecycle

        public LogManager LogManager { get; private set; }

        public TransactionManager TransactionManager { get; private set; }

        public StorageManager StorageManager { get; private set; }

        public DatabaseService DatabaseService { get; private set; }

        public static ServiceConfiguration ServiceConfiguration => Get().DatabaseService.ServiceConfiguration;

        protected Database(
            DatabaseService databaseService,
            LogManager logManager,
            StorageManager dataManager)
        {
            DatabaseService = databaseService;
            LogManager = logManager;
            StorageManager = dataManager;
            TransactionManager = new TransactionManager();
        }

        public static Database Get() => DatabaseService.Get().Database;

        public void Start()
        {
            LogManager.Recover();
        }

        #endregion Database Lifecycle

        #region Checkpoint and Disk Operations

        // In a production system Checkpoint would have been performed automatically as part of some
        // background thread. In our case, we will only allow user to perform Checkpoint due to simplicity.
        // This way we can get away for doing a lot of licking as everything should be single threaded.
        //
        private void Checkpoint()
        {
            LogRecordCheckpoint logRecordCheckpoint = new LogRecordCheckpoint(TransactionManager.IsTransactionActive);
            LogManager.PersistLogRecord(logRecordCheckpoint);
            StorageManager.Checkpoint(logRecordCheckpoint.LogSequenceNumber);
        }

        #endregion Checkpoint and Disk Operations

        #region Table Operations

        public Table GetExistingTable(string tableName)
        {
            Table table = StorageManager.GetTable(tableName);
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

        public DatabaseServiceResponseResultQuery ProcessQuery(string query)
        {
            Utility.LogDebugMessage("Received query: " + query);

            Table table = null;
            List<int> values = null, extraElementsInTable = null, extraElementsInValues = null;
            string tableName = null;
            DatabaseServiceResponseResultQuery databaseServiceResponseResultQuery = null;

            switch (query.Trim())
            {
                case string s when s.StartsWith(CreateTableStatement):
                    Utility.LogOperationBegin("Creating table: " + tableName);

                    tableName = query[CreateTableStatement.Length..].Trim();
                    if (!tableName.All(char.IsLower))
                    {
                        throw new Exception("Invalid table name.");
                    }

                    StorageManager.CreateTable(tableName);

                    Utility.LogOperationEnd("Created table: " + tableName);
                    break;

                case string s when s.StartsWith(InsertIntoTableStatementStart):
                    (table, values) = ParseTableRowStatement(query, InsertIntoTableStatementStart,emptySupported: false);
                    Utility.LogOperationBegin(string.Format("Adding [{0}] to table {1}", string.Join(", ", values), table.TableName));

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

                    Utility.LogOperationEnd(string.Format("Added [{0}] to table {1}", string.Join(", ", values), table.TableName));
                    break;

                case string s when s.StartsWith(DeleteFromTableStatementStart):
                    (table, values) = ParseTableRowStatement(query, DeleteFromTableStatementStart, emptySupported: false);
                    Utility.LogOperationBegin(string.Format("Deleting [{0}] from table {1}", string.Join(", ", values), table.TableName));

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

                    Utility.LogOperationEnd(string.Format("Deleted [{0}] from table {1}", string.Join(", ", values), table.TableName));
                    break;

                case string s when s.StartsWith(CheckTableStatement):
                    (table, values) = ParseTableRowStatement(query, CheckTableStatement, emptySupported: true);
                    Utility.LogOperationBegin(string.Format("Checking [{0}] in table {1}", string.Join(", ", values), table.TableName));

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

                    Utility.LogOperationEnd(string.Format("Checked [{0}] in table {1}", string.Join(", ", values), table.TableName));
                    break;

                case string s when s.StartsWith(SelectFromTableStatement):
                    tableName = query[SelectFromTableStatement.Length..].Trim();
                    Utility.LogOperationBegin(string.Format("Selecting table: ", tableName));

                    if (!tableName.All(char.IsLower))
                    {
                        throw new Exception("Invalid table name.");
                    }

                    databaseServiceResponseResultQuery = new DatabaseServiceResponseResultQuery(GetExistingTable(tableName));

                    Utility.LogOperationEnd(string.Format("Returning table: ", databaseServiceResponseResultQuery.Table));
                    break;

                case CheckpointStatement:
                    Utility.LogOperationBegin("Checkpoint started.");
                    Checkpoint();
                    Utility.LogOperationEnd("Checkpoint done.");
                    break;

                case TransactionStatementBegin:
                    Utility.LogOperationBegin("Transaction started.");
                    TransactionManager.BeginTransaction();
                    break;

                case TransactionStatementEnd:
                    TransactionManager.EndTransaction();
                    Utility.LogOperationEnd("Transaction ended.");
                    break;

                default:
                    throw new Exception("Syntax error.");
            }

            return databaseServiceResponseResultQuery;
        }

        private (Table, List<int>) ParseTableRowStatement(string query, string prefix, bool emptySupported)
        {
            string statementPart = query[prefix.Length..].Trim();

            string tableName = statementPart[..statementPart.IndexOf(" ")];
            Table table = GetExistingTable(tableName);

            statementPart = statementPart[tableName.Length..].Trim();
            string valuesPart = ParseOutValues(statementPart);

            return (table, Table.ParseValues(valuesPart, emptySupported));
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
