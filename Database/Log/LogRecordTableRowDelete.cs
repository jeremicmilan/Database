﻿using System;

namespace Database
{
    public class LogRecordTableRowDelete : LogRecordTableRowOperation
    {
        public LogRecordTableRowDelete(string[] parameters)
            : base(parameters)
        { }

        public LogRecordTableRowDelete(string tableName, int value)
            : base(tableName, value)
        { }

        public override LogRecordType GetLogRecordType() => LogRecordType.TableRowDelete;

        public override void RedoRowOperation(Table table)
        {
            table.DeleteRow(Value, redo: true);
        }
    }
}
