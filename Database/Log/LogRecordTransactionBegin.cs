using System;

namespace Database
{
    public class LogRecordTransactionBegin : LogRecordTransaction
    {
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
