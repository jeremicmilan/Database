using System;

namespace Database
{
    public abstract class StorageServiceRequest<TServiceResponseResult> : ServiceRequest<TServiceResponseResult>
        where TServiceResponseResult : ServiceResponseResult
    {
        protected override Type GetServiceType() => typeof(StorageService);
    }
}
