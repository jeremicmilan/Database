using System.Collections.Generic;

namespace Database
{
    public class LogServiceResponseResultReadEntireLog : LogServiceResponseResult
    {
        public List<LogRecord> LogRecords { get; set; }

        public LogServiceResponseResultReadEntireLog()
        { }

        public LogServiceResponseResultReadEntireLog(List<LogRecord> logRecords)
        {
            LogRecords = logRecords;
        }
    }
}
