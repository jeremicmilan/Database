using System;

namespace Database
{
    public class LogRecordTransactionBegin : LogRecordTransaction
    {
        public LogRecordTransactionBegin()
        { }

        public LogRecordTransactionBegin(int logSequenceNumber)
            : base (logSequenceNumber)
        { }

        public override void Redo()
        {
            Database.TransactionManager.BeginTransaction(redo: true);
        }

        public override LogRecordType GetLogRecordType()
        {
            return LogRecordType.TransactionBegin;
        }
    }
}
