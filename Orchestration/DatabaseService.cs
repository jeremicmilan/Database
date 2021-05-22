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

            // Block on waiting for input from clients
            //
            RegisterPipeServer(DatabasePipeName, (message) => ProcessQuery(message));
        }

        public string ProcessQuery(string message)
        {
            return _Database.ProcessQuery(query: message);
        }

        public void SetLogFilePath(string logFilePath)
        {
            ServiceConfiguration.LogFilePath = logFilePath;
        }
    }
}
