namespace Database
{
    public abstract class LogRecordTransaction : LogRecord
    {
        protected LogRecordTransaction()
        { }

        protected LogRecordTransaction(int logSequenceNumber)
            : base(logSequenceNumber)
        { }

        public override void Undo()
        {
            throw new System.NotSupportedException();
        }
    }
}
