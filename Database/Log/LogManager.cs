using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Database
{
    public abstract class LogManager
    {
        public readonly List<LogRecord> LogRecords = new List<LogRecord>();

        public int LogSequenceNumberMax => LogRecords.Last().LogSequenceNumber;

        protected Database Database { get => Database.Get(); }

        public abstract void ReadEntireLog();

        public void Recover()
        {
            ReadEntireLog(); // TODO: implement better

            RedoLog();
            UndoLog();
        }

        private void RedoLog()
        {
            int indexCheckpoint = LogRecords.FindLastIndex(logRecord => logRecord is LogRecordCheckpoint);
            if (indexCheckpoint < 0)
            {
                // If checkpoint not found, start redoing from the beginning of the log
                //
                indexCheckpoint = 0;
            }

            foreach (LogRecord logRecord in LogRecords.Skip(indexCheckpoint).ToList())
            {
                logRecord.Redo();
            }
        }

        private void UndoLog()
        {
            if (!Database.TransactionManager.IsTransactionActive)
            {
                return;
            }

            List<LogRecord> logRecordsToBeUndone = GetLogToBeUndone();
            List<LogRecordUndo> logRecordsUndone = GetUndoneLog();

            // Do not undo log records we have undone on the previous recovery.
            //
            foreach (LogRecordUndo logRecordUndo in logRecordsUndone)
            {
                LogRecord logRecordToBeUndone = logRecordsToBeUndone.First();

                if (!logRecordUndo.LogRecord.Equals(logRecordToBeUndone))
                {
                    throw new Exception("We did not undo properly the last time around.");
                }

                logRecordsToBeUndone.RemoveAt(0);
            }

            // Undo log.
            //
            foreach (LogRecord logRecord in logRecordsToBeUndone)
            {
                LogRecordUndo logRecordUndo = new LogRecordUndo(logRecord);
                PersistLogRecord(logRecordUndo);
                logRecordUndo.Redo();
            }

            // Complete the transaction so we can open a new one later.
            // Also, this would be signal on the recovery not to undo this part of the log again.
            //
            Database.TransactionManager.EndTransaction();
        }

        private List<LogRecord> GetLogToBeUndone()
        {
            return GetLogAfterLastBeginTransaction()
                .TakeWhile(logRecord => logRecord.GetLogRecordType() != LogRecordType.Undo)
                .Reverse()
                .ToList();
        }

        private List<LogRecordUndo> GetUndoneLog()
        {
            return GetLogAfterLastBeginTransaction()
                .SkipWhile(logRecord => logRecord.GetLogRecordType() != LogRecordType.Undo)
                .Select(logRecord => logRecord as LogRecordUndo)
                .ToList();
        }

        private List<LogRecord> GetLogAfterLastBeginTransaction()
        {
            int indexBeginTransaction = LogRecords.FindLastIndex(logRecord => logRecord is LogRecordTransactionBegin);

            // We should not get the begin transaction itself.
            //
            indexBeginTransaction++;

            return LogRecords.Skip(indexBeginTransaction).ToList();
        }

        public abstract void PersistLogRecord(LogRecord logRecord);

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine(" ---- Log Manager start ----");

            foreach (LogRecord logRecord in LogRecords)
            {
                stringBuilder.AppendLine(logRecord.ToString());
            }

            stringBuilder.AppendLine(" ---- Log Manager end ----");

            return stringBuilder.ToString();
        }
    }
}
