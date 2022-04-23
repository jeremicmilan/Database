namespace Database
{
    public class DatabaseServiceRequest: ServiceRequest<DatabaseServiceAction>
    {
        public string Query { get; set; }

        public DatabaseServiceRequest()
        { }

        public DatabaseServiceRequest(DatabaseServiceAction serviceAction, string query)
            : base(serviceAction)
        {
            Query = query;
        }
    }
}
