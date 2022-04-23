namespace Database
{
    public class DatabaseServiceResponseResult : ServiceResponseResult
    {
        public Table Table { get; set; }

        public DatabaseServiceResponseResult()
        { }

        public DatabaseServiceResponseResult(Table table)
        {
            Table = table;
        }

        public override void ProcessResult()
        {
            Table.Print();
        }
    }
}
