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
            Utility.LogMessage(LogSequenceNumberMin == null || LogSequenceNumberMin.Value == -1
                ? "Getting entire log."
                : "Getting log with min LSN: " + LogSequenceNumberMin.Value);

            List<LogRecord> logRecords = LogService.Get().LogManager.LogRecords;

            if (LogSequenceNumberMin != null)
            {
                logRecords = logRecords.SkipWhile(logRecord => logRecord.LogSequenceNumber <= LogSequenceNumberMin.Value).ToList();
            }

            if (logRecords.Any())
            {
                Utility.LogMessage("Returning log records: ");
            }
            else
            {
                Utility.LogMessage("Nothing to return.");
            }

            foreach (LogRecord logRecord in logRecords)
            {
                Utility.LogMessage("-- " + logRecord.ToString());
            }

            return new LogServiceResponseResultGetLog(logRecords);
        }
    }
}
