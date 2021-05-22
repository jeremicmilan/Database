using System;
using System.Collections.Generic;
using System.IO;
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

        public void RedoLog()
        {
            foreach (LogRecord logRecord in LogRecords)
            {
                logRecord.Redo();
            }
        }

        public void WriteLogRecordToDisk(LogRecord logRecord)
        {
            using (StreamWriter streamWriter = File.AppendText(LogFilePath))
            {
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
