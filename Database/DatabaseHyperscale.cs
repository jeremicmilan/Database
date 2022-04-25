using System;

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

        protected override void BootData()
        {
            // Ideally, we should be getting tables only needed for recovery.
            //
            StorageServiceResponseResultGetTable storageServiceResponseResultGetTable =
                new StorageServiceRequestGetTable().Send();
            Tables.Add(storageServiceResponseResultGetTable.Table);
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
            // as StorageService will be the one to persist the data portion of the database
            // automatically by syncing from the secondaries.
            //
        }
    }
}
