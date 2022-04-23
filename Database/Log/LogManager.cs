using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Database
{
    public class LogManager
    {
        private readonly List<LogRecord> LogRecords = new List<LogRecord>();

        public int LogSequenceNumberMax => LogRecords.Last().LogSequenceNumber;

        public string LogFilePath { get; private set; }

        public LogManager(string logFilePath)
        {
            LogFilePath = logFilePath;
        }

        public void ReadFromDisk()
        {
            string[] logRecordTexts = File.ReadAllLines(LogFilePath);

            foreach (string logRecordText in logRecordTexts)
            {
                LogRecords.Add(LogRecord.ParseLogRecord(logRecordText));
            }
        }

        public void Recover()
        {
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
            if (!Database.Get().TransactionManager.IsTransactionActive)
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
            Database.Get().TransactionManager.EndTransaction();
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

            public void PersistLogRecord(LogRecord logRecord)
        {
            if (Database.ServiceConfiguration.LoggingEnabled ?? false)
            {
                using StreamWriter streamWriter = File.AppendText(LogFilePath);
                streamWriter.WriteLine(logRecord.ToString());
            }
        }

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
