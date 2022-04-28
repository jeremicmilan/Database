using System.IO;

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

        public override void PersistLogRecordInternal(LogRecord logRecord)
        {
            using StreamWriter streamWriter = File.AppendText(LogFilePath);
            streamWriter.WriteLine(logRecord.ToString());
        }
    }
}
