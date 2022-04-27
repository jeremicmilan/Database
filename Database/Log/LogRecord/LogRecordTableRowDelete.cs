namespace Database
{
    public class LogRecordTableRowDelete : LogRecordTableRowOperation
    {
        public LogRecordTableRowDelete()
        { }

        public LogRecordTableRowDelete(string tableName, int value)
            : base(tableName, value)
        { }

        public LogRecordTableRowDelete(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber, parameters)
        { }

        public override LogRecordType GetLogRecordType() => LogRecordType.TableRowDelete;

        public override void RedoRowOperation(Table table)
        {
            table.DeleteRow(Value, redo: true);
        }

        public override void UndoRowOperation(Table table)
        {
            table.InsertRow(Value, redo: true);
        }
    }
}
