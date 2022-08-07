namespace Database
{
    public class LogRecordTransactionCommit : LogRecordTransaction
    {
        public LogRecordTransactionCommit()
        { }

        public LogRecordTransactionCommit(int logSequenceNumber, string[] _)
            : base(logSequenceNumber)
        { }

        protected override void RedoInternal()
        {
            TransactionManager.Get().CommitTransaction(redo: true);
        }
    }
}
