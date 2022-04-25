namespace Database
{
    // We need the interface, as we cannot have a reference to a generic base class (of actual non-generic derived class)
    //
    public interface IServiceRequest
    {
        public abstract ServiceResponseResult Process();
    }
}