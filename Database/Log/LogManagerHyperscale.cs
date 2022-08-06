namespace Database
{
    public class LogManagerHyperscale : LogManager
    {
        public override void ReadEntireLog()
        {
            LogRecords.AddRange(new LogServiceRequestGetLog().Send().LogRecords);
        }

        public override void PersistLogRecord(LogRecord logRecord)
        {
            new LogServiceRequestPersistLogRecord(logRecord).Send();
        }
    }
}
