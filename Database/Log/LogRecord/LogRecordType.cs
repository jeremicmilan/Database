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
        Undo,
    }
}
