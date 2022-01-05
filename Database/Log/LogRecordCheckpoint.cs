using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public class LogRecordCheckpoint : LogRecord
    {
        public LogRecordCheckpoint(string[] parameters)
            : this(parameters.Length > 0 && bool.Parse(parameters[0]))
        { }

        public LogRecordCheckpoint(bool isTransactionActive)
        {
            IsTransactionActive = isTransactionActive;
        }

        public override LogRecordType GetLogRecordType()
        {
            return LogRecordType.Checkpoint;
        }

        public bool IsTransactionActive { get; private set; }

        public override void Redo()
        {
            // For now this is not doing anything, but it should boot server state from disk.
            // Server state contains active transactions and locks being held by those transactions.
            //
        }
    }
}
