using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database
{
    public abstract class LogManager
    {
        public readonly List<LogRecord> LogRecords = new List<LogRecord>();

        public int LogSequenceNumberMax => LogRecords.LastOrDefault()?.LogSequenceNumber ?? -1;

        public static LogManager Get() => Service.Get().GetLogManager();

        public abstract void ReadEntireLog();

        public void Recover()
        {
            // We could improve here by reading only the log section needed for recovery.
            // However, this would mean we should probably dabble log truncation, which is currently out of scope.
            //
            ReadEntireLog();

            // Redo phase
            //
            RedoLog();

            // Undo phase - rolling back the transaction will trigger log undo if necessary.
            //
            if (Database.Get().TransactionManager.IsTransactionActive)
            {
                Database.Get().TransactionManager.RollbackTransaction();
            }
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

        public void UndoLog()
        {

            List<LogRecordPage> logRecordsToBeUndone = GetLogToBeUndone();
            List<LogRecordUndo> logRecordsUndone = GetUndoneLog();

            // Do not undo log records we have undone on the previous recovery.
            //
            foreach (LogRecordUndo logRecordUndo in logRecordsUndone)
            {
                LogRecordPage logRecordToBeUndone = logRecordsToBeUndone.First();

                if (!logRecordUndo.LogRecordPage.Equals(logRecordToBeUndone))
                {
                    throw new Exception("We did not undo properly the last time around.");
                }

                logRecordsToBeUndone.RemoveAt(0);
            }

            // Undo log.
            //
            foreach (LogRecordPage logRecord in logRecordsToBeUndone)
            {
                LogRecordUndo logRecordUndo = new LogRecordUndo(logRecord);
                ProcessLogRecord(logRecordUndo);
                logRecordUndo.Redo();
            }
        }

        private List<LogRecordPage> GetLogToBeUndone()
        {
            return GetLogAfterLastBeginTransaction()
                .TakeWhile(logRecord => !(logRecord is LogRecordUndo))
                .Where(logRecord => logRecord.GetType().IsSubclassOf(typeof(LogRecordPage)))
                .Select(logRecord => (LogRecordPage)logRecord)
                .Reverse()
                .ToList();
        }

        private List<LogRecordUndo> GetUndoneLog()
        {
            return GetLogAfterLastBeginTransaction()
                .SkipWhile(logRecord => !(logRecord is LogRecordUndo))
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

        public void ProcessLogRecord(LogRecord logRecord)
        {
            if (Service.Get().ServiceConfiguration.LoggingEnabled())
            {
                PersistLogRecord(logRecord);
                LogRecords.Add(logRecord);
            }
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
