﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Database
{
    public class StorageService : Service
    {
        public StorageManagerTraditional StorageManager { get; private set; }
        public override StorageManager GetStorageManager() => StorageManager;

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
            List<LogRecord> logRecords = new LogServiceRequestGetLog(StorageManager.LogSequenceNumberMax).Send().LogRecords;
            foreach (LogRecord logRecord in logRecords)
            {
                if (logRecord.GetType().IsSubclassOf(typeof(LogRecordTable)) ||
                    logRecord is LogRecordCheckpoint ||
                    logRecord is LogRecordUndo)
                {
                    logRecord.Redo();
                }
            }

            if (logSequenceNumberMax > StorageManager.LogSequenceNumberMax)
            {
                throw new Exception(string.Format("We caught up log redo up to {0}, but we needed up to {1}",
                    StorageManager.LogSequenceNumberMax, logSequenceNumberMax));
            }
        }

        public override void SnapWindow()
        {
            Window.SnapBottomLeft(Process.GetCurrentProcess());
        }
    }
}
