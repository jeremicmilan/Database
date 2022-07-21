using System;

namespace Database
{
    public abstract class DatabaseServiceRequest<TDatabaseServiceResponseResult> : ServiceRequest<TDatabaseServiceResponseResult>
        where TDatabaseServiceResponseResult : DatabaseServiceResponseResult
    {
        protected override Type GetServiceType() => typeof(DatabaseService);
    }
}
