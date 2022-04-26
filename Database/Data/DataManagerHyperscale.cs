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
    }
}
