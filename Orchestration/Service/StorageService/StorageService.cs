using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Database
{
    public class StorageService : Service
    {
        public StorageManagerTraditional StorageManager { get; private set; }
        public override StorageManager GetStorageManager() => StorageManager;

        public int LogSequenceNumberMax { get; private set; } = -1;

        public StorageService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        {
            StorageManager = new StorageManagerTraditional(serviceConfiguration?.DataFilePath);
        }

        public static new StorageService Get()
        {
            return Get<StorageService>();
        }

        protected override void StartInternal()
        {
            // We could start a new thread to periodically apply the log from storage here,
            // but it is not needed for consistency.
            // Starting this thread will lower the latency of table delivery by storage service,
            // but we do not care about performance in this prototype.
            //
        }

        public void CatchUpLog(int logSequenceNumberMax)
        {
            Utility.LogMessage("Catching up log from LSN {0} to LSN {1}.", LogSequenceNumberMax, logSequenceNumberMax);

            List<LogRecord> logRecords = new LogServiceRequestGetLog(
                    logSequenceNumberMin: LogSequenceNumberMax,
                    logSequenceNumberMax: logSequenceNumberMax)
                .Send().LogRecords;
            foreach (LogRecord logRecord in logRecords)
            {
                if (logRecord.GetType().IsSubclassOf(typeof(LogRecordTable)) ||
                    logRecord is LogRecordUndo)
                {
                    logRecord.Redo();
                }

                LogSequenceNumberMax = logRecord.LogSequenceNumber;
            }

            if (logSequenceNumberMax > LogSequenceNumberMax)
            {
                throw new Exception(string.Format("We caught up log redo up to {0}, but we needed up to {1}",
                    LogSequenceNumberMax, logSequenceNumberMax));
            }

            Utility.LogMessage("Caught up log from LSN {0} to LSN {1}.", LogSequenceNumberMax, logSequenceNumberMax);
        }

        public override void SnapWindow()
        {
            Window.SnapBottomLeft();
        }
    }
}
