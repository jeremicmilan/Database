using System;

namespace Database
{
    public class ServiceResponseFailure : ServiceResponse
    {
        public Exception Exception { get; set; }

        public ServiceResponseFailure()
        { }

        public ServiceResponseFailure(Exception exception)
        {
            Exception = exception;
        }
    }
}
