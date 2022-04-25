using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class DataManagerHyperscale : DataManager
    {
        public override void AddTable(Table table)
        {
            throw new NotImplementedException();
        }

        public override void Checkpoint()
        {
            // TODO: We need to clean the dirty tables
            //
            throw new NotImplementedException();
        }

        public override Table GetTable(string tableName)
        {
            return new StorageServiceRequestGetTable(tableName).Send().Table;
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
