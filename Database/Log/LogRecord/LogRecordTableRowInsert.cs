namespace Database
{
    public class LogRecordTableRowInsert : LogRecordTableRowOperation
    {
        public LogRecordTableRowInsert(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber, parameters)
        { }

        public LogRecordTableRowInsert(string tableName, int value)
            : base(tableName, value)
        { }

        public override LogRecordType GetLogRecordType() => LogRecordType.TableRowInsert;

        public override void RedoRowOperation(Table table)
        {
            table.InsertRow(Value, redo: true);
        }

        public override void UndoRowOperation(Table table)
        {
            table.DeleteRow(Value, redo: true);
        }
    }
}
