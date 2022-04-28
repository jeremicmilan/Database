namespace Database
{
    public class LogRecordTransactionEnd : LogRecordTransaction
    {
        public LogRecordTransactionEnd()
        { }

        public LogRecordTransactionEnd(int logSequenceNumber)
            : base(logSequenceNumber)
        { }

        public override void Redo()
        {
            TransactionManager.Get().EndTransaction(redo: true);
        }

        public override LogRecordType GetLogRecordType()
        {
            return LogRecordType.TransactionEnd;
        }
    }
}
