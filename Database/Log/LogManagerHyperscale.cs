using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class LogManagerHyperscale : LogManager
    {
        public override void PersistLogRecord(LogRecord logRecord)
        {
            new LogServiceRequestPersistLogRecord(logRecord).Send();
        }

        public override void ReadEntireLog()
        {
            LogRecords.AddRange(new LogServiceRequestReadEntireLog().Send().LogRecords);
        }
    }
}
