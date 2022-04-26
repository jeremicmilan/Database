using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public abstract class DataManager
    {
        public abstract Table GetTable(string tableName);

        public abstract void AddTable(Table table);

        public abstract void Checkpoint();
    }
}
