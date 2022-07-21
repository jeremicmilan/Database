namespace Database
{
    public class LogRecordTableRowInsert : LogRecordTableRowOperation
    {

        public LogRecordTableRowInsert()
        { }

        public LogRecordTableRowInsert(string tableName, int value)
            : base(tableName, value)
        { }

        public LogRecordTableRowInsert(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber, parameters)
        { }

        protected override void RedoRowOperation(Table table)
        {
            table.InsertRow(Value, logRecord: this);
        }

        protected override void UndoRowOperation(LogRecordUndo logRecordUndo, Table table)
        {
            table.DeleteRow(Value, logRecord: logRecordUndo);
        }
    }
}
