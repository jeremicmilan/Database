using System;

namespace Database
{
    public class TransactionManager
    {
        // As we do not support multiple users at a single time
        // and we do not support nested transactions,
        // we only need to know whether we have started the transaction or not.
        //
        public bool IsTransactionActive { get; private set; }

        protected Database Database { get => Database.Get(); }

        public static TransactionManager Get() => Service.Get().GetTransactionManager();

        public void BeginTransaction(bool redo = false)
        {
            if (IsTransactionActive)
            {
                throw new Exception("There can be only one active transaction.");
            }

            IsTransactionActive = true;

            if (!redo)
            {
                LogRecord logRecord = new LogRecordTransactionBegin();
                Database.LogManager.PersistLogRecord(logRecord);
            }
        }

        public void EndTransaction(bool redo = false)
        {
            if (!IsTransactionActive)
            {
                throw new Exception("There is no transaction to end");
            }

            IsTransactionActive = false;

            if (!redo)
            {
                LogRecord logRecord = new LogRecordTransactionEnd();
                Database.LogManager.PersistLogRecord(logRecord);
            }
        }
    }
}
