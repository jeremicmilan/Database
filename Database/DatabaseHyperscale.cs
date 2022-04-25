﻿using System;

namespace Database
{
    public class DatabaseHyperscale : Database
    {
        public DatabaseHyperscale(DatabaseService databaseService)
            : base(
                  databaseService,
                  new LogManagerHyperscale(),
                  new DataManagerHyperscale())
        { }

        public static DatabaseHyperscale Create(DatabaseService databaseService)
        {
            return new DatabaseHyperscale(databaseService);
        }
    }
}
