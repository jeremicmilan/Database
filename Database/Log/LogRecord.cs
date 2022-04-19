using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database
{
    public abstract class LogRecord
    {
        public const string LogRecordParameterDelimiter = ",";

        protected Database Database { get => Database.Get(); }
        
        public static LogRecord ParseLogRecord(string logRecordText)
        {
            return InterpretLogRecord(logRecordText.Split(LogRecordParameterDelimiter));
        }

        protected static LogRecord InterpretLogRecord(string[] logRecordParts)
        {
            LogRecordType logRecordType = Enum.Parse<LogRecordType>(logRecordParts[0]);
            string[] parameters = logRecordParts.Skip(1).ToArray();

            return logRecordType switch
            {
                LogRecordType.TableCreate => new LogRecordTableCreate(parameters),
                LogRecordType.TableRowInsert => new LogRecordTableRowInsert(parameters),
                LogRecordType.TableRowDelete => new LogRecordTableRowDelete(parameters),
                LogRecordType.Checkpoint => new LogRecordCheckpoint(parameters),
                LogRecordType.TransactionBegin => new LogRecordTransactionBegin(),
                LogRecordType.TransactionEnd => new LogRecordTransactionEnd(),
                LogRecordType.Undo => new LogRecordUndo(parameters),
                _ => throw new Exception("Unsupported log record type"),
            };
        }

        public override string ToString() => GetLogRecordType().ToString();

        public virtual bool Equals(LogRecord other) => GetLogRecordType() == other.GetLogRecordType();

        public abstract LogRecordType GetLogRecordType();

        public abstract void Redo();

        public abstract void Undo();

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
