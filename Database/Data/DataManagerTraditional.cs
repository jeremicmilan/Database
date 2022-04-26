using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Database
{
    public class DataManagerTraditional : DataManager
    {
        private readonly List<Table> CachedTables = new List<Table>();

        private string DataFilePath => ((DatabaseTraditional)DatabaseService.Get().Database).DataFilePath;

        public override void Checkpoint()
        {
            foreach (Table table in CachedTables)
            {
                if (table.IsDirty)
                {
                    table.WriteToFile(DataFilePath);
                    table.Clean();
                }
            }
        }

        public override void AddTable(Table table)
        {
            CachedTables.Add(table);
        }

        public override Table GetTable(string tableName)
        {
            Table table = CachedTables.Where(table => table.TableName == tableName).FirstOrDefault();
            if (table != null)
            {
                return table;
            }

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
                table = File.ReadAllLines(DataFilePath)
                    .Select(line => Table.Parse(line))
                    .Where(table => table.TableName == tableName)
                    .FirstOrDefault();
                CachedTables.Add(table);
                return table;
            }

            return null;
        }
    }
}
