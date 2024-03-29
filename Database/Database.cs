﻿using System;
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

        protected Database(
            DatabaseService databaseService,
            LogManager logManager,
            StorageManager storageManager)
        {
            DatabaseService = databaseService;
            LogManager = logManager;
            StorageManager = storageManager;
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
            LogRecordCheckpoint logRecordCheckpoint = new(TransactionManager.IsTransactionActive);
            LogManager.ProcessLogRecord(logRecordCheckpoint);
            StorageManager.Checkpoint(logRecordCheckpoint.LogSequenceNumber);
        }

        #endregion Checkpoint and Disk Operations

        #region Table Operations

        public readonly List<Table> CachedTables = new();

        private Table GetTableFromCache(string tableName)
        {
            return CachedTables.Where(table => table.TableName == tableName).FirstOrDefault();
        }

        public Table CreateTable(string tableName, LogRecordPageCreate logRecordPageCreate = null)
        {
            Table table = GetTable(tableName);
            if (table != null)
            {
                if (logRecordPageCreate != null && logRecordPageCreate.IsAlreadyApplied(table.LogSequenceNumberMax))
                {
                    return table;
                }
                else
                {
                    throw new Exception(string.Format("Table with name {0} already exists.", tableName));
                }
            }

            Page page = Page.CreateAndProcess(tableName);
            table = new Table(tableName, new List<Page> { page });
            AddTableToCache(table);

            return table;
        }

        private void AddTableToCache(Table table)
        {
            if (GetTableFromCache(table.TableName) != null)
            {
                throw new Exception(string.Format("Cannot add table {0} to cache, as it already exists.", table.TableName));
            }

            CachedTables.Add(table);
        }

        public Table GetTable(string tableName)
        {
            Table table = GetTableFromCache(tableName);
            if (table != null)
            {
                return table;
            }

            List<Page> pages = StorageManager.GetPagesForTable(tableName);
            if (pages.Any())
            {
                table = new Table(tableName, pages);
                AddTableToCache(table);
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
        private const string TransactionStatementCommit = "COMMIT" + TransactionStatementPart;
        private const string TransactionStatementRollback = "ROLLBACK" + TransactionStatementPart;

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
                    tableName = query[CreateTableStatement.Length..].Trim();
                    Utility.LogOperationBegin("Creating table: " + tableName);
                    if (!tableName.All(char.IsLower))
                    {
                        throw new Exception("Invalid table name.");
                    }

                    CreateTable(tableName);

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
                    Utility.LogOperationBegin(string.Format("Selecting table: {0}", tableName));

                    if (!tableName.All(char.IsLower))
                    {
                        throw new Exception("Invalid table name.");
                    }

                    databaseServiceResponseResultQuery = new DatabaseServiceResponseResultQuery(GetExistingTable(tableName));

                    Utility.LogOperationEnd(string.Format("Returning table: {0}", databaseServiceResponseResultQuery.Table));
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

                case TransactionStatementCommit:
                    TransactionManager.CommitTransaction();
                    Utility.LogOperationEnd("Transaction committed.");
                    break;

                case TransactionStatementRollback:
                    TransactionManager.RollbackTransaction();
                    Utility.LogOperationEnd("Transaction rolled back.");
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

        private static string ParseOutValues(string partWithValues)
        {
            if (!partWithValues.StartsWith(ValuesStatementPart))
            {
                throw new Exception("Syntax error. VALUES expected.");
            }

            return partWithValues[ValuesStatementPart.Length..].Trim();
        }

        private static (List<int>, List<int>) CompareTableValues(Table table, List<int> values)
        {
            List<int> extraElementsInTable = new();
            List<int> extraElementsInValues = new(values);

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
