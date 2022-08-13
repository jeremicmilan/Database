using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database
{
    public class Page
    {
        public string TableName { get; set; }

        public int PageId { get; set; }
        private static int _pageIdMax = 0;

        public List<int> Values { get; private set; } = new List<int>();
        private const int ValueCountMax = 4;
        public bool IsPageFull => Values.Count >= ValueCountMax;

        public int LogSequenceNumberMax { get; set; }

        public const string Empty = "<empty>";

        public Page()
        {
            PageId = ++_pageIdMax;
        }

        public Page(string tableName)
            : this()
        {
            TableName = tableName;
        }

        public Page(string tableName, int logSequenceNumber)
            : this(tableName)
        {
            LogSequenceNumberMax = logSequenceNumber;
        }

        public Page(LogRecordPageCreate logRecordPageCreate)
        {
            PageId = logRecordPageCreate.PageId;
            TableName = logRecordPageCreate.TableName;
            LogSequenceNumberMax = logRecordPageCreate.LogSequenceNumber;
        }

        private Page(string tableName, int pageId, List<int> values, int logSequenceNumberMax)
        {
            TableName = tableName;
            PageId = pageId;
            Values = values ?? new List<int>();
            _pageIdMax = Math.Max(_pageIdMax, pageId);
            LogSequenceNumberMax = logSequenceNumberMax;
        }

        public static Page CreateAndProcess(string tableName)
        {
            Page page = new(tableName);
            LogRecordPageCreate logRecordPageCreate = new(page.PageId, tableName);
            page.LogSequenceNumberMax = logRecordPageCreate.LogSequenceNumber;
            LogManager.Get().ProcessLogRecord(logRecordPageCreate);
            StorageManager.Get().MarkPageAsDirty(page);
            return page;
        }

        public void AddValue(int value, LogRecord logRecord = null)
        {
            PageOperation(
                preConditionCheck: () => IsPageFull,
                preConditionFailureMessage:
                    String.Format("Cannot add value {0} to a full page with id {1} on table {2}.",
                        value, PageId, TableName),
                pageOperationInMemory: () => Values.Add(value),
                logRecord: logRecord,
                instaniateLogRecord: () => new LogRecordPageRowInsert(PageId, TableName, value));
        }

        public void RemoveValue(int value, LogRecord logRecord = null)
        {
            PageOperation(
                preConditionCheck: () => !Values.Contains(value),
                preConditionFailureMessage:
                    String.Format("Cannot remove value {0} as it does not exist in the page with id {1} on table {2}.",
                        value, PageId, TableName),
                pageOperationInMemory: () => Values.Remove(value),
                logRecord: logRecord,
                instaniateLogRecord: () => new LogRecordPageRowDelete(PageId, TableName, value));
        }

        private void PageOperation(
            Func<bool> preConditionCheck,
            string preConditionFailureMessage,
            Action pageOperationInMemory,
            LogRecord logRecord,
            Func<LogRecordPage> instaniateLogRecord)
        {
            if (logRecord != null && logRecord.IsAlreadyApplied(LogSequenceNumberMax))
            {
                return;
            }

            if (preConditionCheck())
            {
                throw new Exception(preConditionFailureMessage);
            }

            pageOperationInMemory();

            if (logRecord == null)
            {
                logRecord = instaniateLogRecord();
                LogManager.Get().ProcessLogRecord(logRecord);
            }

            LogSequenceNumberMax = logRecord.LogSequenceNumber;
            StorageManager.Get().MarkPageAsDirty(this);
        }

        public override string ToString()
        {
            return PageId +
                ":" + TableName +
                ":" + (Values.Any() ? string.Join(",", Values) : Empty) +
                ":" + LogSequenceNumberMax;
        }

        public static Page Parse(string pageString)
        {
            string[] tableStringParts = pageString.Split(":");
            int pageId = int.Parse(tableStringParts[0]);
            string tableName = tableStringParts[1];
            string valuesString = tableStringParts[2];
            int logSequenceNumberMax = int.Parse(tableStringParts[3]);
            return new Page(tableName, pageId, Table.ParseValues(valuesString), logSequenceNumberMax);
        }

        // Note that we are currently using a suboptimal implementation of writing to file. In reality,
        // pages should be of a fixed size (8KB for example) making random access writes in database files optimal.
        // However, in our implementation we are reading the whole file and only changing the desired page
        // which is a line in the database file.
        //
        public void WriteToFile(string filePath)
        {
            Utility.LogOperationBegin("Writing page to disk: " + ToString());

            Utility.FileCreateIfNeeded(filePath);

            string pageString = ToString();
            string[] lines = Utility.FileReadAllLines(filePath);

            int index = Array.FindIndex(lines, (line) => Parse(line).PageId == PageId);
            if (index == -1)
            {
                lines = lines.Append(pageString).ToArray();
            }
            else
            {
                lines[index] = pageString;
            }

            Utility.FileWriteAllLines(filePath, lines);

            Utility.LogOperationBegin("Written page to disk: " + ToString());
        }
    }
}
