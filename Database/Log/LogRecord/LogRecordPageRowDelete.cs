namespace Database
{
    public class LogRecordPageRowDelete : LogRecordPageOperation
    {
        public LogRecordPageRowDelete()
        { }

        public LogRecordPageRowDelete(int pageId, string tableName, int value)
            : base(pageId, tableName, value)
        { }

        public LogRecordPageRowDelete(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber, parameters)
        { }

        protected override void RedoPageOperation(Page page)
        {
            page.RemoveValue(Value, logRecord: this);
        }

        protected override void UndoPageOperation(LogRecordUndo logRecordUndo, Page page)
        {
            page.AddValue(Value, logRecord: logRecordUndo);
        }
    }
}
