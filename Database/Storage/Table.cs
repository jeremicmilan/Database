using System;
using System.Collections.Generic;
using System.Linq;

namespace Database
{
    public class Table
    {
        public string TableName { get; set; }

        public List<int> Values { get; set; }

        public int LogSequenceNumberMax { get; set; }

        public Table()
        { }

        public Table(string tableName, List<int> values = null, int logSequenceNumberMax = -1)
        {
            TableName = tableName;
            Values = values ?? new List<int>();
            LogSequenceNumberMax = logSequenceNumberMax;
        }

        public const string Empty = "<empty>";

        public void InsertRow(int value, LogRecord logRecord = null)
        {
            if (IsLogAlreadyApplied(logRecord))
            {
                return;
            }

            if (Values.Contains(value))
            {
                throw new Exception(string.Format("Insert failed. Value {0} already exists in table {1}.", value, TableName));
            }

            Values.Add(value);
            StorageManager.Get().MarkTableAsDirty(this);

            if (logRecord == null)
            {
                logRecord = new LogRecordTableRowInsert(TableName, value);
                LogManager.Get().PersistLogRecord(logRecord);
            }

            UpdateLogSequenceNumberMax(logRecord);
        }

        public void DeleteRow(int value, LogRecord logRecord = null)
        {
            if (IsLogAlreadyApplied(logRecord))
            {
                return;
            }

            if (!Values.Contains(value))
            {
                throw new Exception(string.Format("Delete failed. Value {0} does not exist in table {1}.", value, TableName));
            }

            Values.Remove(value);
            StorageManager.Get().MarkTableAsDirty(this);

            if (logRecord == null)
            {
                logRecord = new LogRecordTableRowDelete(TableName, value);
                LogManager.Get().PersistLogRecord(logRecord);
            }

            UpdateLogSequenceNumberMax(logRecord);
        }

        public void UpdateLogSequenceNumberMax<TLogRecord>(TLogRecord logRecord)
            where TLogRecord : LogRecord
        {
            if (LogSequenceNumberMax < logRecord.LogSequenceNumber)
            {
                LogSequenceNumberMax = logRecord.LogSequenceNumber;
            }
            else
            {
                throw new Exception(string.Format("{0} cannot be applied on table {1}.",
                    logRecord.ToString(), ToString()));
            }
        }

        public bool IsLogAlreadyApplied(LogRecord logRecord)
        {
            if (logRecord == null)
            {
                return false;
            }

            bool isLogAlreadyApplied = logRecord.LogSequenceNumber <= LogSequenceNumberMax;

            if (isLogAlreadyApplied)
            {
                Utility.LogOperationSkip("Skipping apply of log record: " + logRecord);
            }

            return isLogAlreadyApplied;
        }

        public void Print()
        {
            Utility.LogMessage(ToString());
        }

        public override string ToString()
        {
            return TableName + ":" + (Values.Any() ? string.Join(",", Values) : Empty) + ":" + LogSequenceNumberMax;
        }

        public static Table Parse(string tableString)
        {
            string[] tableStringParts = tableString.Split(":");
            string tableName = tableStringParts[0];
            string valuesString = tableStringParts[1];
            int logSequenceNumberMax = int.Parse(tableStringParts[2]);
            return new Table(tableName, ParseValues(valuesString), logSequenceNumberMax);
        }

        public static List<int> ParseValues(string valuesString, bool emptySupported = true)
        {
            if (valuesString == Empty)
            {
                if (emptySupported)
                {
                    return new List<int>();
                }
                else
                {
                    throw new Exception("Clause <empty> not supported for this statement.");
                }
            }

            return valuesString.Split(',').Select(value => int.Parse(value.Trim())).ToList();
        }

        // Note that we are currently using a suboptimal implementation of writing to file.
        // In our current implementation Table object maps to Table, Column and Page in a conventional database.
        // Table would have multiple logical columns, while on the physical level Table would have multiple Pages.
        // Pages are of a fixed size (8KB for example) making random access writes in database files optimal.
        // However, in our implementation we are reading the whole file and only changing the desired table
        // which is a line in the database file.
        //
        public void WriteToFile(string filePath)
        {
            Utility.LogOperationBegin("Writing table to disk: " + ToString());

            Utility.FileCreateIfNeeded(filePath);

            string tableString = ToString();
            string[] lines = Utility.FileReadAllLines(filePath);

            int index = Array.FindIndex(lines, (line) => Parse(line).TableName == TableName);
            if (index == -1)
            {
                lines = lines.Append(tableString).ToArray();
            }
            else
            {
                lines[index] = tableString;
            }

            Utility.FileWriteAllLines(filePath, lines);

            Utility.LogOperationBegin("Written table to disk: " + ToString());
        }
    }
}
