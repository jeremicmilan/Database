using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class LogManagerHyperscale : LogManager
    {
        public override void ReadEntireLog()
        {
            LogRecords.AddRange(new LogServiceRequestReadLog().Send().LogRecords);
        }

        public override void PersistLogRecordInternal(LogRecord logRecord)
        {
            new LogServiceRequestPersistLogRecord(logRecord).Send();
        }
    }
}
