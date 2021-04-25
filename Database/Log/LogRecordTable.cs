using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    abstract class LogRecordTable : LogRecord
    {
        string TableName;

        protected LogRecordTable (string tableName)
        {
            TableName = tableName;
        }

        public override string ToString() => base.ToString() + LogRecordParameterDelimiter + TableName;
    }
}
