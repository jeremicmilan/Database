using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class DataManagerHyperscale : DataManager
    {
        public override void Checkpoint()
        {
            // We do not need to do anything here, as all of that will be handled on the page server side autoamtically.
            //
        }

        protected override Table GetTableFromPersistentStorage(string tableName)
        {
            // TODO: Read from storage service
            //
            throw new NotImplementedException();
        }
    }
}
