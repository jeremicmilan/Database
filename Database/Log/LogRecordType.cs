using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public enum LogRecordType
    {
        TableCreate,
        TableRowInsert,
        TableRowDelete,
        Checkpoint,
        TransactionBegin,
        TransactionEnd,
    }
}
