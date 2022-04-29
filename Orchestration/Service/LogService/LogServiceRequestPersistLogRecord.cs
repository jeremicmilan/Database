﻿namespace Database
{
    public class LogServiceRequestPersistLogRecord : LogServiceRequest<ServiceResponseResult>
    {
        public LogRecord LogRecord { get; set; }

        public LogServiceRequestPersistLogRecord()
        { }

        public LogServiceRequestPersistLogRecord(LogRecord logRecord)
        {
            LogRecord = logRecord;
        }

        public override ServiceResponseResult Process()
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
