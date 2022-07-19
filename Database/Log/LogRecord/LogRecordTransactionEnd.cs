namespace Database
{
    public class LogRecordTransactionEnd : LogRecordTransaction
    {
        public LogRecordTransactionEnd()
        { }

        public LogRecordTransactionEnd(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber)
        { }

        protected override void RedoInternal()
        {
            TransactionManager.Get().EndTransaction(redo: true);
        }
    }
}
