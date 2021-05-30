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

        public LogManager LogManager;

        private Database(string logPath = null)
        {
            Tables = new List<Table>();
            LogManager = new LogManager(logPath ?? Utility.DefaultLogFilePath);
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

        private const string CreateTableStatement = "CREATE TABLE ";
        private const string SelectFromTableStatement = "SELECT FROM ";

        private const string InsertIntoTableStatementStart = "INSERT INTO ";
        private const string CheckTableStatement = "CHECK ";
        private const string ValuesStatementPart = "VALUES";

        public string ProcessQuery(string query)
        {
            Utility.TraceDebugMessage("Received query: " + query);

            Table table = null;
            List<int> values = null;
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

            List<int> values = valuesPart.Split(',').Select(value => int.Parse(value.Trim())).ToList();

            return (table, values);
        }

        private string ParseOutValues(string partWithValues)
        {
            if (!partWithValues.StartsWith(ValuesStatementPart))
            {
                throw new Exception("Syntax error. VALUES expected.");
            }

            return partWithValues.Substring(ValuesStatementPart.Length).Trim();
        }
    }
}
