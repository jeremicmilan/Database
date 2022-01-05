using System;

namespace Database
{
    public class TransactionManager
    {
        // As we do not support multiple users at a single time
        // and we do not support nested transactions,
        // we only need to know whether we have started the transaction or not.
        //
        public bool isTransactionActive { get; private set; }

        public void BeginTransaction(bool redo = false)
        {
            if (isTransactionActive)
            {
                throw new Exception("There can be only one active transaction.");
            }

            isTransactionActive = true;

            if (!redo)
            {
                LogRecord logRecord = new LogRecordTransactionBegin();
                Database.Get().LogManager.WriteLogRecordToDisk(logRecord);
            }
        }

        public void EndTransaction(bool redo = false)
        {
            if (!isTransactionActive)
            {
                throw new Exception("There is no transaction to end");
            }

            isTransactionActive = false;

            if (!redo)
            {
                LogRecord logRecord = new LogRecordTransactionEnd();
                Database.Get().LogManager.WriteLogRecordToDisk(logRecord);
            }
        }
    }
}
