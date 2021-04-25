using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    class DatabaseService : Service
    {
        private Database _Database = null;

        public DatabaseService()
        {
            _Database = Database.Create();
        }

        public override void StartUp()
        {
            _Database.StartUp();
        }
    }
}
