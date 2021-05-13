using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    class DatabaseService : Service
    {
        private Database _Database = null;

        public const string DatabasePipeName = "DatabasePipe";

        public DatabaseService(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        {
            _Database = Database.Create(serviceConfiguration?.LogFilePath);
        }

        public override void StartUp()
        {
            _Database.StartUp();

            RegisterPipeServer(DatabasePipeName, (message) => ProcessQuery(message));
        }

        public void ProcessQuery(string message)
        {
            _Database.ProcessQuery(query: message);
        }

        public void SetLogFilePath(string logFilePath)
        {
            ServiceConfiguration.LogFilePath = logFilePath;
        }
    }
}
