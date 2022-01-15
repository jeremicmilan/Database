using System;

namespace Database
{
    public class LogRecordTableRowInsert : LogRecordTableRowOperation
    {
        public LogRecordTableRowInsert(string[] parameters)
            : base(parameters)
        { }

        public LogRecordTableRowInsert(string tableName, int value)
            : base(tableName, value)
        { }

        public override LogRecordType GetLogRecordType() => LogRecordType.TableRowInsert;

        public override void RedoRowOperation(Table table)
        {
            table.InsertRow(Value, redo: true);
        }
    }
}
