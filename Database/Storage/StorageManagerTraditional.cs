using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Database
{
    public class StorageManagerTraditional : StorageManager
    {
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
                    WritePageToFile(page);
                    DirtyPages.Remove(page);
                }
            }
        }

        protected override Page GetPageFromPersistedStorage(int pageId)
        {
            Utility.LogOperationBegin(String.Format("Reading the page with id {0} from disk.", pageId));

            if (File.Exists(DataFilePath))
            {
                Page page = File.ReadAllLines(DataFilePath)
                    .Select(line => ParsePage(line))
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
                    return GetPageFromCache(pageId);
                }
                else
                {
                    Utility.LogOperationBegin(String.Format("Page with id {0} not found on disk.", pageId));
                }
            }

            return null;
        }

        protected override List<Page> GetPagesForTableFromPersistedStorage(string tableName)
        {
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
                List<Page> pages = File.ReadAllLines(DataFilePath)
                    .Select(line => ParsePage(line))
                    .Where(page => page.TableName == tableName)
                    .ToList();
                if (pages != null)
                {
                    Utility.LogOperationEnd(String.Format("Read pages for table {0} from disk.", tableName));
                    foreach (Page page in pages)
                    {
                        Utility.LogOperationEnd(page.ToString());
                    }

                    return pages;
                }
            }

            Utility.LogOperationBegin(String.Format("Pages for table {0} not found on disk.", tableName));
            return new List<Page>();
        }

        protected override void MarkPageAsDirtyInternal(Page page)
        {
            DirtyPages.Add(page);
        }

        // Note that we are currently using a suboptimal implementation of writing to file. In reality,
        // pages should be of a fixed size (8KB for example) making random access writes in database files optimal.
        // However, in our implementation we are reading the whole file and only changing the desired page
        // which is a line in the database file.
        //
        private void WritePageToFile(Page page)
        {
            Utility.LogOperationBegin("Writing page to disk: " + page.ToString());

            Utility.FileCreateIfNeeded(DataFilePath);

            string pageString = page.ToString();
            string[] lines = Utility.FileReadAllLines(DataFilePath);

            int index = Array.FindIndex(lines, (line) => ParsePage(line).PageId == page.PageId);
            if (index == -1)
            {
                lines = lines.Append(pageString).ToArray();
            }
            else
            {
                lines[index] = pageString;
            }

            Utility.FileWriteAllLines(DataFilePath, lines);

            Utility.LogOperationBegin("Written page to disk: " + page.ToString());
        }

        private Page ParsePage(string pageString)
        {
            string[] tableStringParts = pageString.Split(":");
            int pageId = int.Parse(tableStringParts[0]);
            string tableName = tableStringParts[1];
            string valuesString = tableStringParts[2];
            int logSequenceNumberMax = int.Parse(tableStringParts[3]);
            return new Page(tableName, pageId, Table.ParseValues(valuesString), logSequenceNumberMax);
        }
    }
}
