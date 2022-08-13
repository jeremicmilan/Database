using System;

namespace Database
{
    public abstract class LogRecordPageOperation : LogRecordPage
    {
        public int Value { get; set; }

        public LogRecordPageOperation()
        { }

        public LogRecordPageOperation(int pageId, string tableName, int value)
            : base(pageId, tableName)
        {
            Value = value;
        }

        public LogRecordPageOperation(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber, int.Parse(parameters[0]), parameters[1])
        {
            CheckParameterLength(parameters, 3);

            Value = int.Parse(parameters[2]);
        }

        protected override void RedoInternal()
        {
            RedoPageOperation(GetPage());
        }

        protected abstract void RedoPageOperation(Page page);

        protected override void UndoInternal(LogRecordUndo logRecordUndo)
        {
            UndoPageOperation(logRecordUndo, GetPage());
        }

        protected abstract void UndoPageOperation(LogRecordUndo logRecordUndo, Page page);

        private Page GetPage()
        {
            Page page = StorageManager.Get().GetPage(PageId);

            if (page == null)
            {
                throw new Exception("Page not found while processing log record: " + ToString());
            }

            return page;
        }

        public override string ToString() => base.ToString() + LogRecordParameterDelimiter + Value;

        public override bool Equals(LogRecord other) => base.Equals(other) &&
            Value == (other as LogRecordPageOperation).Value;
    }
}
