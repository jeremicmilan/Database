using System.IO;
using System.Linq;

namespace Database
{
    public class LogManagerTraditional : LogManager
    {
        public string LogFilePath { get; private set; }

        public LogManagerTraditional(string logFilePath)
        {
            LogFilePath = logFilePath ?? Utility.DefaultLogFilePath;
        }

        public override void ReadEntireLog()
        {
            Utility.LogOperationBegin("Reading log from disk...");

            if (!File.Exists(LogFilePath))
            {
                Utility.LogOperationEnd("No log file found.");
                return;
            }

            string[] logRecordTexts = File.ReadAllLines(LogFilePath);
            foreach (string logRecordText in logRecordTexts)
            {
                LogRecords.Add(LogRecord.ParseLogRecord(logRecordText));
            }

            if (LogRecords.Any())
            {
                Utility.LogOperationEnd("Read log from disk: ");
            }
            else
            {
                Utility.LogOperationEnd("No log found in file.");
            }

            foreach (LogRecord logRecord in LogRecords)
            {
                Utility.LogOperationEnd("-- " + logRecord.ToString());
            }
        }

        public override void PersistLogRecordInternal(LogRecord logRecord)
        {
            Utility.LogOperationBegin("Writing log record to disk: " + logRecord);

            using StreamWriter streamWriter = File.AppendText(LogFilePath);
            streamWriter.WriteLine(logRecord.ToString());

            Utility.LogOperationEnd("Written log record to disk: " + logRecord);
        }
    }
}
