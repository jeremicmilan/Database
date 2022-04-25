using System;

namespace Database
{
    public abstract class LogRecordTableRowOperation : LogRecordTable
    {
        protected int Value;

        public LogRecordTableRowOperation(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber, parameters[0])
        {
            CheckParameterLength(parameters, 2);

            Value = int.Parse(parameters[1]);
        }

        public LogRecordTableRowOperation(string tableName, int value)
            : base(tableName)
        {
            Value = value;
        }

        public override void Redo()
        {
            RedoRowOperation(GetTable());
        }

        public abstract void RedoRowOperation(Table table);

        public override void Undo()
        {
            UndoRowOperation(GetTable());
        }

        public abstract void UndoRowOperation(Table table);

        private Table GetTable()
        {
            Table table = Database.GetTable(TableName);

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
