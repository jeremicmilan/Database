namespace Database
{
    public class LogRecordTransactionBegin : LogRecordTransaction
    {
        public LogRecordTransactionBegin()
        { }

        public LogRecordTransactionBegin(int logSequenceNumber)
            : base (logSequenceNumber)
        { }

        public override void Redo()
        {
            TransactionManager.Get().BeginTransaction(redo: true);
        }

        public override LogRecordType GetLogRecordType()
        {
            return LogRecordType.TransactionBegin;
        }
    }
}
