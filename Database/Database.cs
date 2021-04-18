using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Database.Database
{
    class Database : Service
    {
        List<Table> Tables;

        public Database ()
        {
            Tables = new List<Table>();
        }

        public override void StartUp ()
        {
            Table testTable = new Table("testTable");
            Tables.Add(testTable);

            testTable.AddValue(1);
            testTable.AddValue(2);
            testTable.AddValue(3);

            testTable.Print();
        }
    }
}
