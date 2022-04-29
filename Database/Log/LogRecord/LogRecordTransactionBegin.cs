namespace Database
{
    public class LogRecordTransactionBegin : LogRecordTransaction
    {
        public LogRecordTransactionBegin()
        { }

        public LogRecordTransactionBegin(int logSequenceNumber)
            : base (logSequenceNumber)
        { }

        protected override void RedoInternal()
        {
            TransactionManager.Get().BeginTransaction(redo: true);
        }

        public override LogRecordType GetLogRecordType()
        {
            return LogRecordType.TransactionBegin;
        }
    }
}
