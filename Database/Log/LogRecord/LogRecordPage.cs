namespace Database
{
    public abstract class LogRecordPage : LogRecord
    {
        public int PageId;

        public string TableName;

        protected LogRecordPage()
        { }

        protected LogRecordPage(int pageId, string tableName)
        {
            PageId = pageId;
            TableName = tableName;
        }

        protected LogRecordPage(int logSequenceNumber, int pageId, string tableName)
            : base(logSequenceNumber)
        {
            PageId = pageId;
            TableName = tableName;
        }

        public override string ToString() => base.ToString()
            + LogRecordParameterDelimiter + PageId
            + LogRecordParameterDelimiter + TableName;

        public override bool Equals(LogRecord other) => base.Equals(other)
            && PageId == (other as LogRecordPage).PageId
            && TableName == (other as LogRecordPage).TableName;
    }
}
