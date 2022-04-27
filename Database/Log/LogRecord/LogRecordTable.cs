namespace Database
{
    public abstract class LogRecordTable : LogRecord
    {
        public string TableName;

        protected LogRecordTable()
        { }

        protected LogRecordTable(string tableName)
        {
            TableName = tableName;
        }

        protected LogRecordTable(int logSequenceNumber, string tableName)
            : base(logSequenceNumber)
        {
            TableName = tableName;
        }

        public override string ToString() => base.ToString() + LogRecordParameterDelimiter + TableName;

        public override bool Equals(LogRecord other) => base.Equals(other) &&
            TableName == (other as LogRecordTable).TableName;
    }
}
