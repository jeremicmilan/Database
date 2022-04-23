namespace Database
{
    public class StorageServiceRequest : ServiceRequest<StorageServiceAction>
    {
        public StorageServiceRequest()
        { }

        public StorageServiceRequest(StorageServiceAction serviceAction)
            : base(serviceAction)
        { }
    }
}
