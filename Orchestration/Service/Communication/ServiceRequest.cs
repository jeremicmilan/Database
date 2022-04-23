using System;

namespace Database
{
    public class ServiceRequest<TServiceAction> : ServiceMessage<TServiceAction>
        where TServiceAction : Enum
    {
        public ServiceRequest()
        { }

        public ServiceRequest(TServiceAction serviceAction)
            : base(serviceAction)
        { }
    }
}
