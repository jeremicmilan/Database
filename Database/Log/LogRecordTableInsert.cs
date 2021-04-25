using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Database
{
    class LogRecordTableInsert : LogRecordTable
    {
        int Value;

        public LogRecordTableInsert(string[] parameters)
            : base(parameters[0])
        {
            CheckParameterLength(parameters, 2);

            Value = int.Parse(parameters[1]);
        }

        public LogRecordTableInsert(string tableName, int value)
            : base(tableName)
        {
            Value = value;
        }

        public override LogRecordType GetLogRecordType() => LogRecordType.TableInsert;

        public override void Redo()
        {
            Table table = Database.GetTable(TableName);

            if (table == null)
            {
                throw new Exception("Table not found while redoing log record: " + ToString());
            }

            table.Insert(Value, redo: true);
        }

        public override string ToString() => base.ToString() + LogRecordParameterDelimiter + Value;
    }
}
