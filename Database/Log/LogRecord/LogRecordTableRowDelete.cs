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

        protected override void RedoRowOperation(Table table)
        {
            table.DeleteRow(Value, logRecord: this);
        }

        protected override void UndoRowOperation(LogRecordUndo logRecordUndo, Table table)
        {
            table.InsertRow(Value, logRecord: logRecordUndo);
        }
    }
}
