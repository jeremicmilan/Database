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

        public override StorageServiceResponseResultGetTable Process()
        {
            Utility.LogServiceRequestBegin("Getting table {0} with LSN {1}.", TableName, LogSequenceNumber);

            StorageService storageService = StorageService.Get();

            if (LogSequenceNumber > storageService.LogSequenceNumberMax)
            {
                storageService.CatchUpLog(LogSequenceNumber);
            }

            Table table = storageService.StorageManager.GetTable(TableName);

            if (table != null)
            {
                Utility.LogServiceRequestEnd("Returning table {0}", table);
            }
            else
            {
                Utility.LogServiceRequestEnd("No table to return.");
            }

            return new StorageServiceResponseResultGetTable(table);
        }
    }
}
