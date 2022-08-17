using System;
using System.Collections.Generic;
using System.Linq;

namespace Database
{
    public abstract class StorageManager
    {
        protected List<Page> CachedPages { get; set; } = new();

        public void AddPageToCache(Page page)
        {
            if (CachedPages.Contains(page))
            {
                throw new Exception(string.Format("Page {0} already exists in the storage manager cache.", page.ToString()));
            }

            CachedPages.Add(page);
        }

        private void AddPagesToCache(List<Page> pages)
        {
            foreach (Page page in pages)
            {
                AddPageToCache(page);
            }
        }

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

        public Page GetPage(int pageId)
        {
            Page page = GetPageFromCache(pageId);

            if (page == null)
            {
                page = GetPageFromPersistedStorage(pageId);

                if (GetPageFromCache(pageId) == null)
                {
                    AddPageToCache(page);
                }
            }

            return page;
        }

        protected abstract Page GetPageFromPersistedStorage(int pageId);

        public List<Page> GetPagesForTable(string tableName)
        {
            Utility.LogOperationBegin(String.Format("Reading pages for table {0} from cache.", tableName));

            List<Page> pages = GetPagesForTableFromCache(tableName);
            if (pages != null && pages.Any())
            {
                Utility.LogOperationEnd(String.Format("Read pages for table {0} from cache.", tableName));
                foreach (Page page in pages)
                {
                    Utility.LogOperationEnd(page.ToString());
                }

                return pages;
            }
            else
            {
                Utility.LogOperationEnd(String.Format("Pages for table {0} not found in cache.", tableName));

                pages = GetPagesForTableFromPersistedStorage(tableName);
                AddPagesToCache(pages);
            }

            return pages ?? new List<Page>();
        }

        protected abstract List<Page> GetPagesForTableFromPersistedStorage(string tableName);

        public abstract void Checkpoint(int logSequenceNumber);

        public void MarkPageAsDirty(Page page)
        {
            MarkPageAsDirtyInternal(page);

            if (GetPageFromCache(page.PageId) == null)
            {
                AddPageToCache(page);
            }
        }

        protected abstract void MarkPageAsDirtyInternal(Page page);
    }
}
