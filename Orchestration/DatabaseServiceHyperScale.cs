using System.Diagnostics;

namespace Database
{
    public class DatabaseServiceHyperScale : DatabaseService
    {
        public DatabaseServiceHyperScale(ServiceConfiguration serviceConfiguration = null)
            : base(serviceConfiguration)
        { }

        public override void SnapWindow()
        {
            Window.SnapTopRight(Process.GetCurrentProcess());
        }
    }
}
