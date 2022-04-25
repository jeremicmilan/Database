namespace Database
{
    public class DatabaseServiceRequestQuery : DatabaseServiceRequest<DatabaseServiceResponseResultQuery>
    {
        public string Query { get; set; }

        public DatabaseServiceRequestQuery()
        { }

        public DatabaseServiceRequestQuery(string query)
        {
            Query = query;
        }

        public override ServiceResponseResult Process()
        {
            return Database.Get().ProcessQuery(Query);
        }
    }
}
