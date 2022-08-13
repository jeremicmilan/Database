namespace Database
{
    public class LogRecordPageRowInsert : LogRecordPageOperation
    {

        public LogRecordPageRowInsert()
        { }

        public LogRecordPageRowInsert(int pageId, string tableName, int value)
            : base(pageId, tableName, value)
        { }

        public LogRecordPageRowInsert(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber, parameters)
        { }

        protected override void RedoPageOperation(Page page)
        {
            page.AddValue(Value, logRecord: this);
        }

        protected override void UndoPageOperation(LogRecordUndo logRecordUndo, Page page)
        {
            page.RemoveValue(Value, logRecord: logRecordUndo);
        }
    }
}
