namespace Database
{
    public class DatabaseServiceResponseResultQuery : DatabaseServiceResponseResult
    {
        public Table Table { get; set; }

        public DatabaseServiceResponseResultQuery()
        { }

        public DatabaseServiceResponseResultQuery(Table table)
        {
            Table = table;
        }
    }
}
