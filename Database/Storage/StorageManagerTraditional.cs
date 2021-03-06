using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Database
{
    public class StorageManagerTraditional : StorageManager
    {
        private readonly HashSet<string> DirtyTableNames = new HashSet<string>();

        private string DataFilePath { get; set; }

        public StorageManagerTraditional(string dataFilePath)
        {
            DataFilePath = dataFilePath ?? Utility.DefaultDataFilePath;
        }

        public override void Checkpoint(int logSequenceNumber)
        {
            foreach (Table table in CachedTables)
            {
                if (DirtyTableNames.Contains(table.TableName))
                {
                    table.WriteToFile(DataFilePath);
                    DirtyTableNames.Remove(table.TableName);
                }
            }
        }

        protected override Table GetTableFromPersistentStorage(string tableName)
        {
            Utility.LogOperationBegin("Reading table from disk: " + tableName);

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
                Table table = File.ReadAllLines(DataFilePath)
                    .Select(line => Table.Parse(line))
                    .Where(table => table.TableName == tableName)
                    .FirstOrDefault();
                if (table != null)
                {
                    Utility.LogOperationBegin("Read table from disk: " + table);
                    return table;
                }
            }

            Utility.LogOperationBegin("Table on disk not found.");
            return null;
        }

        public override void MarkTableAsDirty(Table table)
        {
            DirtyTableNames.Add(table.TableName);
        }
    }
}
