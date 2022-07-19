namespace Database
{
    public class LogRecordTransactionBegin : LogRecordTransaction
    {
        public LogRecordTransactionBegin()
        { }

        public LogRecordTransactionBegin(int logSequenceNumber, string[] parameters)
            : base (logSequenceNumber)
        { }

        protected override void RedoInternal()
        {
            TransactionManager.Get().BeginTransaction(redo: true);
        }
    }
}
