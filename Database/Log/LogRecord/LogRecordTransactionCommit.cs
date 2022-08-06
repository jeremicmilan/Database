namespace Database
{
    public class LogRecordTransactionCommit : LogRecordTransaction
    {
        public LogRecordTransactionCommit()
        { }

        public LogRecordTransactionCommit(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber)
        { }

        protected override void RedoInternal()
        {
            TransactionManager.Get().CommitTransaction(redo: true);
        }
    }
}
