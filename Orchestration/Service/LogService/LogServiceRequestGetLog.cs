using System.Collections.Generic;
using System.Linq;

namespace Database
{
    public class LogServiceRequestGetLog : LogServiceRequest<LogServiceResponseResultGetLog>
    {
        // If specified only log after this log sequence number will be sent in response (exluding this one)
        //
        public int? LogSequenceNumberMin { get; set; }

        // If specified only log before this log sequence number will be sent in response (including this one)
        //
        public int? LogSequenceNumberMax { get; set; }

        public LogServiceRequestGetLog()
        { }

        public LogServiceRequestGetLog(int? logSequenceNumberMin = null, int ? logSequenceNumberMax = null)
        {
            LogSequenceNumberMin = logSequenceNumberMin;
            LogSequenceNumberMax = logSequenceNumberMax;
        }

        public override LogServiceResponseResultGetLog Process()
        {
            Utility.LogServiceRequestBegin(LogSequenceNumberMin == null || LogSequenceNumberMin.Value == -1
                ? "Getting entire log."
                : "Getting log with min LSN: " + LogSequenceNumberMin.Value);

            List<LogRecord> logRecords = LogService.Get().LogManager.LogRecords;

            if (LogSequenceNumberMin != null)
            {
                logRecords = logRecords
                    .SkipWhile(logRecord => logRecord.LogSequenceNumber <= LogSequenceNumberMin.Value)
                    .TakeWhile(logRecord => logRecord.LogSequenceNumber <= LogSequenceNumberMax.Value)
                    .ToList();
            }

            if (logRecords.Any())
            {
                Utility.LogServiceRequestEnd("Returning log records: ");
            }
            else
            {
                Utility.LogServiceRequestEnd("Nothing to return.");
            }

            foreach (LogRecord logRecord in logRecords)
            {
                Utility.LogServiceRequestEnd("-- " + logRecord.ToString());
            }

            return new LogServiceResponseResultGetLog(logRecords);
        }
    }
}
