using System;

namespace Database
{
    public abstract class LogServiceRequest<TServiceResponseResult> : ServiceRequest<TServiceResponseResult>
        where TServiceResponseResult : ServiceResponseResult
    {
        protected override Type GetServiceType() => typeof(LogService);
    }
}
