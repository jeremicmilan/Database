namespace Database
{
    // TODO: Deprecate
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
