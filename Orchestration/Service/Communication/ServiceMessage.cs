using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;

namespace Database
{
    public abstract class ServiceMessage
    {
        // We ahve to put the PipeClients in this base class as ServiceRequest class is generic
        // and will have multiple versions of this static field.
        //
        protected static readonly Dictionary<string, NamedPipeClientStream> PipeClients = new Dictionary<string, NamedPipeClientStream>();

        public ServiceMessage()
        { }

        public void WriteToPipeStream(PipeStream pipeStream)
        {
            StreamWriter streamWriter = new StreamWriter(pipeStream);
            streamWriter.WriteLine(Utility.Serialize(this));
            streamWriter.Flush();
        }
    }
}
