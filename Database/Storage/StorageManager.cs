using System;
using System.Collections.Generic;
using System.Linq;

namespace Database
{
    public abstract class StorageManager
    {
        protected readonly List<Table> CachedTables = new List<Table>();

        public static StorageManager Get() => Service.Get().GetStorageManager();

        public Table CreateTable(string tableName, LogRecordTableCreate logRecordTableCreate = null)
        {
            Table table = GetTable(tableName);
            if (table != null)
            {
                if (table.IsLogAlreadyApplied(logRecordTableCreate))
                {
                    return table;
                }

                throw new Exception(string.Format("Table with name {0} already exists.", tableName));
            }

            table = new Table(tableName);
            AddTable(table);

            if (logRecordTableCreate == null)
            {
                logRecordTableCreate = new LogRecordTableCreate(tableName);
                LogManager.Get().PersistLogRecord(logRecordTableCreate);
            }

            table.UpdateLogSequenceNumberMax(logRecordTableCreate);

            return table;
        }

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

        public abstract void Checkpoint(int logSequenceNumber);

        public virtual void MarkTableAsDirty(Table table)
        { }
    }
}
