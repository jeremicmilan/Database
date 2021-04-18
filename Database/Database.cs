using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Database.Database
{
    class Database : Service
    {
        public override void StartUp()
        {
            Console.WriteLine("Database startup. Yeeey!");
        }
    }
}
