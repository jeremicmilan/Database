using System;
using System.Collections.Generic;
using System.Linq;

namespace Database
{
    public abstract class StorageManager
    {
        protected abstract List<Page> CachedPages { get; set; }

        protected Page GetPageFromCache(int pageId)
        {
            Utility.LogOperationBegin(String.Format("Reading the page with id {0} from cache.", pageId));

            Page page = CachedPages.FirstOrDefault(page => page.PageId == pageId);
            if (page != null)
            {
                Utility.LogOperationEnd(String.Format("Read the page with id {0} from cache.", pageId));
                return page;
            }
            else
            {
                Utility.LogOperationEnd(String.Format("Page with id {0} not found in cache.", pageId));
                return null;
            }
        }

        protected List<Page> GetPagesForTableFromCache(string tableName)
        {
            return CachedPages.Where(page => page.TableName == tableName).ToList();
        }

        public static StorageManager Get() => Service.Get().GetStorageManager();

        public abstract Page GetPage(int pageId);

        public abstract List<Page> GetPagesForTable(string tableName);

        public abstract void AddPageToCache(Page page);

        public abstract void Checkpoint(int logSequenceNumber);

        public abstract void MarkPageAsDirty(Page page);
    }
}
