using System;

namespace Database
{
    public class LogRecordTransactionEnd : LogRecordTransaction
    {
        public override void Redo()
        {
            Database.TransactionManager.EndTransaction(redo: true);
        }

        public override LogRecordType GetLogRecordType()
        {
            return LogRecordType.TransactionEnd;
        }
    }
}
