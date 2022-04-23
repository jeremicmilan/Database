﻿namespace Database
{
    public class LogRecordCheckpoint : LogRecord
    {
        public LogRecordCheckpoint(int logSequenceNumber, string[] parameters)
            : this(logSequenceNumber, parameters.Length > 0 && bool.Parse(parameters[0]))
        { }

        public LogRecordCheckpoint(int logSequenceNumber, bool isTransactionActive)
            : base(logSequenceNumber)
        {
            IsTransactionActive = isTransactionActive;
        }

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
            if (IsTransactionActive)
            {
                Database.TransactionManager.BeginTransaction(redo: true);
            }
        }

        public override void Undo()
        {
            // Checkpoint does not redo anything.
            //
        }

        public override string ToString() => base.ToString() + LogRecordParameterDelimiter + IsTransactionActive;

        public override bool Equals(LogRecord other) => base.Equals(other) &&
            IsTransactionActive == (other as LogRecordCheckpoint).IsTransactionActive;
    }
}
