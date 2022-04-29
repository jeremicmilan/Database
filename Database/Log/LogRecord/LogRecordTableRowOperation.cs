using System;

namespace Database
{
    public abstract class LogRecordTableRowOperation : LogRecordTable
    {
        public int Value { get; set; }

        public LogRecordTableRowOperation()
        { }

        public LogRecordTableRowOperation(string tableName, int value)
            : base(tableName)
        {
            Value = value;
        }

        public LogRecordTableRowOperation(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber, parameters[0])
        {
            CheckParameterLength(parameters, 2);

            Value = int.Parse(parameters[1]);
        }

        protected override void RedoInternal()
        {
            RedoRowOperation(GetTable());
        }

        public abstract void RedoRowOperation(Table table);

        protected override void UndoInternal(LogRecordUndo logRecordUndo)
        {
            UndoRowOperation(logRecordUndo, GetTable());
        }

        public abstract void UndoRowOperation(LogRecordUndo logRecordUndo, Table table);

        private Table GetTable()
        {
            Table table = StorageManager.Get().GetTable(TableName);

            if (table == null)
            {
                throw new Exception("Table not found while processing log record: " + ToString());
            }

            return table;
        }

        public override string ToString() => base.ToString() + LogRecordParameterDelimiter + Value;

        public override bool Equals(LogRecord other) => base.Equals(other) &&
            Value == (other as LogRecordTableRowOperation).Value;
    }
}
