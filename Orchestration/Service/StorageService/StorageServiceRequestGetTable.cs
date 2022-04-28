using System;

namespace Database
{
    public class StorageServiceRequestGetTable : StorageServiceRequest<StorageServiceResponseResultGetTable>
    {
        public string TableName { get; set; }
        public int LogSequenceNumber { get; set; }

        public StorageServiceRequestGetTable()
        { }

        public StorageServiceRequestGetTable(string tableName, int logSequenceNumber)
        {
            TableName = tableName;
            LogSequenceNumber = logSequenceNumber;
        }

        public override ServiceResponseResult Process()
        {
            if (LogSequenceNumber > StorageService.LogSequenceNumberMax)
            {
                StorageService.CatchUpLog(LogSequenceNumber);
            }

            return new StorageServiceResponseResultGetTable(StorageService.Get().StorageManager.GetTable(TableName));
        }
    }
}
