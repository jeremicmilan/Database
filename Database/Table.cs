using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    class Table
    {
        public string TableName { get; private set; }

        public HashSet<KeyValuePair<int, int>> Values { get; private set; }
        private int autoIncrement = 0;

        public Table(string tableName)
        {
            TableName = tableName;
            Values = new HashSet<KeyValuePair<int, int>>();
        }

        protected Database Database { get => Database.Get(); }

        public void Insert(int value, bool redo = false)
        {
            Values.Add(new KeyValuePair<int, int>(autoIncrement++, value));

            if (!redo)
            {
                LogRecord logRecord = new LogRecordTableInsert(TableName, value);
                Database.LogManager.WriteLogRecordToDisk(logRecord);
            }
        }

        public void Print()
        {
            Console.WriteLine("----- Printing table: " + TableName);

            foreach ((int key, int value) in Values)
            {
                Console.WriteLine("Key: " + key + " Value: " + value);
            }

            Console.WriteLine("---------------------");
        }
    }
}
