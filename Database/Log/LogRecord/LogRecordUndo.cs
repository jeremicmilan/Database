using System;

namespace Database
{
    public class LogRecordUndo : LogRecord
    {
        public LogRecordTable LogRecordTable { get; set; }

        public LogRecordUndo()
        { }

        public LogRecordUndo(LogRecordTable logRecordTable)
        {
            LogRecordTable = logRecordTable;
        }

        public LogRecordUndo(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber)
        {
            LogRecordTable = (LogRecordTable)InterpretLogRecord(parameters);
        }

        protected override void RedoInternal()
        {
            LogRecordTable.Undo(logRecordUndo: this);
        }

        public override string ToString() => base.ToString() + LogRecordParameterDelimiter + LogRecordTable.ToString();
    }
}
