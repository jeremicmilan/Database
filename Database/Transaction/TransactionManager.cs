using System;
using System.Collections.Generic;
using System.Linq;

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
                Database.LogManager.ProcessLogRecord(logRecord);
            }
        }

        public void CommitTransaction(bool redo = false)
        {
            if (!IsTransactionActive)
            {
                throw new Exception("There is no transaction to commit.");
            }

            IsTransactionActive = false;

            if (!redo)
            {
                LogRecord logRecord = new LogRecordTransactionCommit();
                Database.LogManager.ProcessLogRecord(logRecord);
            }
        }

        public void RollbackTransaction(bool redo = false)
        {
            if (!IsTransactionActive)
            {
                throw new Exception("There is no transaction to rollback.");
            }

            IsTransactionActive = false;

            if (!redo)
            {
                Database.LogManager.UndoLog();

                LogRecord logRecord = new LogRecordTransactionRollback();
                Database.LogManager.ProcessLogRecord(logRecord);
            }
        }
    }
}
