using System;

namespace Database
{
    public abstract class DatabaseServiceRequest<TServiceResponseResult> : ServiceRequest<TServiceResponseResult>
        where TServiceResponseResult : ServiceResponseResult
    {
        protected override Type GetServiceType() => typeof(DatabaseService);
    }
}
