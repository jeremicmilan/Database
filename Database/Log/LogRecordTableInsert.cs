using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    class LogRecordTableInsert : LogRecordTable
    {
        int Value;

        public LogRecordTableInsert (string[] parameters)
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

        public override string ToString() => base.ToString() + LogRecordParameterDelimiter + Value;
    }
}
