using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class LogManagerHyperscale : LogManager
    {
        public override void PersistLogRecord(LogRecord logRecord)
        {
            throw new NotImplementedException();
        }

        public override void ReadEntireLog()
        {
            throw new NotImplementedException();
        }
    }
}
