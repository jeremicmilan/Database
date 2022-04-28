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
            LogRecordType logRecordType = Enum.Parse<LogRecordType>(logRecordParts[1]);
            string[] parameters = logRecordParts.Skip(2).ToArray();

            return logRecordType switch
            {
                LogRecordType.TableCreate => new LogRecordTableCreate(logSequenceNumber, parameters),
                LogRecordType.TableRowInsert => new LogRecordTableRowInsert(logSequenceNumber, parameters),
                LogRecordType.TableRowDelete => new LogRecordTableRowDelete(logSequenceNumber, parameters),
                LogRecordType.Checkpoint => new LogRecordCheckpoint(logSequenceNumber, parameters),
                LogRecordType.TransactionBegin => new LogRecordTransactionBegin(logSequenceNumber),
                LogRecordType.TransactionEnd => new LogRecordTransactionEnd(logSequenceNumber),
                LogRecordType.Undo => new LogRecordUndo(logSequenceNumber, parameters),
                _ => throw new Exception("Unsupported log record type"),
            };
        }

        public override string ToString() => LogSequenceNumber + LogRecordParameterDelimiter + GetLogRecordType().ToString();

        public virtual bool Equals(LogRecord other) => GetLogRecordType() == other.GetLogRecordType();

        public abstract LogRecordType GetLogRecordType();

        public abstract void Redo();

        public virtual void Undo(LogRecordUndo logRecordUndo) => throw new Exception(GetLogRecordType() + " is not undoable.");

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
