namespace Database
{
    public abstract class StorageServiceRequest<TServiceResponseResult> : ServiceRequest<TServiceResponseResult>
        where TServiceResponseResult : ServiceResponseResult
    {
        public override TServiceResponseResult Send()
        {
            return WriteToPipe(StorageService.StorageServicePipeName);
        }

        protected StorageService StorageService => StorageService.Get();
    }
}
