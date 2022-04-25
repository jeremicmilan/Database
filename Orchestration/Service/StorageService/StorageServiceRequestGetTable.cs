using System;

namespace Database
{
    public class StorageServiceRequestGetTable : StorageServiceRequest<StorageServiceResponseResultGetTable>
    {
        public string TableName { get; set; }

        public StorageServiceRequestGetTable()
        { }

        public StorageServiceRequestGetTable(string tableName)
        {
            TableName = tableName;
        }

        public override ServiceResponseResult Process()
        {
            //StorageService.Get().;
            throw new NotImplementedException();
        }
    }
}
