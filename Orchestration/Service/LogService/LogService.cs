﻿using System;
using System.Diagnostics;

namespace Database
{
    public class LogService : Service<LogServiceAction, LogServiceRequest, LogServiceResponseResult>
    {
        private const string LogServicePipeName = "LogServicePipe";
        protected override string ServicePipeName => LogServicePipeName;

        public LogService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        { }

        protected override void StartInternal()
        {
            // Log service only processes the requests from Database Service and Storage Service
            // in the current implementation. In the real world, log service would do much more.
            // One example is log destage to cheaper storage for point in time restore.
            //
        }

        public override void SnapWindow()
        {
            Window.SnapBottomRight(Process.GetCurrentProcess());
        }

        protected override LogServiceResponseResult ProcessRequest(LogServiceRequest message)
        {
            throw new NotImplementedException();
        }

        public static void SendMessageToPipe<TLogServiceRequest>(TLogServiceRequest logServiceRequest)
            where TLogServiceRequest : LogServiceRequest
        {
            SendMessageToPipe(logServiceRequest, LogServicePipeName);
        }
    }
}
