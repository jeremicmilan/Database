namespace Database
{
    public class LogServiceRequestPersistLogRecord : LogServiceRequest<LogServiceResponseResult>
    {
        public LogRecord LogRecord { get; set; }

        public LogServiceRequestPersistLogRecord()
        { }

        public LogServiceRequestPersistLogRecord(LogRecord logRecord)
        {
            LogRecord = logRecord;
        }

        public override LogServiceResponseResult Process()
        {
            Utility.LogServiceRequestBegin("Persisting log record: " + LogRecord);

            LogManager logManager = LogService.Get().LogManager;
            logManager.LogRecords.Add(LogRecord);
            logManager.PersistLogRecord(LogRecord);

            Utility.LogServiceRequestEnd("Persisted log record: " + LogRecord);

            return null;
        }
    }
}
