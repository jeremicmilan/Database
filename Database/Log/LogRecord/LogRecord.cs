using System;
using System.Linq;

namespace Database
{
    public abstract class LogRecord
    {
        public int LogSequenceNumber { get; set; }

        private static int _logSequenceNumberMax = 0;

        public const string LogRecordParameterDelimiter = ",";

        protected LogRecord()
        {
            LogSequenceNumber = ++_logSequenceNumberMax;
        }

        protected LogRecord(int logSequenceNumber)
        {
            LogSequenceNumber = logSequenceNumber;

            if (logSequenceNumber > _logSequenceNumberMax)
            {
                _logSequenceNumberMax = logSequenceNumber;
            }
        }

        public static LogRecord ParseLogRecord(string logRecordText)
        {
            return InterpretLogRecord(logRecordText.Split(LogRecordParameterDelimiter));
        }

        protected static LogRecord InterpretLogRecord(string[] logRecordParts)
        {
            int logSequenceNumber = int.Parse(logRecordParts[0]);
            Type type = Type.GetType("Database.LogRecord" + logRecordParts[1]);
            string[] parameters = logRecordParts.Skip(2).ToArray();
            LogRecord logRecord = (LogRecord)Activator.CreateInstance(type, logSequenceNumber, parameters);

            return logRecord;
        }

        public override string ToString() => LogSequenceNumber + LogRecordParameterDelimiter + GetLogRecordType();

        public virtual bool Equals(LogRecord other) => LogSequenceNumber == other.LogSequenceNumber;

        public string GetLogRecordType() => GetType().ToString()[18..];

        protected abstract void RedoInternal();
        public void Redo()
        {
            Utility.LogOperationBegin("Redoing log record {0}", ToString());
            RedoInternal();
            Utility.LogOperationEnd("Redone log record {0}", ToString());
        }

        protected virtual void UndoInternal(LogRecordUndo logRecordUndo) => throw new Exception(ToString() + " is not undoable.");
        public void Undo(LogRecordUndo logRecordUndo)
        {
            Utility.LogOperationBegin("Undoing log record {0}", ToString());
            UndoInternal(logRecordUndo);
            Utility.LogOperationEnd("Undone log record {0}", ToString());
        }

        protected void CheckParameterLength(string[] parameters, int expectedParameterCount)
        {
            if (parameters.Length != expectedParameterCount)
            {
                throw new Exception(string.Format(
                    "Expected parameter count {0}, but received {1} parameters for log record type {2}.",
                    expectedParameterCount,
                    parameters.Length,
                    GetLogRecordType()));
            }
        }
    }
}
