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
            StorageService storageService = StorageService.Get();

            if (LogSequenceNumber > storageService.StorageManager.LogSequenceNumberMax)
            {
                storageService.CatchUpLog(LogSequenceNumber);
            }

            return new StorageServiceResponseResultGetTable(storageService.StorageManager.GetTable(TableName));
        }
    }
}
