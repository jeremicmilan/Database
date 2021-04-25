using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database
{
    abstract class LogRecord
    {
        public const string LogRecordParameterDelimiter = ",";

        protected Database Database { get => Database.GetDatabase(); }

        public static LogRecord ParseLogRecord(string logRecordText)
        {
            string[] logRecordParts = logRecordText.Split(LogRecordParameterDelimiter);

            LogRecordType logRecordType = Enum.Parse<LogRecordType>(logRecordParts[0]);
            string[] parameters = logRecordParts.Skip(1).ToArray();

            switch (logRecordType)
            {
                case LogRecordType.TableCreate:
                    return new LogRecordTableCreate(parameters);
                case LogRecordType.TableInsert:
                    return new LogRecordTableInsert(parameters);
                default:
                    throw new Exception("Unsupported log record type");
            }
        }

        public override string ToString() => GetLogRecordType().ToString();

        public abstract LogRecordType GetLogRecordType();

        public abstract void Redo();

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
