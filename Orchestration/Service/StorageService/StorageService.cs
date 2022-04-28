using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Database
{
    public class StorageService : Service
    {
        public StorageManagerTraditional StorageManager { get; private set; }

        public int LogSequenceNumberMax { get; private set; }

        public StorageService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        {
            StorageManager = new StorageManagerTraditional(serviceConfiguration?.DataFilePath);
            LogSequenceNumberMax = 0;
        }

        public static new StorageService Get()
        {
            return Get<StorageService>();
        }

        protected override void StartInternal()
        {
            // We could start a new thread here 
        }

        public void CatchUpLog(int logSequenceNumerMax)
        {
            List<LogRecord> logRecords = new LogServiceRequestGetLog(LogSequenceNumberMax).Send().LogRecords;
            foreach (LogRecord logRecord in logRecords)
            {
                if (logRecord.GetType().IsSubclassOf(typeof(LogRecordTable)) ||
                    logRecord is LogRecordUndo)
                {
                    logRecord.Redo();
                }

                LogSequenceNumberMax = logRecord.LogSequenceNumber;
            }

            if (logSequenceNumerMax > LogSequenceNumberMax)
            {
                throw new Exception(string.Format("We caught up log redo up to {0}, but we needed up to {1}",
                    LogSequenceNumberMax, logSequenceNumerMax));
            }
        }

        public override void SnapWindow()
        {
            Window.SnapBottomLeft(Process.GetCurrentProcess());
        }
    }
}
