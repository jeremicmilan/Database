namespace Database
{
    public class LogRecordTableCreate : LogRecordTable
    {
        public LogRecordTableCreate()
        { }

        public LogRecordTableCreate(string tableName)
            : base(tableName)
        { }

        public LogRecordTableCreate(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber, parameters[0])
        {
            CheckParameterLength(parameters, 1);
        }

        public override LogRecordType GetLogRecordType() => LogRecordType.TableCreate;

        public override void Redo()
        {
            StorageManager.Get().CreateTable(TableName, redo: true);
        }

        public override void Undo()
        {
            throw new System.NotSupportedException();
        }
    }
}
