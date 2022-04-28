namespace Database
{
    public class StorageManagerHyperscale : StorageManager
    {
        public override void Checkpoint()
        {
            // We do not need to do anything here, as all of that will be handled on the page server side autoamtically.
            //
        }

        protected override Table GetTableFromPersistentStorage(string tableName)
        {
            return new StorageServiceRequestGetTable(tableName, Database.Get().LogManager.LogSequenceNumberMax).Send().Table;
        }
    }
}