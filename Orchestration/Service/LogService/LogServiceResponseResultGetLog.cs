using System.Collections.Generic;

namespace Database
{
    public class LogServiceResponseResultGetLog : LogServiceResponseResult
    {
        public List<LogRecord> LogRecords { get; set; }

        public LogServiceResponseResultGetLog()
        { }

        public LogServiceResponseResultGetLog(List<LogRecord> logRecords)
        {
            LogRecords = logRecords;
        }
    }
}
