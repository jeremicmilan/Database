using System.Collections.Generic;

namespace Database
{
    public class StorageServiceResponseResultAllTables : StorageServiceResponseResult
    {
        public List<Table> Tables { get; set; }

        public StorageServiceResponseResultAllTables()
        { }

        public StorageServiceResponseResultAllTables(List<Table> tables)
        {
            Tables = tables;
        }
    }
}
