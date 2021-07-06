using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class LogRecordCheckpoint : LogRecord
    {
        public override LogRecordType GetLogRecordType()
        {
            return LogRecordType.Checkpoint;
        }

        public override void Redo()
        {
            // For now this is not doing anything, but it should boot server state from disk.
            // Server state contains active transactions and locks being held by those transactions.
            //
        }
    }
}
