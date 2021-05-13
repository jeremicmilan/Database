using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    class DatabaseService : Service
    {
        private Database _Database = null;

        public const string DatabasePipeName = "DatabasePipe";
        public const string SetLogPathStatement = "SET LOG PATH = ";

        public DatabaseService()
        {
            _Database = Database.Create();
        }

        public override void StartUp()
        {
            _Database.StartUp();

            RegisterPipeServer(DatabasePipeName, (message) => ProcessQuery(message));
        }

        public void ProcessQuery(string message)
        {
            if (message.StartsWith(SetLogPathStatement))
            {
                string logPath = message.Substring(SetLogPathStatement.Length).Trim();

                Database.Destroy();
                _Database = Database.Create(logPath);
            }
            else
            {
                _Database.ProcessQuery(query: message);
            }
        }
    }
}
