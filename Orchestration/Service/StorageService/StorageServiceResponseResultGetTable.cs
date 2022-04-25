using System.Collections.Generic;

namespace Database
{
    public class StorageServiceResponseResultGetTable : StorageServiceResponseResult
    {
        public Table Table { get; set; }

        public StorageServiceResponseResultGetTable()
        { }

        public StorageServiceResponseResultGetTable(Table table)
        {
            Table = table;
        }
    }
}
