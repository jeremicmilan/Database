namespace Database
{
    public class ServiceResponseFailure : ServiceResponse
    {
        public string ExceptionMessage { get; set; }

        public ServiceResponseFailure()
        { }

        public ServiceResponseFailure(string exceptionMessage)
        {
            ExceptionMessage = exceptionMessage;
        }
    }
}
