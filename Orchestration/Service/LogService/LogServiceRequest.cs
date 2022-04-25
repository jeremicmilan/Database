namespace Database
{
    public abstract class LogServiceRequest<TServiceResponseResult> : ServiceRequest<TServiceResponseResult>
        where TServiceResponseResult : ServiceResponseResult
    {
        public override TServiceResponseResult Send()
        {
            return WriteToPipe(LogService.LogServicePipeName);
        }
    }
}
