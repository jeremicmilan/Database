using System;

namespace Database
{
    public abstract class ServiceMessage<TServiceAction>
        where TServiceAction : Enum
    {
        public TServiceAction ServiceAction { get; set; }

        public ServiceMessage()
        { }

        public ServiceMessage(TServiceAction serviceAction)
        {
            ServiceAction = serviceAction;
        }
    }
}
