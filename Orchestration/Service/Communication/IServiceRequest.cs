namespace Database
{
    // We need the interface, as we cannot have a reference to a generic base class (of actual non-generic derived class)
    //
    public interface IServiceRequest
    {
        // To use TServiceResponseResult here we need https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/covariant-returns
        //
        public abstract ServiceResponseResult Process();
    }
}