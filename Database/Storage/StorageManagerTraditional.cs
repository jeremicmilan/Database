using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Database
{
    public class StorageManagerTraditional : StorageManager
    {
        protected override List<Page> CachedPages { get; set; } = new();

        public override void AddPageToCache(Page page)
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

        private readonly HashSet<Page> DirtyPages = new();

        private string DataFilePath { get; set; }

        public StorageManagerTraditional(string dataFilePath)
        {
            DataFilePath = dataFilePath ?? Utility.DefaultDataFilePath;
        }

        public override void Checkpoint(int logSequenceNumber)
        {
            foreach (Page page in DirtyPages)
            {
                if (DirtyPages.Contains(page))
                {
                    page.WriteToFile(DataFilePath);
                    DirtyPages.Remove(page);
                }
            }
        }

        public override Page GetPage(int pageId)
        {
            Page page = GetPageFromCache(pageId);

            if (page == null)
            {
                Utility.LogOperationBegin(String.Format("Reading the page with id {0} from disk.", pageId));

                if (File.Exists(DataFilePath))
                {
                    page = File.ReadAllLines(DataFilePath)
                        .Select(line => Page.Parse(line))
                        .Where(page => page.PageId == pageId)
                        .FirstOrDefault();
                    if (page != null)
                    {
                        Utility.LogOperationEnd(String.Format("Read the page with id {0} from disk.", pageId));

                        // Instead of adding only this page to the cache, we are going to cache all pages
                        // for the affected table in memory to avoid consistency issues. In reality,
                        // we would not do this for performance reasons.
                        //
                        GetPagesForTable(page.TableName);

                        // In order to reference the correct object, we need to get the page from the cache,
                        // and not the one we created in this function.
                        //
                        page = GetPageFromCache(pageId);
                    }
                    else
                    {
                        Utility.LogOperationBegin(String.Format("Page with id {0} not found on disk.", pageId));
                    }
                }
            }

            return page;
        }

        public override List<Page> GetPagesForTable(string tableName)
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
                Utility.LogOperationBegin("Reading pages from disk for table: " + tableName);

                if (File.Exists(DataFilePath))
                {
                    // This is a very bad implementation as we are always reading the entire file to read one table and we do that every time.
                    // It would be better to just read the file once. However, the idea is to simulate what a DBMS would which is to read one
                    // chunk from the data file (in our case a table). To implement this better, we could add an identifier to tables,
                    // similar to what we did with log records and that will give us direct read to line needed. However, as exaplined in another
                    // comment this is very suboptimal still, as we are still ahving to scan the file to find new lines. Further optimizations
                    // are needed. The best thing to do here is to have a proper physical format and R/W directly to the file and not go through
                    // Windows APIs (eliminating all buffering and make sure it is persisted at the end of write - currently that's not the case).
                    //
                    pages = File.ReadAllLines(DataFilePath)
                        .Select(line => Page.Parse(line))
                        .Where(page => page.TableName == tableName)
                        .ToList();
                    if (pages != null)
                    {
                        Utility.LogOperationEnd(String.Format("Read pages for table {0} from disk.", tableName));
                        foreach (Page page in pages)
                        {
                            Utility.LogOperationEnd(page.ToString());
                        }

                        AddPagesToCache(pages);
                        return pages;
                    }
                }

                Utility.LogOperationBegin(String.Format("Pages for table {0} not found on disk.", tableName));
                return new List<Page>();
            }
        }

        public override void MarkPageAsDirty(Page page)
        {
            DirtyPages.Add(page);

            if (GetPageFromCache(page.PageId) == null)
            {
                AddPageToCache(page);
            }
        }
    }
}
