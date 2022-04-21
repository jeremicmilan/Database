﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class DatabaseHyperscale : Database
    {
        public DatabaseHyperscale(DatabaseService databaseService, string logPath)
            : base(databaseService, logPath)
        { }

        public static Database Create(
            DatabaseService databaseService,
            string logPath)
        {
            Database database = new DatabaseHyperscale(databaseService, logPath);
            Set(database);
            return database;
        }

        protected override void BootData()
        {
            // TODO: We need to read from StorageService
            //
            throw new NotImplementedException();
        }

        protected override void BootLog()
        {
            // TODO: We need to read from LogService
            //
            throw new NotImplementedException();
        }

        public override void PersistTable(Table table)
        {
            // We do not do anything to persist the table on DatabaseHyperscale,
            // as StorageService will be the one to persist the data portion of the database.
            //
        }
    }
}
