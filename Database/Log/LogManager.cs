using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Database
{
    class LogManager
    {
        public List<LogRecord> LogRecords = new List<LogRecord>();

        string LogFilePath;

        public LogManager (string logFilePath)
        {
            LogFilePath = logFilePath;
        }

        public void ReadFromDisk ()
        {
            string[] logRecordTexts = File.ReadAllLines(LogFilePath);

            foreach (string logRecordText in logRecordTexts)
            {
                LogRecords.Add(LogRecord.ParseLogRecord(logRecordText));
            }
        }

        public void WriteLogRecordToDisk (LogRecord logRecord)
        {
            using (StreamWriter streamWriter = File.AppendText(LogFilePath))
            {
                streamWriter.WriteLine(logRecord.ToString());
            }
        }
    }
}
