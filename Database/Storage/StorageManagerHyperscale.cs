using System;
using System.Collections.Generic;
using System.Linq;

namespace Database
{
    public class StorageManagerHyperscale : StorageManager
    {
        protected override List<Page> CachedPages
        {
            get
            {
                return Database.Get().CachedTables.SelectMany(table => table.Pages).ToList();
            }
            set
            {
                throw new Exception("CachedPages are readonly in StorageManagerHyperscale");
            }
        }

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

        private static int LogSequenceNumberMax => Database.Get().LogManager.LogSequenceNumberMax;

        public override Page GetPage(int pageId)
        {
            return GetPageFromCache(pageId)
                ?? new StorageServiceRequestGetPage(pageId, LogSequenceNumberMax).Send().Page;
        }

        public override List<Page> GetPagesForTable(string tableName)
        {
            return new StorageServiceRequestGetPages(tableName, LogSequenceNumberMax).Send().Pages;
        }

        public override void AddPageToCache(Page page)
        {
            Table table = Database.Get().GetTable(page.TableName);
            if (table == null || !table.Pages.Any(p => p.PageId == page.PageId))
            {
                throw new Exception("The table with all the pages should have already been created.");
            }
        }

        public override void MarkPageAsDirty(Page page)
        {
            // In Hyperscale, nothing needs to be done here as storage service is the one responsible for persisting the data.
            // and whenever the database service process needs a page that it is not in memory anymore, it will get it from
            // storage service.
            //
        }
    }
}