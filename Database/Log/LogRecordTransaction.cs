namespace Database
{
    public abstract class LogRecordTransaction : LogRecord
    {
        public override void Undo()
        {
            throw new System.NotSupportedException();
        }
    }
}
