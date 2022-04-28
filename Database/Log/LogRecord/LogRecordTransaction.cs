namespace Database
{
    public abstract class LogRecordTransaction : LogRecord
    {
        protected LogRecordTransaction()
        { }

        protected LogRecordTransaction(int logSequenceNumber)
            : base(logSequenceNumber)
        { }
    }
}
