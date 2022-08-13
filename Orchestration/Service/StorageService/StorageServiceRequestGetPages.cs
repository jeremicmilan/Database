using System.Collections.Generic;

namespace Database
{
    public class StorageServiceRequestGetPages : StorageServiceRequest<StorageServiceResponseResultGetPages>
    {
        public string TableName { get; set; }
        public int LogSequenceNumber { get; set; }

        public StorageServiceRequestGetPages()
        { }

        public StorageServiceRequestGetPages(string tableName, int logSequenceNumber)
        {
            TableName = tableName;
            LogSequenceNumber = logSequenceNumber;
        }

        public override StorageServiceResponseResultGetPages Process()
        {
            Utility.LogServiceRequestBegin("Getting pages for table {0} with LSN {1}.", TableName, LogSequenceNumber);

            StorageService storageService = StorageService.Get();

            if (LogSequenceNumber > storageService.LogSequenceNumberMax)
            {
                storageService.CatchUpLog(LogSequenceNumber);
            }

            List<Page> pages = storageService.StorageManager.GetPagesForTable(TableName);

            if (pages != null)
            {
                Utility.LogServiceRequestEnd("Returning pages:");
                foreach (Page page in pages)
                {
                    Utility.LogServiceRequestEnd(page.ToString());
                }
            }
            else
            {
                Utility.LogServiceRequestEnd("No pages to return.");
            }

            return new StorageServiceResponseResultGetPages(pages);
        }
    }
}
