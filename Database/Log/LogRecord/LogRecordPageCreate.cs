namespace Database
{
    public class LogRecordPageCreate : LogRecordPage
    {
        public LogRecordPageCreate()
        { }

        public LogRecordPageCreate(int pageId, string tableName)
            : base(pageId, tableName)
        { }

        public LogRecordPageCreate(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber, int.Parse(parameters[0]), parameters[1])
        {
            CheckParameterLength(parameters, 2);
        }

        protected override void RedoInternal()
        {
            StorageManager.Get().AddPageToCache(new Page(this));
        }
    }
}
