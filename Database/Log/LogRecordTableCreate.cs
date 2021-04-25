using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    class LogRecordTableCreate : LogRecordTable
    {
        public LogRecordTableCreate(string[] parameters)
            : base(parameters[0])
        {
            CheckParameterLength(parameters, 1);
        }

        public LogRecordTableCreate(string tableName)
            : base(tableName)
        { }

        public override LogRecordType GetLogRecordType() => LogRecordType.TableCreate;
    }
}
