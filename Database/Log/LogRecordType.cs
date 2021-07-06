using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public enum LogRecordType
    {
        TableCreate,
        TableInsert,
        Checkpoint,
    }
}
