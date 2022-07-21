using System;

namespace Database
{
    public abstract class StorageServiceRequest<TStorageServiceResponseResult> : ServiceRequest<TStorageServiceResponseResult>
        where TStorageServiceResponseResult : StorageServiceResponseResult
    {
        protected override Type GetServiceType() => typeof(StorageService);
    }
}
