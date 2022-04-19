using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class LogRecordTableCreate : LogRecordTable
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

        public override void Redo()
        {
            Database.CreateTable(TableName, redo: true);
        }

        public override void Undo()
        {
            throw new System.NotSupportedException();
        }
    }
}
