namespace Database
{
    public abstract class DatabaseServiceRequest<TServiceResponseResult> : ServiceRequest<TServiceResponseResult>
        where TServiceResponseResult : ServiceResponseResult
    {
        public override TServiceResponseResult Send()
        {
            return WriteToPipe(DatabaseService.DatabaseServicePipeName);
        }
    }
}
