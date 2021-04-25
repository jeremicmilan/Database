using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    abstract class LogRecord
    {
        public const string LogRecordParameterDelimiter = ",";

        public static LogRecord ParseLogRecord(string logRecordText)
        {
            string[] logRecordParts = logRecordText.Split(",");

            LogRecordType logRecordType = Enum.Parse<LogRecordType>(logRecordParts[0]);
            string[] parameters = new string[logRecordParts.Length - 1];
            Array.Copy(logRecordParts, parameters, 1);

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
