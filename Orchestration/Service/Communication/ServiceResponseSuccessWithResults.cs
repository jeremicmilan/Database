namespace Database
{
    public class ServiceResponseSuccessWithResults<TServiceResponseResult> : ServiceResponse
        where TServiceResponseResult : ServiceResponseResult
    {
        public TServiceResponseResult ServiceResponseResult { get; set; }

        public ServiceResponseSuccessWithResults()
        { }

        public ServiceResponseSuccessWithResults(TServiceResponseResult serviceResponseResult)
        {
            ServiceResponseResult = serviceResponseResult;
        }
    }
}
