namespace Database
{
    public class LogRecordTransactionRollback : LogRecordTransaction
    {
        public LogRecordTransactionRollback()
        { }

        public LogRecordTransactionRollback(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber)
        { }

        protected override void RedoInternal()
        {
            TransactionManager.Get().RollbackTransaction(redo: true);
        }
    }
}
