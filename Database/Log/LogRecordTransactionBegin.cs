using System;

namespace Database
{
    public class LogRecordTransactionBegin : LogRecordTransaction
    {
        public override LogRecordType GetLogRecordType()
        {
            return LogRecordType.TransactionBegin;
        }
    }
}
