using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Database
{
    public class LogManager
    {
        private List<LogRecord> LogRecords = new List<LogRecord>();

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

        public void RedoLogFromLastCheckpoint()
        {
            int index = LogRecords.FindLastIndex(logRecord => logRecord is LogRecordCheckpoint);

            foreach (LogRecord logRecord in LogRecords.Skip(index + 1))
            {
                logRecord.Redo();
            }
        }

        public void WriteLogRecordToDisk(LogRecord logRecord)
        {
            if (Database.ServiceConfiguration.LoggingEnabled)
            {
                using (StreamWriter streamWriter = File.AppendText(LogFilePath))
                {
                    streamWriter.WriteLine(logRecord.ToString());
                }
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
