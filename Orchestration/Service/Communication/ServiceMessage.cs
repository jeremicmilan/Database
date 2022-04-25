using System.IO;
using System.IO.Pipes;

namespace Database
{
    public abstract class ServiceMessage
    {
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
