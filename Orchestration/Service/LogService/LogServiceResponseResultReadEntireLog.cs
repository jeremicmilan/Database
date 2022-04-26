using System.Collections.Generic;

namespace Database
{
    public class LogServiceResponseResultReadEntireLog : LogServiceResponseResult
    {
        public readonly List<LogRecord> LogRecords;

        public LogServiceResponseResultReadEntireLog()
        { }

        public LogServiceResponseResultReadEntireLog(List<LogRecord> logRecords)
        {
            LogRecords = logRecords;
        }
    }
}
