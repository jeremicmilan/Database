namespace Database
{
    public class ServiceResponse : ServiceMessage<ServiceResponseStatus>
    {
        public ServiceResponse()
        { }

        public ServiceResponse(ServiceResponseStatus serviceAction)
            : base(serviceAction)
        { }
    }
}
