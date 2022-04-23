namespace Database
{
    public class LogServiceRequest : ServiceRequest<LogServiceAction>
    {
        public LogServiceRequest()
        { }

        public LogServiceRequest(LogServiceAction serviceAction)
            : base(serviceAction)
        { }
    }
}
