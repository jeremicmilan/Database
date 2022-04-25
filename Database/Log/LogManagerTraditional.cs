using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Database
{
    public class LogManagerTraditional : LogManager
    {
        public string LogFilePath { get; private set; }

        public LogManagerTraditional(string logFilePath)
        {
            LogFilePath = logFilePath;
        }

        public override void ReadEntireLog()
        {
            if (!File.Exists(LogFilePath))
            {
                return;
            }

            string[] logRecordTexts = File.ReadAllLines(LogFilePath);
            foreach (string logRecordText in logRecordTexts)
            {
                LogRecords.Add(LogRecord.ParseLogRecord(logRecordText));
            }
        }

        public override void PersistLogRecord(LogRecord logRecord)
        {
            bool? loggingDisabled = Database.ServiceConfiguration.LoggingDisabled;
            bool loggingEnabled = !(loggingDisabled.HasValue && loggingDisabled.Value);
            if (loggingEnabled)
            {
                using StreamWriter streamWriter = File.AppendText(LogFilePath);
                streamWriter.WriteLine(logRecord.ToString());
            }
        }
    }
}
