namespace Database
{
    public class StorageManagerHyperscale : StorageManager
    {
        public override void Checkpoint(int logSequenceNumber)
        {
            // We do not need to do anything here, as all of that will be handled on the storage service side automatically.
            // However, to have the same test compatibity for this prototype between traditional and hyperscale,
            // we will have signal from database service to storage service to propagate the CHECKPOINT,
            // because the tests are expecting the checkpoint to persist the data to disk.
            // In reality, storage service would have a background thread to sync log from log service and redo the log.
            // Log caught up on the log service side can lag a lot and that would not be a problem as long as the table (page)
            // is present in the buffer pool of the database service. 
            //
            new StorageServiceRequestCheckpoint(logSequenceNumber).Send();
        }

        protected override Table GetTableFromPersistentStorage(string tableName)
        {
            return new StorageServiceRequestGetTable(tableName, Database.Get().LogManager.LogSequenceNumberMax).Send().Table;
        }
    }
}