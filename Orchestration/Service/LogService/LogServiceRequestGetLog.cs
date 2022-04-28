using System.Collections.Generic;
using System.Linq;

namespace Database
{
    public class LogServiceRequestGetLog : LogServiceRequest<LogServiceResponseResultGetLog>
    {
        // If specified only log after this log sequence number will be sent in response (exluding this one)
        //
        public int? LogSequenceNumberMin { get; set; }

        public LogServiceRequestGetLog()
        { }

        public LogServiceRequestGetLog(int? logSequenceNumberMin = null)
        {
            LogSequenceNumberMin = logSequenceNumberMin;
        }

        public override ServiceResponseResult Process()
        {
            List<LogRecord> logRecords = LogService.Get().LogManager.LogRecords;

            if (LogSequenceNumberMin != null)
            {
                logRecords = logRecords.SkipWhile(logRecord => logRecord.LogSequenceNumber <= LogSequenceNumberMin.Value).ToList();
            }

            return new LogServiceResponseResultGetLog(logRecords);
        }
    }
}
