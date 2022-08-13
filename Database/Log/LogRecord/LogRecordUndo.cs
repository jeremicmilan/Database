using System;

namespace Database
{
    public class LogRecordUndo : LogRecord
    {
        public LogRecordPage LogRecordPage { get; set; }

        public LogRecordUndo()
        { }

        public LogRecordUndo(LogRecordPage logRecordTable)
        {
            LogRecordPage = logRecordTable;
        }

        public LogRecordUndo(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber)
        {
            LogRecordPage = (LogRecordPage)InterpretLogRecord(parameters);
        }

        protected override void RedoInternal()
        {
            LogRecordPage.Undo(logRecordUndo: this);
        }

        public override string ToString() => base.ToString() + LogRecordParameterDelimiter + LogRecordPage.ToString();
    }
}
