using System;
using System.Collections.Generic;
using System.Linq;

namespace Database
{
    public abstract class StorageManager
    {
        protected readonly List<Table> CachedTables = new List<Table>();

        public void AddTable(Table table)
        {
            if (GetTableFromCache(table.TableName) != null)
            {
                throw new Exception(string.Format("Cannot add table {0} to cache, as it already exists.", table.TableName));
            }

            CachedTables.Add(table);
        }

        public Table GetTable(string tableName)
        {
            Table table = GetTableFromCache(tableName);
            if (table != null)
            {
                return table;
            }

            table = GetTableFromPersistentStorage(tableName);
            if (table != null)
            {
                AddTable(table);
            }

            return table;
        }

        private Table GetTableFromCache(string tableName)
        {
            return CachedTables.Where(table => table.TableName == tableName).FirstOrDefault();
        }

        protected abstract Table GetTableFromPersistentStorage(string tableName);

        public abstract void Checkpoint();

        public virtual void MarkTableAsDirty(Table table)
        { }
    }
}
