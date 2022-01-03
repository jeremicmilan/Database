using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public abstract class LogRecordTable : LogRecord
    {
        public string TableName;

        protected LogRecordTable(string tableName)
        {
            TableName = tableName;
        }

        public override string ToString() => base.ToString() + LogRecordParameterDelimiter + TableName;
    }
}
