using System.Collections.Generic;

namespace Database
{
    public class StorageServiceRequestGetPage : StorageServiceRequest<StorageServiceResponseResultGetPage>
    {
        public int PageId { get; set; }
        public int LogSequenceNumber { get; set; }

        public StorageServiceRequestGetPage()
        { }

        public StorageServiceRequestGetPage(int pageId, int logSequenceNumber)
        {
            PageId = pageId;
            LogSequenceNumber = logSequenceNumber;
        }

        public override StorageServiceResponseResultGetPage Process()
        {
            Utility.LogServiceRequestBegin("Getting page with id {0} with LSN {1}.", PageId, LogSequenceNumber);

            StorageService storageService = StorageService.Get();

            if (LogSequenceNumber > storageService.LogSequenceNumberMax)
            {
                storageService.CatchUpLog(LogSequenceNumber);
            }

            Page page = storageService.StorageManager.GetPage(PageId);

            if (page != null)
            {
                Utility.LogServiceRequestEnd("Returning page:" + page.ToString());
            }
            else
            {
                Utility.LogServiceRequestEnd(string.Format("Page with id {0} not found.", PageId));
            }

            return new StorageServiceResponseResultGetPage(page);
        }
    }
}
