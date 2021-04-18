using System;
using System.Collections.Generic;
using System.Text;

namespace Database.Database
{
    class Table
    {
        public string TableName { get; private set; }

        public HashSet<KeyValuePair<int, int>> Values { get; private set; }
        private int autoIncrement = 0;

        public Table (string tableName)
        {
            TableName = tableName;
            Values = new HashSet<KeyValuePair<int, int>>();
        }

        public void AddValue (int value)
        {
            Values.Add(new KeyValuePair<int, int>(autoIncrement++, value));
        }

        public void Print ()
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
