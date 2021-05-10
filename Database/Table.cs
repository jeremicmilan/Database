using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    class Table
    {
        public string TableName { get; private set; }

        public List<int> Values { get; private set; }

        public Table(string tableName)
        {
            TableName = tableName;
            Values = new List<int>();
        }

        protected Database Database { get => Database.Get(); }

        public void Insert(int value, bool redo = false)
        {
            if (Values.Contains(value))
            {
                throw new Exception(string.Format("Insert failed. Value {0} already exists in table {1}.", value, TableName));
            }

            Values.Add(value);

            if (!redo)
            {
                LogRecord logRecord = new LogRecordTableInsert(TableName, value);
                Database.LogManager.WriteLogRecordToDisk(logRecord);
            }
        }

        public void Print()
        {
            Console.WriteLine("----- Printing table: " + TableName);

            foreach (int value in Values)
            {
                Console.WriteLine("Value: " + value);
            }

            Console.WriteLine("---------------------");
        }
    }
}
