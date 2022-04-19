namespace Database
{
    public abstract class LogRecordTransaction : LogRecord
    {
        public override void Redo()
        {
            Database.TransactionManager.EndTransaction(redo: true);
        }

        public override void Undo()
        {
            throw new System.NotSupportedException();
        }
    }
}
