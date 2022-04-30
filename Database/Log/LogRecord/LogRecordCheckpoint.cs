using System;

namespace Database
{
    public class LogRecordCheckpoint : LogRecord
    {
        public bool IsTransactionActive { get; set; }

        public LogRecordCheckpoint()
        { }

        public LogRecordCheckpoint(int logSequenceNumber, bool isTransactionActive)
            : base(logSequenceNumber)
        {
            IsTransactionActive = isTransactionActive;
        }

        public LogRecordCheckpoint(int logSequenceNumber, string[] parameters)
            : this(logSequenceNumber, parameters.Length > 0 && bool.Parse(parameters[0]))
        { }

        public LogRecordCheckpoint(bool isTransactionActive)
        {
            IsTransactionActive = isTransactionActive;
        }

        protected override void RedoInternal()
        {
            if (IsTransactionActive)
            {
                TransactionManager.Get().BeginTransaction(redo: true);
            }
        }

        public override string ToString() => base.ToString() + LogRecordParameterDelimiter + IsTransactionActive;

        public override bool Equals(LogRecord other) => base.Equals(other) &&
            IsTransactionActive == (other as LogRecordCheckpoint).IsTransactionActive;
    }
}
