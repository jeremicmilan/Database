using System;

namespace Database
{
    public abstract class LogServiceRequest<TLogServiceResponseResult> : ServiceRequest<TLogServiceResponseResult>
        where TLogServiceResponseResult : LogServiceResponseResult
    {
        protected override Type GetServiceType() => typeof(LogService);
    }
}
