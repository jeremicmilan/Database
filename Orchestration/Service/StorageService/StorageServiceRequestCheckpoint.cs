namespace Database
{
    public class StorageServiceRequestCheckpoint : StorageServiceRequest<ServiceResponseResult>
    {
        public int LogSequenceNumber { get; set; }

        public StorageServiceRequestCheckpoint()
        { }

        public StorageServiceRequestCheckpoint(int logSequenceNumber)
        {
            LogSequenceNumber = logSequenceNumber;
        }

        public override ServiceResponseResult Process()
        {
            Utility.LogServiceRequestBegin("Processing checkpoint with LSN {0}.", LogSequenceNumber);

            StorageService storageService = StorageService.Get();

            if (LogSequenceNumber > storageService.LogSequenceNumberMax)
            {
                storageService.CatchUpLog(LogSequenceNumber);
            }

            storageService.StorageManager.Checkpoint(LogSequenceNumber);

            Utility.LogServiceRequestEnd("Processed checkpoint with LSN {0}.", LogSequenceNumber);

            return null;
        }
    }
}
