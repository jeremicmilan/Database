using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    class Database : Service
    {
        List<Table> Tables;
        LogManager LogManager;

        public Database ()
        {
            Tables = new List<Table>();
            LogManager = new LogManager("..\\..\\..\\DatabaseFiles\\database.log");
        }

        public override void StartUp ()
        {
            Table testTable = TableCreate("testTable");

            TableInsert(testTable, 1);
            TableInsert(testTable, 2);
            TableInsert(testTable, 3);

            testTable.Print();
        }

        void RedoLog ()
        {
            foreach (LogRecord logRecord in LogManager.LogRecords)
            {

            }
        }

        public Table TableCreate (string tableName)
        {
            Table table = new Table(tableName);
            Tables.Add(table);

            LogRecord logRecord = new LogRecordTableCreate(tableName);
            LogManager.WriteLogRecordToDisk(logRecord);

            return table;
        }

        public void TableInsert (
            Table table,
            int value)
        {
            table.AddValue(value);

            LogRecord logRecord = new LogRecordTableInsert(table.TableName, value);
            LogManager.WriteLogRecordToDisk(logRecord);
        }
    }
}
