using System;

namespace Database
{
    public class LogRecordUndo : LogRecord
    {
        public LogRecord LogRecord { get; private set; }

        public LogRecordUndo(string[] parameters)
        {
            LogRecord = InterpretLogRecord(parameters);
        }

        public LogRecordUndo(LogRecord logRecord)
        {
            LogRecord = logRecord;
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
