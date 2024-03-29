﻿using System.Diagnostics;

namespace Database
{
    public class LogService : Service
    {
        public LogManagerTraditional LogManager { get; private set; }
        public override LogManager GetLogManager() => LogManager;

        public LogService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        {
            LogManager = new LogManagerTraditional(serviceConfiguration?.LogFilePath);
        }

        public static new LogService Get()
        {
            return Get<LogService>();
        }

        protected override void StartInternal()
        {
            // Log service only processes the requests from Database Service and Storage Service
            // in the current implementation. In the real world, log service would do much more.
            // One example is log destage to cheaper storage for point in time restore.
            // For now just read what you found on disk in case of log manager restart.
            //
            LogManager.ReadEntireLog();
        }

        public override void SnapWindow()
        {
            Window.SnapBottomRight();
        }
    }
}
