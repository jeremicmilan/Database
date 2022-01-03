using System;

namespace Database
{
    public class LogRecordTransactionEnd : LogRecordTransaction
    {
        public override LogRecordType GetLogRecordType()
        {
            return LogRecordType.TransactionEnd;
        }

        public override void Redo()
        {
            throw new NotImplementedException();
        }
    }
}
