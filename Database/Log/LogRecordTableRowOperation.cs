using System;

namespace Database
{
    public abstract class LogRecordTableRowOperation : LogRecordTable
    {
        protected int Value;


        public LogRecordTableRowOperation(string[] parameters)
            : base(parameters[0])
        {
            CheckParameterLength(parameters, 2);

            Value = int.Parse(parameters[1]);
        }

        public LogRecordTableRowOperation(string tableName, int value)
            : base(tableName)
        {
            Value = value;
        }

        public abstract void RedoRowOperation(Table table);

        public override void Redo()
        {
            Table table = Database.GetTable(TableName);

            if (table == null)
            {
                throw new Exception("Table not found while redoing log record: " + ToString());
            }

            RedoRowOperation(table);
        }

        public override string ToString() => base.ToString() + LogRecordParameterDelimiter + Value;
    }
}
