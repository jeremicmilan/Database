using System;

namespace Database
{
    public class LogRecordUndo : LogRecord
    {
        public LogRecord LogRecord { get; set; }

        public LogRecordUndo()
        { }

        public LogRecordUndo(LogRecord logRecord)
        {
            LogRecord = logRecord;
        }

        public LogRecordUndo(int logSequenceNumber, string[] parameters)
            : base(logSequenceNumber)
        {
            LogRecord = InterpretLogRecord(parameters);
        }

        public override LogRecordType GetLogRecordType()
        {
            return LogRecordType.Undo;
        }

        public override void Redo()
        {
            LogRecord.Undo();
        }

        public override void Undo()
        {
            throw new NotSupportedException();
        }

        public override string ToString() => base.ToString() + LogRecordParameterDelimiter + LogRecord.ToString();
    }
}
