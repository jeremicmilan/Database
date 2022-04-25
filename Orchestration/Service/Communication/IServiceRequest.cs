namespace Database
{
    internal interface IServiceRequest
    {
        public abstract ServiceResponseResult Process();
    }
}